using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class ModsConfig
{
	private class ModsConfigData
	{
		[LoadAlias("buildNumber")]
		public string version;

		public List<string> activeMods = new List<string>();

		public List<string> knownExpansions = new List<string>();
	}

	private static ModsConfigData data;

	private static bool royaltyActive;

	private static bool ideologyActive;

	private static bool biotechActive;

	private static bool anomalyActive;

	private static bool odysseyActive;

	private static HashSet<string> activeModsHashSet;

	private static List<ModMetaData> activeModsInLoadOrderCached;

	private static bool activeModsInLoadOrderCachedDirty;

	private static List<string> newKnownExpansions;

	private static readonly string Core;

	private static readonly List<string> ExpansionsInReleaseOrder;

	public static IEnumerable<ModMetaData> ActiveModsInLoadOrder
	{
		get
		{
			ModLister.EnsureInit();
			if (activeModsInLoadOrderCachedDirty)
			{
				activeModsInLoadOrderCached.Clear();
				for (int i = 0; i < data.activeMods.Count; i++)
				{
					activeModsInLoadOrderCached.Add(ModLister.GetModWithIdentifier(data.activeMods[i]));
				}
				activeModsInLoadOrderCachedDirty = false;
			}
			return activeModsInLoadOrderCached;
		}
	}

	public static ExpansionDef LastInstalledExpansion
	{
		get
		{
			for (int num = data.knownExpansions.Count - 1; num >= 0; num--)
			{
				ExpansionDef expansionWithIdentifier = ModLister.GetExpansionWithIdentifier(data.knownExpansions[num]);
				if (expansionWithIdentifier != null && !expansionWithIdentifier.isCore && expansionWithIdentifier.Status != ExpansionStatus.NotInstalled)
				{
					return expansionWithIdentifier;
				}
			}
			return null;
		}
	}

	public static bool RoyaltyActive => royaltyActive;

	public static bool IdeologyActive => ideologyActive;

	public static bool BiotechActive => biotechActive;

	public static bool AnomalyActive => anomalyActive;

	public static bool OdysseyActive => odysseyActive;

	static ModsConfig()
	{
		activeModsHashSet = new HashSet<string>();
		activeModsInLoadOrderCached = new List<ModMetaData>();
		newKnownExpansions = new List<string>();
		Core = "ludeon.rimworld";
		ExpansionsInReleaseOrder = new List<string> { "ludeon.rimworld.royalty", "ludeon.rimworld.ideology", "ludeon.rimworld.biotech", "ludeon.rimworld.anomaly", "ludeon.rimworld.odyssey" };
		bool flag = false;
		bool flag2 = false;
		data = DirectXmlLoader.ItemFromXmlFile<ModsConfigData>(GenFilePaths.ModsConfigFilePath);
		if (data.version != null)
		{
			bool flag3 = false;
			int result;
			if (data.version.Contains("."))
			{
				int num = VersionControl.MinorFromVersionString(data.version);
				if (VersionControl.MajorFromVersionString(data.version) != VersionControl.CurrentMajor || num != VersionControl.CurrentMinor)
				{
					flag3 = true;
				}
			}
			else if (data.version.Length > 0 && data.version.All(char.IsNumber) && int.TryParse(data.version, out result) && result <= 2009)
			{
				flag3 = true;
			}
			if (flag3)
			{
				Log.Message("Mods config data is from version " + data.version + " while we are running " + VersionControl.CurrentVersionStringWithRev + ". Resetting.");
				data = new ModsConfigData();
				flag = true;
			}
		}
		for (int i = 0; i < data.activeMods.Count; i++)
		{
			string packageId = data.activeMods[i];
			if (ModLister.GetModWithIdentifier(packageId) == null)
			{
				ModMetaData modMetaData = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData m) => m.FolderName == packageId);
				if (modMetaData != null)
				{
					data.activeMods[i] = modMetaData.PackageId;
					flag2 = true;
				}
				if (TryGetPackageIdWithoutExtraSteamPostfix(packageId, out var nonSteamPackageId) && ModLister.GetModWithIdentifier(nonSteamPackageId) != null)
				{
					data.activeMods[i] = nonSteamPackageId;
				}
			}
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ModMetaData allInstalledMod in ModLister.AllInstalledMods)
		{
			if (allInstalledMod.Active)
			{
				if (hashSet.Contains(allInstalledMod.PackageIdNonUnique))
				{
					allInstalledMod.Active = false;
					Log.Warning("There was more than one enabled instance of mod with PackageID: " + allInstalledMod.PackageIdNonUnique + ". Disabling the duplicates.");
					continue;
				}
				hashSet.Add(allInstalledMod.PackageIdNonUnique);
			}
			if (allInstalledMod.IsCoreMod || !allInstalledMod.Official || !IsExpansionNew(allInstalledMod.PackageId))
			{
				continue;
			}
			SetActive(allInstalledMod.PackageId, active: true);
			AddKnownExpansion(allInstalledMod.PackageId);
			int y = data.activeMods.IndexOf(allInstalledMod.PackageId);
			string errorMessage;
			if (!allInstalledMod.ForceLoadAfter.NullOrEmpty())
			{
				foreach (string item in allInstalledMod.ForceLoadAfter)
				{
					ModMetaData activeModWithIdentifier = ModLister.GetActiveModWithIdentifier(item);
					if (activeModWithIdentifier != null)
					{
						int x = data.activeMods.IndexOf(activeModWithIdentifier.PackageId);
						if (x != -1 && x > y)
						{
							TryReorder(y, x, out errorMessage);
							Gen.Swap(ref x, ref y);
						}
					}
				}
			}
			if (!allInstalledMod.ForceLoadBefore.NullOrEmpty())
			{
				foreach (string item2 in allInstalledMod.ForceLoadBefore)
				{
					ModMetaData activeModWithIdentifier2 = ModLister.GetActiveModWithIdentifier(item2);
					if (activeModWithIdentifier2 != null)
					{
						int x2 = data.activeMods.IndexOf(activeModWithIdentifier2.PackageId);
						if (x2 != -1 && x2 < y)
						{
							TryReorder(y, x2, out errorMessage);
							Gen.Swap(ref x2, ref y);
						}
					}
				}
			}
			Prefs.Notify_NewExpansion();
			flag2 = true;
		}
		if (newKnownExpansions.Any())
		{
			newKnownExpansions.SortBy((string item) => ExpansionsInReleaseOrder.IndexOf(item));
			data.knownExpansions.AddRange(newKnownExpansions);
			newKnownExpansions.Clear();
		}
		if (!File.Exists(GenFilePaths.ModsConfigFilePath) || flag)
		{
			Reset();
		}
		else if (flag2)
		{
			Save();
		}
		RecacheActiveMods();
	}

	public static bool TryGetPackageIdWithoutExtraSteamPostfix(string packageId, out string nonSteamPackageId)
	{
		if (packageId.EndsWith(ModMetaData.SteamModPostfix))
		{
			nonSteamPackageId = packageId.Substring(0, packageId.Length - ModMetaData.SteamModPostfix.Length);
			return true;
		}
		nonSteamPackageId = null;
		return false;
	}

	public static void DeactivateNotInstalledMods(Action<string> logCallback = null)
	{
		for (int num = data.activeMods.Count - 1; num >= 0; num--)
		{
			ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(data.activeMods[num]);
			if (modWithIdentifier == null && TryGetPackageIdWithoutExtraSteamPostfix(data.activeMods[num], out var nonSteamPackageId))
			{
				modWithIdentifier = ModLister.GetModWithIdentifier(nonSteamPackageId);
			}
			if (modWithIdentifier == null)
			{
				logCallback?.Invoke("Deactivating " + data.activeMods[num]);
				data.activeMods.RemoveAt(num);
			}
		}
		RecacheActiveMods();
	}

	public static void Reset()
	{
		data.activeMods.Clear();
		data.activeMods.Add("ludeon.rimworld");
		foreach (ModMetaData allInstalledMod in ModLister.AllInstalledMods)
		{
			if (allInstalledMod.Official && !allInstalledMod.IsCoreMod && allInstalledMod.VersionCompatible)
			{
				data.activeMods.Add(allInstalledMod.PackageId);
			}
		}
		activeModsInLoadOrderCachedDirty = true;
		TrySortMods();
		Save();
		RecacheActiveMods();
	}

	public static bool TryReorder(int modIndex, int newIndex, out string errorMessage)
	{
		errorMessage = null;
		if (modIndex == newIndex)
		{
			return false;
		}
		if (ReorderConflict(modIndex, newIndex, out errorMessage))
		{
			return false;
		}
		data.activeMods.Insert(newIndex, data.activeMods[modIndex]);
		data.activeMods.RemoveAt((modIndex < newIndex) ? modIndex : (modIndex + 1));
		activeModsInLoadOrderCachedDirty = true;
		return true;
	}

	private static bool ReorderConflict(int modIndex, int newIndex, out string errorMessage)
	{
		errorMessage = null;
		ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(data.activeMods[modIndex]);
		if (modWithIdentifier.IsCoreMod)
		{
			foreach (string item in ExpansionsInReleaseOrder)
			{
				int num = data.activeMods.IndexOf(item);
				if (num != -1 && num < newIndex)
				{
					errorMessage = "ModReorderConflict_MustLoadBefore".Translate(modWithIdentifier.Name, ModLister.GetModWithIdentifier(item).Name);
					return true;
				}
			}
		}
		if (modWithIdentifier.Source == ContentSource.OfficialModsFolder && data.activeMods.IndexOf(Core) >= newIndex)
		{
			errorMessage = "ModReorderConflict_MustLoadAfter".Translate(modWithIdentifier.Name, ModLister.GetModWithIdentifier(Core).Name);
			return true;
		}
		if (!modWithIdentifier.ForceLoadBefore.NullOrEmpty())
		{
			foreach (string item2 in modWithIdentifier.ForceLoadBefore)
			{
				ModMetaData modWithIdentifier2 = ModLister.GetModWithIdentifier(item2);
				if (modWithIdentifier2 == null)
				{
					continue;
				}
				for (int num2 = newIndex - 1; num2 >= 0; num2--)
				{
					if (modWithIdentifier2.SamePackageId(data.activeMods[num2]))
					{
						errorMessage = "ModReorderConflict_MustLoadBefore".Translate(modWithIdentifier.Name, ModLister.GetModWithIdentifier(data.activeMods[num2]).Name);
						return true;
					}
				}
			}
		}
		if (!modWithIdentifier.ForceLoadAfter.NullOrEmpty())
		{
			foreach (string item3 in modWithIdentifier.ForceLoadAfter)
			{
				ModMetaData modWithIdentifier3 = ModLister.GetModWithIdentifier(item3);
				if (modWithIdentifier3 == null)
				{
					continue;
				}
				for (int i = newIndex; i < data.activeMods.Count; i++)
				{
					if (modWithIdentifier3.SamePackageId(data.activeMods[i]))
					{
						errorMessage = "ModReorderConflict_MustLoadAfter".Translate(modWithIdentifier.Name, ModLister.GetModWithIdentifier(data.activeMods[i]).Name);
						return true;
					}
				}
			}
		}
		return false;
	}

	public static void Reorder(List<int> newIndices)
	{
		List<string> list = new List<string>();
		foreach (int newIndex in newIndices)
		{
			list.Add(data.activeMods[newIndex]);
		}
		data.activeMods = list;
		activeModsInLoadOrderCachedDirty = true;
	}

	public static bool IsActive(ModMetaData mod)
	{
		return IsActive(mod.PackageId);
	}

	[Obsolete("Callers should use ModLister.AllModsActiveNoSuffix instead which automatically trims _steam suffixes.")]
	public static bool AreAllActive(string mods)
	{
		if (mods != null && mods.Contains(','))
		{
			return AreAllActive(mods.ToLower().Split(','));
		}
		return IsActive(mods);
	}

	[Obsolete("Callers should use ModLister.AllModsActiveNoSuffix instead which automatically trims _steam suffixes.")]
	public static bool AreAllActive(IEnumerable<string> mods)
	{
		foreach (string mod in mods)
		{
			if (!IsActive(mod.Trim()))
			{
				return false;
			}
		}
		return true;
	}

	[Obsolete("Callers should use ModLister.AnyModActiveNoSuffix instead which automatically trims _steam suffixes.")]
	public static bool IsAnyActiveOrEmpty(IEnumerable<string> mods, bool trimNames = false)
	{
		if (!mods.EnumerableNullOrEmpty())
		{
			foreach (string mod in mods)
			{
				if (IsActive(trimNames ? mod.Trim() : mod))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static bool IsActive(string id)
	{
		return activeModsHashSet.Contains(id.ToLower());
	}

	public static void SetActive(ModMetaData mod, bool active)
	{
		SetActive(mod.PackageId, active);
	}

	public static void SetActive(string modIdentifier, bool active)
	{
		string item = modIdentifier.ToLower();
		if (active)
		{
			if (!data.activeMods.Contains(item))
			{
				data.activeMods.Add(item);
				EnsureModAdheresToForcedLoadOrder(modIdentifier);
			}
		}
		else if (data.activeMods.Contains(item))
		{
			data.activeMods.Remove(item);
		}
		RecacheActiveMods();
	}

	public static void EnsureModAdheresToForcedLoadOrder(string modIdentifier)
	{
		ModMetaData modWithIdentifier = ModLister.GetModWithIdentifier(modIdentifier);
		if (modWithIdentifier == null)
		{
			return;
		}
		string item = modIdentifier.ToLower();
		if (!data.activeMods.Contains(item))
		{
			return;
		}
		int? num = null;
		int num2 = data.activeMods.IndexOf(item);
		if (!modWithIdentifier.ForceLoadAfter.NullOrEmpty())
		{
			foreach (string item2 in modWithIdentifier.ForceLoadAfter)
			{
				int num3 = data.activeMods.IndexOf(item2.ToLower());
				if (num3 != -1)
				{
					num = Mathf.Max(num ?? int.MinValue, num3 + 1);
				}
			}
		}
		if (!modWithIdentifier.ForceLoadBefore.NullOrEmpty())
		{
			foreach (string item3 in modWithIdentifier.ForceLoadBefore)
			{
				int num4 = data.activeMods.IndexOf(item3.ToLower());
				if (num4 != -1)
				{
					num = Mathf.Min(num ?? int.MaxValue, num4);
				}
			}
		}
		if (num.HasValue && num.Value != num2)
		{
			data.activeMods.Remove(item);
			data.activeMods.Insert(num.Value, item);
		}
	}

	public static void SetActiveToList(List<string> mods)
	{
		data.activeMods = mods.Where((string mod) => ModLister.GetModWithIdentifier(mod) != null).ToList();
		RecacheActiveMods();
	}

	public static bool IsExpansionNew(string id)
	{
		return !data.knownExpansions.Contains(id.ToLower());
	}

	public static void AddKnownExpansion(string id)
	{
		if (!IsExpansionNew(id))
		{
			Log.Error("Tried to add already known expansion: " + id);
		}
		else
		{
			newKnownExpansions.Add(id.ToLower());
		}
	}

	public static void Save()
	{
		data.version = VersionControl.CurrentVersionStringWithRev;
		DirectXmlSaver.SaveDataObject(data, GenFilePaths.ModsConfigFilePath);
	}

	public static void SaveFromList(List<string> mods)
	{
		DirectXmlSaver.SaveDataObject(new ModsConfigData
		{
			version = VersionControl.CurrentVersionStringWithRev,
			activeMods = mods,
			knownExpansions = data.knownExpansions
		}, GenFilePaths.ModsConfigFilePath);
	}

	public static void RestartFromChangedMods()
	{
		Find.WindowStack.Add(new Dialog_MessageBox("ModsChanged".Translate(), null, delegate
		{
			GenCommandLine.Restart();
		}));
	}

	public static Dictionary<string, string> GetModWarnings()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		List<ModMetaData> mods = ActiveModsInLoadOrder.ToList();
		for (int i = 0; i < mods.Count; i++)
		{
			int index = i;
			ModMetaData modMetaData = mods[index];
			StringBuilder stringBuilder = new StringBuilder("");
			ModMetaData activeModWithIdentifier = ModLister.GetActiveModWithIdentifier(modMetaData.PackageId);
			if (activeModWithIdentifier != null && modMetaData != activeModWithIdentifier)
			{
				stringBuilder.AppendLine("ModWithSameIdAlreadyActive".Translate(activeModWithIdentifier.Name));
			}
			List<string> list = FindConflicts(modMetaData.IncompatibleWith, null);
			if (list.Any())
			{
				stringBuilder.AppendLine("ModIncompatibleWithTip".Translate(list.ToCommaList(useAnd: true)));
			}
			List<string> list2 = FindConflicts(modMetaData.LoadBefore, (ModMetaData beforeMod) => mods.IndexOf(beforeMod) < index);
			if (list2.Any())
			{
				stringBuilder.AppendLine("ModMustLoadBefore".Translate(list2.ToCommaList(useAnd: true)));
			}
			List<string> list3 = FindConflicts(modMetaData.ForceLoadBefore, (ModMetaData beforeMod) => mods.IndexOf(beforeMod) < index);
			if (list3.Any())
			{
				stringBuilder.AppendLine("ModMustLoadBefore".Translate(list3.ToCommaList(useAnd: true)));
			}
			List<string> list4 = FindConflicts(modMetaData.LoadAfter, (ModMetaData afterMod) => mods.IndexOf(afterMod) > index);
			if (list4.Any())
			{
				stringBuilder.AppendLine("ModMustLoadAfter".Translate(list4.ToCommaList(useAnd: true)));
			}
			List<string> list5 = FindConflicts(modMetaData.ForceLoadAfter, (ModMetaData afterMod) => mods.IndexOf(afterMod) > index);
			if (list5.Any())
			{
				stringBuilder.AppendLine("ModMustLoadAfter".Translate(list5.ToCommaList(useAnd: true)));
			}
			if (modMetaData.Dependencies.Any())
			{
				List<string> list6 = modMetaData.UnsatisfiedDependencies();
				if (list6.Any())
				{
					stringBuilder.AppendLine("ModUnsatisfiedDependency".Translate(list6.ToCommaList(useAnd: true)));
				}
			}
			dictionary.Add(modMetaData.PackageId, stringBuilder.ToString().TrimEndNewlines());
		}
		return dictionary;
	}

	public static bool ModHasAnyOrderingIssues(ModMetaData mod)
	{
		List<ModMetaData> mods = ActiveModsInLoadOrder.ToList();
		int index = mods.IndexOf(mod);
		if (index == -1)
		{
			return false;
		}
		if (FindConflicts(mod.LoadBefore, (ModMetaData beforeMod) => mods.IndexOf(beforeMod) < index).Count > 0)
		{
			return true;
		}
		if (FindConflicts(mod.LoadAfter, (ModMetaData afterMod) => mods.IndexOf(afterMod) > index).Count > 0)
		{
			return true;
		}
		return false;
	}

	private static List<string> FindConflicts(List<string> modsToCheck, Func<ModMetaData, bool> predicate)
	{
		List<string> list = new List<string>();
		foreach (string item in modsToCheck)
		{
			ModMetaData activeModWithIdentifier = ModLister.GetActiveModWithIdentifier(item, ignorePostfix: true);
			if (activeModWithIdentifier != null && (predicate == null || predicate(activeModWithIdentifier)))
			{
				list.Add(activeModWithIdentifier.Name);
			}
		}
		return list;
	}

	public static void TrySortMods()
	{
		List<ModMetaData> list = ActiveModsInLoadOrder.ToList();
		DirectedAcyclicGraph directedAcyclicGraph = new DirectedAcyclicGraph(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			ModMetaData modMetaData = list[i];
			foreach (string before in modMetaData.LoadBefore.Concat(modMetaData.ForceLoadBefore))
			{
				ModMetaData modMetaData2 = list.FirstOrDefault((ModMetaData m) => m.SamePackageId(before, ignorePostfix: true));
				if (modMetaData2 != null)
				{
					directedAcyclicGraph.AddEdge(list.IndexOf(modMetaData2), i);
				}
			}
			foreach (string after in modMetaData.LoadAfter.Concat(modMetaData.ForceLoadAfter))
			{
				ModMetaData modMetaData3 = list.FirstOrDefault((ModMetaData m) => m.SamePackageId(after, ignorePostfix: true));
				if (modMetaData3 != null)
				{
					directedAcyclicGraph.AddEdge(i, list.IndexOf(modMetaData3));
				}
			}
		}
		int num = directedAcyclicGraph.FindCycle();
		if (num != -1)
		{
			Find.WindowStack.Add(new Dialog_MessageBox("ModCyclicDependency".Translate(list[num].Name)));
		}
		else
		{
			Reorder(directedAcyclicGraph.TopologicalSort());
		}
	}

	private static void RecacheActiveMods()
	{
		activeModsHashSet.Clear();
		foreach (string activeMod in data.activeMods)
		{
			activeModsHashSet.Add(activeMod);
		}
		royaltyActive = IsActive("ludeon.rimworld.royalty");
		ideologyActive = IsActive("ludeon.rimworld.ideology");
		biotechActive = IsActive("ludeon.rimworld.biotech");
		anomalyActive = IsActive("ludeon.rimworld.anomaly");
		odysseyActive = IsActive("ludeon.rimworld.odyssey");
		activeModsInLoadOrderCachedDirty = true;
	}
}
