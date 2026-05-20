using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse;

public static class DefDatabase<T> where T : Def
{
	private static readonly List<T> defsList = new List<T>();

	private static readonly Dictionary<string, T> defsByName = new Dictionary<string, T>();

	private static readonly Dictionary<ushort, T> defsByShortHash = new Dictionary<ushort, T>();

	public static IEnumerable<T> AllDefs => defsList;

	public static List<T> AllDefsListForReading => defsList;

	public static int DefCount => defsList.Count;

	public static void AddAllInMods()
	{
		HashSet<string> hashSet = new HashSet<string>();
		foreach (ModContentPack item in LoadedModManager.RunningMods.OrderBy((ModContentPack m) => m.OverwritePriority).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
		{
			string sourceName = item.ToString();
			hashSet.Clear();
			foreach (T item2 in GenDefDatabase.DefsToGoInDatabase<T>(item))
			{
				if (!hashSet.Add(item2.defName))
				{
					Log.Error("Mod " + item?.ToString() + " has multiple " + typeof(T)?.ToString() + "s named " + item2.defName + ". Skipping.");
				}
				else
				{
					AddDef(item2, sourceName);
				}
			}
		}
		foreach (T item3 in LoadedModManager.PatchedDefsForReading.OfType<T>())
		{
			AddDef(item3, "Patches");
		}
		static void AddDef(T def, string text2)
		{
			if (def.defName == "UnnamedDef")
			{
				string text = "Unnamed" + typeof(T).Name + Rand.Range(1, 100000) + "A";
				Log.Error(typeof(T).Name + " in " + text2 + " with label " + def.label + " lacks a defName. Giving name " + text);
				def.defName = text;
				def.ResolveDefNameHash();
			}
			if (defsByName.TryGetValue(def.defName, out var value))
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
		if (def == null)
		{
			Log.Error("Tried to add null def to DefDatabase.");
			return;
		}
		while (defsByName.ContainsKey(def.defName))
		{
			Log.Error("Adding duplicate " + typeof(T)?.ToString() + " name: " + def.defName);
			def.defName += Mathf.RoundToInt(Rand.Value * 1000f);
			def.ResolveDefNameHash();
		}
		defsList.Add(def);
		defsByName.Add(def.defName, def);
		if (defsList.Count > 65535)
		{
			Log.Error("Too many " + typeof(T)?.ToString() + "; over " + ushort.MaxValue);
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
		defsByShortHash.Clear();
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
				if (onlyExactlyMyType && def.GetType() != typeof(T))
				{
					return;
				}
				DeepProfiler.Start("Resolver call");
				try
				{
					def.ResolveReferences();
				}
				catch (Exception ex)
				{
					Log.Error("Error while resolving references for def " + def?.ToString() + ": " + ex);
				}
				finally
				{
					DeepProfiler.End();
				}
			};
			if (parallel)
			{
				GenThreading.ParallelForEach(defsList, action);
			}
			else
			{
				for (int num = 0; num < defsList.Count; num++)
				{
					action(defsList[num]);
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
		for (int j = 0; j < defsList.Count; j++)
		{
			defsList[j].PostSetIndices();
		}
	}

	public static void ErrorCheckAllDefs()
	{
		foreach (T allDef in AllDefs)
		{
			try
			{
				if (allDef.ignoreConfigErrors)
				{
					continue;
				}
				foreach (string item in allDef.ConfigErrors())
				{
					Log.Error("Config error in " + allDef?.ToString() + ": " + item);
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
			if (defsByName.TryGetValue(defName, out var value))
			{
				return value;
			}
			Log.Error("Failed to find " + typeof(T)?.ToString() + " named " + defName + ". There are " + defsList.Count + " defs of this type loaded.");
			return null;
		}
		if (defsByName.TryGetValue(defName, out var value2))
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
		if (defsByShortHash.TryGetValue(shortHash, out var value))
		{
			return value;
		}
		return null;
	}

	public static void InitializeShortHashDictionary()
	{
		defsByShortHash.EnsureCapacity(defsList.Count);
		for (int i = 0; i < defsList.Count; i++)
		{
			defsByShortHash[defsList[i].shortHash] = defsList[i];
		}
	}

	public static T GetRandom()
	{
		return defsList.RandomElement();
	}
}
