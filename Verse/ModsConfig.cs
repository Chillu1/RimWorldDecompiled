using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
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

		private static HashSet<string> activeModsHashSet;

		private static List<ModMetaData> activeModsInLoadOrderCached;

		private static bool activeModsInLoadOrderCachedDirty;

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

		public static bool RoyaltyActive => royaltyActive;

		static ModsConfig()
		{
			activeModsHashSet = new HashSet<string>();
			activeModsInLoadOrderCached = new List<ModMetaData>();
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
				else if (data.version.Length > 0 && data.version.All((char x) => char.IsNumber(x)) && int.TryParse(data.version, out result) && result <= 2009)
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
					if (TryGetPackageIdWithoutExtraSteamPostfix(packageId, out string nonSteamPackageId) && ModLister.GetModWithIdentifier(nonSteamPackageId) != null)
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
						Debug.LogWarning("There was more than one enabled instance of mod with PackageID: " + allInstalledMod.PackageIdNonUnique + ". Disabling the duplicates.");
						continue;
					}
					hashSet.Add(allInstalledMod.PackageIdNonUnique);
				}
				if (!allInstalledMod.IsCoreMod && allInstalledMod.Official && IsExpansionNew(allInstalledMod.PackageId))
				{
					SetActive(allInstalledMod.PackageId, active: true);
					AddKnownExpansion(allInstalledMod.PackageId);
					flag2 = true;
				}
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
				if (modWithIdentifier == null && TryGetPackageIdWithoutExtraSteamPostfix(data.activeMods[num], out string nonSteamPackageId))
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
			data.activeMods.Add(ModContentPack.CoreModPackageId);
			foreach (ModMetaData allInstalledMod in ModLister.AllInstalledMods)
			{
				if (allInstalledMod.Official && !allInstalledMod.IsCoreMod && allInstalledMod.VersionCompatible)
				{
					data.activeMods.Add(allInstalledMod.PackageId);
				}
			}
			Save();
			RecacheActiveMods();
		}

		public static void Reorder(int modIndex, int newIndex)
		{
			if (modIndex != newIndex)
			{
				data.activeMods.Insert(newIndex, data.activeMods[modIndex]);
				data.activeMods.RemoveAt((modIndex < newIndex) ? modIndex : (modIndex + 1));
				activeModsInLoadOrderCachedDirty = true;
			}
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
				}
			}
			else if (data.activeMods.Contains(item))
			{
				data.activeMods.Remove(item);
			}
			RecacheActiveMods();
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
			if (IsExpansionNew(id))
			{
				data.knownExpansions.Add(id.ToLower());
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

		public static List<string> GetModWarnings()
		{
			List<string> list = new List<string>();
			List<ModMetaData> mods = ActiveModsInLoadOrder.ToList();
			for (int i = 0; i < mods.Count; i++)
			{
				int index = i;
				ModMetaData modMetaData = mods[index];
				StringBuilder stringBuilder = new StringBuilder("");
				for (int j = 0; j < mods.Count; j++)
				{
					if (i != j && mods[j].PackageId != "" && mods[j].SamePackageId(mods[i].PackageId))
					{
						stringBuilder.AppendLine("ModWithSameIdAlreadyActive".Translate(mods[j].Name));
					}
				}
				List<string> list2 = FindConflicts(mods, modMetaData.IncompatibleWith, null);
				if (list2.Any())
				{
					stringBuilder.AppendLine("ModIncompatibleWithTip".Translate(list2.ToCommaList(useAnd: true)));
				}
				List<string> list3 = FindConflicts(mods, modMetaData.LoadBefore, (ModMetaData beforeMod) => mods.IndexOf(beforeMod) < index);
				if (list3.Any())
				{
					stringBuilder.AppendLine("ModMustLoadBefore".Translate(list3.ToCommaList(useAnd: true)));
				}
				List<string> list4 = FindConflicts(mods, modMetaData.LoadAfter, (ModMetaData afterMod) => mods.IndexOf(afterMod) > index);
				if (list4.Any())
				{
					stringBuilder.AppendLine("ModMustLoadAfter".Translate(list4.ToCommaList(useAnd: true)));
				}
				if (modMetaData.Dependencies.Any())
				{
					List<string> list5 = modMetaData.UnsatisfiedDependencies();
					if (list5.Any())
					{
						stringBuilder.AppendLine("ModUnsatisfiedDependency".Translate(list5.ToCommaList(useAnd: true)));
					}
				}
				list.Add(stringBuilder.ToString().TrimEndNewlines());
			}
			return list;
		}

		public static bool ModHasAnyOrderingIssues(ModMetaData mod)
		{
			List<ModMetaData> mods = ActiveModsInLoadOrder.ToList();
			int index = mods.IndexOf(mod);
			if (index == -1)
			{
				return false;
			}
			if (FindConflicts(mods, mod.LoadBefore, (ModMetaData beforeMod) => mods.IndexOf(beforeMod) < index).Count > 0)
			{
				return true;
			}
			if (FindConflicts(mods, mod.LoadAfter, (ModMetaData afterMod) => mods.IndexOf(afterMod) > index).Count > 0)
			{
				return true;
			}
			return false;
		}

		private static List<string> FindConflicts(List<ModMetaData> allMods, List<string> modsToCheck, Func<ModMetaData, bool> predicate)
		{
			List<string> list = new List<string>();
			foreach (string modId in modsToCheck)
			{
				ModMetaData modMetaData = allMods.FirstOrDefault((ModMetaData m) => m.SamePackageId(modId, ignorePostfix: true));
				if (modMetaData != null && (predicate == null || predicate(modMetaData)))
				{
					list.Add(modMetaData.Name);
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
				foreach (string before in modMetaData.LoadBefore)
				{
					ModMetaData modMetaData2 = list.FirstOrDefault((ModMetaData m) => m.SamePackageId(before, ignorePostfix: true));
					if (modMetaData2 != null)
					{
						directedAcyclicGraph.AddEdge(list.IndexOf(modMetaData2), i);
					}
				}
				foreach (string after in modMetaData.LoadAfter)
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
			royaltyActive = IsActive(ModContentPack.RoyaltyModPackageId);
			activeModsInLoadOrderCachedDirty = true;
		}
	}
}
