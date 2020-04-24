using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class DefDatabase<T> where T : Def
	{
		private static List<T> defsList = new List<T>();

		private static Dictionary<string, T> defsByName = new Dictionary<string, T>();

		public static IEnumerable<T> AllDefs => defsList;

		public static List<T> AllDefsListForReading => defsList;

		public static int DefCount => defsList.Count;

		public static void AddAllInMods()
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (ModContentPack item in LoadedModManager.RunningMods.OrderBy((ModContentPack m) => m.OverwritePriority).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
			{
				hashSet.Clear();
				foreach (T item2 in GenDefDatabase.DefsToGoInDatabase<T>(item))
				{
					if (!hashSet.Add(item2.defName))
					{
						Log.Error("Mod " + item + " has multiple " + typeof(T) + "s named " + item2.defName + ". Skipping.");
					}
					else
					{
						AddDef(item2, item.ToString());
					}
				}
			}
			foreach (T item3 in LoadedModManager.PatchedDefsForReading.OfType<T>())
			{
				AddDef(item3, "Patches");
			}
			void AddDef(T def, string sourceName)
			{
				if (def.defName == "UnnamedDef")
				{
					string text = "Unnamed" + typeof(T).Name + Rand.Range(1, 100000).ToString() + "A";
					Log.Error(typeof(T).Name + " in " + sourceName + " with label " + def.label + " lacks a defName. Giving name " + text);
					def.defName = text;
				}
				if (defsByName.TryGetValue(def.defName, out T value))
				{
					Remove(value);
				}
				Add(def);
			}
		}

		public static void Add(IEnumerable<T> defs)
		{
			foreach (T def in defs)
			{
				Add(def);
			}
		}

		public static void Add(T def)
		{
			while (defsByName.ContainsKey(def.defName))
			{
				Log.Error("Adding duplicate " + typeof(T) + " name: " + def.defName);
				def.defName += Mathf.RoundToInt(Rand.Value * 1000f);
			}
			defsList.Add(def);
			defsByName.Add(def.defName, def);
			if (defsList.Count > 65535)
			{
				Log.Error("Too many " + typeof(T) + "; over " + ushort.MaxValue);
			}
			def.index = (ushort)(defsList.Count - 1);
		}

		private static void Remove(T def)
		{
			defsByName.Remove(def.defName);
			defsList.Remove(def);
			SetIndices();
		}

		public static void Clear()
		{
			defsList.Clear();
			defsByName.Clear();
		}

		public static void ClearCachedData()
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				defsList[i].ClearCachedData();
			}
		}

		public static void ResolveAllReferences(bool onlyExactlyMyType = true, bool parallel = false)
		{
			DeepProfiler.Start("SetIndices");
			try
			{
				SetIndices();
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("ResolveAllReferences " + typeof(T).FullName);
			try
			{
				Action<T> action = delegate(T def)
				{
					if (!onlyExactlyMyType || !(def.GetType() != typeof(T)))
					{
						DeepProfiler.Start("Resolver call");
						try
						{
							def.ResolveReferences();
						}
						catch (Exception ex)
						{
							Log.Error("Error while resolving references for def " + def + ": " + ex);
						}
						finally
						{
							DeepProfiler.End();
						}
					}
				};
				if (parallel)
				{
					GenThreading.ParallelForEach(defsList, action);
				}
				else
				{
					for (int i = 0; i < defsList.Count; i++)
					{
						action(defsList[i]);
					}
				}
			}
			finally
			{
				DeepProfiler.End();
			}
			DeepProfiler.Start("SetIndices");
			try
			{
				SetIndices();
			}
			finally
			{
				DeepProfiler.End();
			}
		}

		private static void SetIndices()
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				defsList[i].index = (ushort)i;
			}
		}

		public static void ErrorCheckAllDefs()
		{
			foreach (T allDef in AllDefs)
			{
				try
				{
					if (!allDef.ignoreConfigErrors)
					{
						foreach (string item in allDef.ConfigErrors())
						{
							Log.Error("Config error in " + allDef + ": " + item);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception in ConfigErrors() of " + allDef.defName + ": " + ex);
				}
			}
		}

		public static T GetNamed(string defName, bool errorOnFail = true)
		{
			if (errorOnFail)
			{
				if (defsByName.TryGetValue(defName, out T value))
				{
					return value;
				}
				Log.Error("Failed to find " + typeof(T) + " named " + defName + ". There are " + defsList.Count + " defs of this type loaded.");
				return null;
			}
			if (defsByName.TryGetValue(defName, out T value2))
			{
				return value2;
			}
			return null;
		}

		public static T GetNamedSilentFail(string defName)
		{
			return GetNamed(defName, errorOnFail: false);
		}

		public static T GetByShortHash(ushort shortHash)
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				if (defsList[i].shortHash == shortHash)
				{
					return defsList[i];
				}
			}
			return null;
		}

		public static T GetRandom()
		{
			return defsList.RandomElement();
		}
	}
}
