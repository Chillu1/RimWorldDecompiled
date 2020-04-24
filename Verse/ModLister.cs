using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse.Steam;

namespace Verse
{
	public static class ModLister
	{
		private static List<ModMetaData> mods;

		private static bool royaltyInstalled;

		private static bool modListBuilt;

		private static bool rebuildingModList;

		private static bool nestedRebuildInProgress;

		private static List<ExpansionDef> AllExpansionsCached;

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
			WorkshopItems.EnsureInit();
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
			RecacheRoyaltyInstalled();
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
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].SamePackageId(identifier, ignorePostfix))
				{
					return mods[i];
				}
			}
			return null;
		}

		public static ModMetaData GetActiveModWithIdentifier(string identifier)
		{
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].SamePackageId(identifier, ignorePostfix: true) && mods[i].Active)
				{
					return mods[i];
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
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].Active && mods[i].Name == name)
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

		private static void RecacheRoyaltyInstalled()
		{
			for (int i = 0; i < mods.Count; i++)
			{
				if (mods[i].SamePackageId(ModContentPack.RoyaltyModPackageId))
				{
					royaltyInstalled = true;
					return;
				}
			}
			royaltyInstalled = false;
		}

		private static bool TryAddMod(ModMetaData mod)
		{
			if (mod.Official && !mod.IsCoreMod && SteamManager.Initialized && mod.SteamAppId != 0)
			{
				bool flag = true;
				try
				{
					flag = SteamApps.BIsDlcInstalled(new AppId_t((uint)mod.SteamAppId));
				}
				catch (Exception arg)
				{
					Log.Error("Could not determine if a DLC is installed: " + arg);
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
						ModMetaData modMetaData = mod.OnSteamWorkshop ? mod : modWithIdentifier;
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
			return true;
		}
	}
}
