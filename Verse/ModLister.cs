using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RimWorld;
using Steamworks;
using Verse.Steam;

namespace Verse;

public static class ModLister
{
	private static List<ModMetaData> mods;

	private static Dictionary<string, List<ModMetaData>> modsByPackageId;

	private static Dictionary<string, List<ModMetaData>> modsByPackageIdIgnorePostfix;

	private static bool modListBuilt;

	private static bool rebuildingModList;

	private static bool nestedRebuildInProgress;

	private static List<ExpansionDef> AllExpansionsCached;

	private static bool royaltyInstalled;

	private static bool ideologyInstalled;

	private static bool biotechInstalled;

	private static bool anomalyInstalled;

	private static bool odysseyInstalled;

	public static IEnumerable<ModMetaData> AllInstalledMods => mods;

	public static IEnumerable<DirectoryInfo> AllActiveModDirs => from mod in mods
		where mod.Active
		select mod.RootDir;

	public static List<ExpansionDef> AllExpansions
	{
		get
		{
			if (AllExpansionsCached.NullOrEmpty())
			{
				AllExpansionsCached = DefDatabase<ExpansionDef>.AllDefsListForReading.Where((ExpansionDef e) => GetModWithIdentifier(e.linkedMod)?.Official ?? true).ToList();
			}
			return AllExpansionsCached;
		}
	}

	public static bool RoyaltyInstalled => royaltyInstalled;

	public static bool IdeologyInstalled => ideologyInstalled;

	public static bool BiotechInstalled => biotechInstalled;

	public static bool AnomalyInstalled => anomalyInstalled;

	public static bool OdysseyInstalled => odysseyInstalled;

	public static bool ShouldLogIssues
	{
		get
		{
			if (!modListBuilt)
			{
				return !nestedRebuildInProgress;
			}
			return false;
		}
	}

	static ModLister()
	{
		mods = new List<ModMetaData>();
		modsByPackageId = new Dictionary<string, List<ModMetaData>>(StringComparer.CurrentCultureIgnoreCase);
		modsByPackageIdIgnorePostfix = new Dictionary<string, List<ModMetaData>>(StringComparer.CurrentCultureIgnoreCase);
		RebuildModList();
		modListBuilt = true;
	}

	public static void EnsureInit()
	{
	}

	public static void RebuildModList()
	{
		nestedRebuildInProgress = rebuildingModList;
		rebuildingModList = true;
		string s = "Rebuilding mods list";
		mods.Clear();
		modsByPackageId.Clear();
		modsByPackageIdIgnorePostfix.Clear();
		WorkshopItems.EnsureInit();
		DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
		s += "\nAdding official mods from content folder:";
		foreach (string item in from d in new DirectoryInfo(GenFilePaths.OfficialModsFolderPath).GetDirectories()
			select d.FullName)
		{
			ModMetaData modMetaData = new ModMetaData(item, official: true);
			if (TryAddMod(modMetaData))
			{
				s = s + "\n  Adding " + modMetaData.ToStringLong();
			}
		}
		s += "\nAdding mods from mods folder:";
		foreach (string item2 in from d in new DirectoryInfo(GenFilePaths.ModsFolderPath).GetDirectories()
			select d.FullName)
		{
			ModMetaData modMetaData2 = new ModMetaData(item2);
			if (TryAddMod(modMetaData2))
			{
				s = s + "\n  Adding " + modMetaData2.ToStringLong();
			}
		}
		s += "\nAdding mods from Steam:";
		foreach (WorkshopItem item3 in WorkshopItems.AllSubscribedItems.Where((WorkshopItem it) => it is WorkshopItem_Mod))
		{
			ModMetaData modMetaData3 = new ModMetaData(item3);
			if (TryAddMod(modMetaData3))
			{
				s = s + "\n  Adding " + modMetaData3.ToStringLong();
			}
		}
		s += "\nDeactivating not-installed mods:";
		ModsConfig.DeactivateNotInstalledMods(delegate(string log)
		{
			s = s + "\n   " + log;
		});
		if (mods.Count((ModMetaData m) => m.Active) == 0)
		{
			s += "\nThere are no active mods. Activating Core mod.";
			mods.First((ModMetaData m) => m.IsCoreMod).Active = true;
		}
		RecacheExpansionsInstalled();
		if (Prefs.LogVerbose)
		{
			Log.Message(s);
		}
		rebuildingModList = false;
		nestedRebuildInProgress = false;
	}

	public static int InstalledModsListHash(bool activeOnly)
	{
		int num = 17;
		int num2 = 0;
		foreach (ModMetaData item in ModsConfig.ActiveModsInLoadOrder)
		{
			if (!activeOnly || ModsConfig.IsActive(item.PackageId))
			{
				num = num * 31 + item.GetHashCode();
				num = num * 31 + num2 * 2654241;
				num2++;
			}
		}
		return num;
	}

	public static ModMetaData GetModWithIdentifier(string identifier, bool ignorePostfix = false)
	{
		if (ignorePostfix)
		{
			if (!modsByPackageIdIgnorePostfix.ContainsKey(identifier))
			{
				return null;
			}
			return modsByPackageIdIgnorePostfix[identifier].ElementAtOrDefault(0);
		}
		if (!modsByPackageId.ContainsKey(identifier))
		{
			return null;
		}
		return modsByPackageId[identifier].ElementAtOrDefault(0);
	}

	public static ModMetaData GetActiveModWithIdentifier(string identifier, bool ignorePostfix = false)
	{
		if (!(ignorePostfix ? modsByPackageIdIgnorePostfix : modsByPackageId).TryGetValue(identifier.ToLowerInvariant().Trim(), out var value))
		{
			return null;
		}
		foreach (ModMetaData item in value)
		{
			if (item.Active)
			{
				return item;
			}
		}
		return null;
	}

	public static ExpansionDef GetExpansionWithIdentifier(string packageId)
	{
		for (int i = 0; i < AllExpansions.Count; i++)
		{
			if (AllExpansions[i].linkedMod == packageId)
			{
				return AllExpansions[i];
			}
		}
		return null;
	}

	public static bool HasActiveModWithName(string name)
	{
		foreach (ModMetaData mod in mods)
		{
			if (mod.Active && mod.Name == name)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyFromListActive(List<string> mods)
	{
		foreach (string mod in mods)
		{
			if (GetActiveModWithIdentifier(mod) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyModActiveNoSuffix(List<string> modIds)
	{
		foreach (string modId in modIds)
		{
			if (GetActiveModWithIdentifier(modId.Trim(), ignorePostfix: true) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AnyModActiveNoSuffix(IEnumerable<string> modIds)
	{
		foreach (string modId in modIds)
		{
			if (GetActiveModWithIdentifier(modId.Trim(), ignorePostfix: true) != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool AllModsActiveNoSuffix(List<string> modIds)
	{
		foreach (string modId in modIds)
		{
			if (GetActiveModWithIdentifier(modId.Trim(), ignorePostfix: true) == null)
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllModsActiveNoSuffix(IEnumerable<string> modIds)
	{
		foreach (string modId in modIds)
		{
			if (GetActiveModWithIdentifier(modId.Trim(), ignorePostfix: true) == null)
			{
				return false;
			}
		}
		return true;
	}

	private static void RecacheExpansionsInstalled()
	{
		royaltyInstalled = modsByPackageId.ContainsKey("ludeon.rimworld.royalty");
		ideologyInstalled = modsByPackageId.ContainsKey("ludeon.rimworld.ideology");
		biotechInstalled = modsByPackageId.ContainsKey("ludeon.rimworld.biotech");
		anomalyInstalled = modsByPackageId.ContainsKey("ludeon.rimworld.anomaly");
		odysseyInstalled = modsByPackageId.ContainsKey("ludeon.rimworld.odyssey");
	}

	private static bool TryAddMod(ModMetaData mod)
	{
		if (mod.Official && !mod.IsCoreMod && SteamManager.Initialized && mod.SteamAppId != 0)
		{
			bool flag = true;
			try
			{
				flag = SteamApps.BIsDlcInstalled(new AppId_t(mod.SteamAppId));
			}
			catch (Exception ex)
			{
				Log.Error("Could not determine if a DLC is installed: " + ex);
			}
			if (!flag)
			{
				return false;
			}
		}
		ModMetaData modWithIdentifier = GetModWithIdentifier(mod.PackageId);
		if (modWithIdentifier != null)
		{
			if (mod.RootDir.FullName != modWithIdentifier.RootDir.FullName)
			{
				if (mod.OnSteamWorkshop != modWithIdentifier.OnSteamWorkshop)
				{
					ModMetaData modMetaData = (mod.OnSteamWorkshop ? mod : modWithIdentifier);
					if (!modMetaData.appendPackageIdSteamPostfix)
					{
						modMetaData.appendPackageIdSteamPostfix = true;
						return TryAddMod(mod);
					}
				}
				Log.Error("Tried loading mod with the same packageId multiple times: " + mod.PackageIdPlayerFacing + ". Ignoring the duplicates.\n" + mod.RootDir.FullName + "\n" + modWithIdentifier.RootDir.FullName);
				return false;
			}
			return false;
		}
		mods.Add(mod);
		if (modsByPackageId.ContainsKey(mod.PackageId))
		{
			modsByPackageId[mod.PackageId].Add(mod);
		}
		else
		{
			modsByPackageId.Add(mod.PackageId, new List<ModMetaData> { mod });
		}
		if (modsByPackageIdIgnorePostfix.ContainsKey(mod.packageIdLowerCase))
		{
			modsByPackageIdIgnorePostfix[mod.packageIdLowerCase].Add(mod);
		}
		else
		{
			modsByPackageIdIgnorePostfix.Add(mod.packageIdLowerCase, new List<ModMetaData> { mod });
		}
		return true;
	}

	private static bool CheckDLC(bool dlc, string featureName, string dlcNameIndef, string installedPropertyName)
	{
		if (!dlc)
		{
			Log.ErrorOnce(featureName + " is " + dlcNameIndef + "-specific game system. If you want to use this code please check ModLister." + installedPropertyName + " before calling it.", featureName.GetHashCode());
		}
		return dlc;
	}

	public static bool CheckRoyalty(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled, featureNameSingular, "a Royalty", "RoyaltyInstalled");
	}

	public static bool CheckIdeology(string featureNameSingular)
	{
		return CheckDLC(IdeologyInstalled, featureNameSingular, "an Ideology", "IdeologyInstalled");
	}

	public static bool CheckBiotech(string featureNameSingular)
	{
		return CheckDLC(BiotechInstalled, featureNameSingular, "a Biotech", "BiotechInstalled");
	}

	public static bool CheckAnomaly(string featureNameSingular)
	{
		return CheckDLC(AnomalyInstalled, featureNameSingular, "an Anomaly", "AnomalyInstalled");
	}

	public static bool CheckOdyssey(string featureNameSingular)
	{
		return CheckDLC(OdysseyInstalled, featureNameSingular, "an Odyssey", "OdysseyInstalled");
	}

	public static bool CheckIdeologyOrBiotech(string featureNameSingular)
	{
		return CheckDLC(IdeologyInstalled || BiotechInstalled, featureNameSingular, "a Ideology or Biotech", "IdeologyInstalled or BiotechInstalled");
	}

	public static bool CheckRoyaltyAndIdeology(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled && IdeologyInstalled, featureNameSingular, "a Royalty and Ideology", "RoyaltyInstalled and IdeologyInstalled");
	}

	public static bool CheckRoyaltyOrIdeology(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || IdeologyInstalled, featureNameSingular, "a Royalty or Ideology", "RoyaltyInstalled or IdeologyInstalled");
	}

	public static bool CheckRoyaltyOrBiotech(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || BiotechInstalled, featureNameSingular, "a Royalty or Biotech", "RoyaltyInstalled or BiotechInstalled");
	}

	public static bool CheckRoyaltyOrAnomaly(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || AnomalyInstalled, featureNameSingular, "a Royalty or Anomaly", "RoyaltyInstalled or AnomalyInstalled");
	}

	public static bool CheckBiotechOrAnomaly(string featureNameSingular)
	{
		return CheckDLC(BiotechInstalled || AnomalyInstalled, featureNameSingular, "a Biotech or Anomaly", "BiotechInstalled or AnomalyInstalled");
	}

	public static bool CheckRoyaltyOrOdyssey(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || OdysseyInstalled, featureNameSingular, "a Royalty or Odyssey", "RoyaltyInstalled or OdysseyInstalled");
	}

	public static bool CheckRoyaltyOrIdeologyOrBiotech(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || BiotechInstalled || IdeologyInstalled, featureNameSingular, "a Royalty or Ideology or Biotech", "RoyaltyInstalled or IdeologyInstalled or BiotechInstalled");
	}

	public static bool CheckBiotechOrAnomalyOrOdyssey(string featureNameSingular)
	{
		return CheckDLC(BiotechInstalled || AnomalyInstalled || OdysseyInstalled, featureNameSingular, "a Biotech or Anomaly or Odyssey", "BiotechInstalled or AnomalyInstalled or OdysseyInstalled");
	}

	public static bool CheckAnyExpansion(string featureNameSingular)
	{
		return CheckDLC(RoyaltyInstalled || IdeologyInstalled || BiotechInstalled || AnomalyInstalled || OdysseyInstalled, featureNameSingular, "a Royalty or Ideology or Biotech or Anomaly or Odyssey", "RoyaltyInstalled or IdeologyInstalled or BiotechInstalled or AnomalyInstalled or OdysseyInstalled");
	}
}
