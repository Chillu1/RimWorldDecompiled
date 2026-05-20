using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class CostListCalculator
{
	private struct CostListPair : IEquatable<CostListPair>
	{
		public BuildableDef buildable;

		public ThingDef stuff;

		public CostListPair(BuildableDef buildable, ThingDef stuff)
		{
			this.buildable = buildable;
			this.stuff = stuff;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(Gen.HashCombine(0, buildable), stuff);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is CostListPair))
			{
				return false;
			}
			return Equals((CostListPair)obj);
		}

		public bool Equals(CostListPair other)
		{
			return this == other;
		}

		public static bool operator ==(CostListPair lhs, CostListPair rhs)
		{
			if (lhs.buildable == rhs.buildable)
			{
				return lhs.stuff == rhs.stuff;
			}
			return false;
		}

		public static bool operator !=(CostListPair lhs, CostListPair rhs)
		{
			return !(lhs == rhs);
		}
	}

	private class FastCostListPairComparer : IEqualityComparer<CostListPair>
	{
		public static readonly FastCostListPairComparer Instance = new FastCostListPairComparer();

		public bool Equals(CostListPair x, CostListPair y)
		{
			return x == y;
		}

		public int GetHashCode(CostListPair obj)
		{
			return obj.GetHashCode();
		}
	}

	private static Dictionary<CostListPair, List<ThingDefCountClass>> cachedCosts = new Dictionary<CostListPair, List<ThingDefCountClass>>(FastCostListPairComparer.Instance);

	private static Difficulty cachedDifficulty = null;

	public static void Reset()
	{
		cachedCosts.Clear();
	}

	public static List<ThingDefCountClass> CostListAdjusted(this Thing thing)
	{
		return thing.def.CostListAdjusted(thing.Stuff);
	}

	public static List<ThingDefCountClass> CostListAdjusted(this BuildableDef entDef, ThingDef stuff, bool errorOnNullStuff = true)
	{
		if (cachedDifficulty != Find.Storyteller.difficulty)
		{
			Reset();
			cachedDifficulty = Find.Storyteller.difficulty;
		}
		CostListPair key = new CostListPair(entDef, stuff);
		if (!cachedCosts.TryGetValue(key, out var value))
		{
			value = new List<ThingDefCountClass>();
			int num = 0;
			if (entDef.MadeFromStuff)
			{
				if (errorOnNullStuff && stuff == null)
				{
					Log.Error("Cannot get AdjustedCostList for " + entDef?.ToString() + " with null Stuff.");
					if (GenStuff.DefaultStuffFor(entDef) == null)
					{
						return null;
					}
					return entDef.CostListAdjusted(GenStuff.DefaultStuffFor(entDef));
				}
				if (stuff != null)
				{
					num = Mathf.RoundToInt((float)entDef.CostStuffCount / stuff.VolumePerUnit);
					if (num < 1)
					{
						num = 1;
					}
				}
				else
				{
					num = entDef.CostStuffCount;
				}
			}
			else if (stuff != null)
			{
				Log.Error("Got AdjustedCostList for " + entDef?.ToString() + " with stuff " + stuff?.ToString() + " but is not MadeFromStuff.");
			}
			bool flag = false;
			if (entDef.CostList != null)
			{
				for (int i = 0; i < entDef.CostList.Count; i++)
				{
					ThingDefCountClass thingDefCountClass = entDef.CostList[i];
					if (thingDefCountClass.thingDef == stuff)
					{
						value.Add(new ThingDefCountClass(thingDefCountClass.thingDef, thingDefCountClass.count + num));
						flag = true;
					}
					else
					{
						value.Add(thingDefCountClass);
					}
				}
			}
			if (!flag && num > 0)
			{
				value.Add(new ThingDefCountClass(stuff, num));
			}
			cachedCosts.Add(key, value);
		}
		return value;
	}
}
