using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class ThingSetMakerByTotalStatUtility
{
	private static List<ThingStuffPairWithQuality> allowedThingStuffPairs = new List<ThingStuffPairWithQuality>();

	private static List<ThingStuffPairWithQuality> candidatesTmp = new List<ThingStuffPairWithQuality>();

	private static List<ThingStuffPairWithQuality> candidatesTmpNew = new List<ThingStuffPairWithQuality>();

	private static Dictionary<ThingStuffPairWithQuality, float> minValuesTmp = new Dictionary<ThingStuffPairWithQuality, float>();

	private static Dictionary<ThingStuffPairWithQuality, float> maxValuesTmp = new Dictionary<ThingStuffPairWithQuality, float>();

	public static List<ThingStuffPairWithQuality> GenerateDefsWithPossibleTotalValue(IntRange countRange, float totalValue, IEnumerable<ThingDef> allowed, TechLevel techLevel, QualityGenerator qualityGenerator, Func<ThingStuffPairWithQuality, float> getMinValue, Func<ThingStuffPairWithQuality, float> getMaxValue, Func<ThingStuffPairWithQuality, float> getSingleThingValue, Func<ThingDef, float> weightSelector = null, int tries = 100, float maxMass = float.MaxValue, bool allowNonStackableDuplicates = true, float minSingleItemValue = 0f)
	{
		minValuesTmp.Clear();
		maxValuesTmp.Clear();
		List<ThingStuffPairWithQuality> chosen = new List<ThingStuffPairWithQuality>();
		if (countRange.max <= 0)
		{
			return chosen;
		}
		if (countRange.min < 1)
		{
			countRange.min = 1;
		}
		CalculateAllowedThingStuffPairs(allowed, techLevel, qualityGenerator);
		float trashThreshold = Mathf.Max(GetTrashThreshold(countRange, totalValue, MaxValue), minSingleItemValue);
		allowedThingStuffPairs.RemoveAll((ThingStuffPairWithQuality x) => MaxValue(x) < trashThreshold);
		if (!allowedThingStuffPairs.Any())
		{
			return chosen;
		}
		float a = float.MaxValue;
		float num = float.MinValue;
		float num2 = float.MaxValue;
		foreach (ThingStuffPairWithQuality allowedThingStuffPair in allowedThingStuffPairs)
		{
			a = Mathf.Min(a, MinValue(allowedThingStuffPair));
			num = Mathf.Max(num, MaxValue(allowedThingStuffPair));
			if (allowedThingStuffPair.thing.category != ThingCategory.Pawn)
			{
				num2 = Mathf.Min(num2, GetNonTrashMass(allowedThingStuffPair, trashThreshold, MinValue));
			}
		}
		a = Mathf.Max(a, trashThreshold);
		float totalMinValueSoFar = 0f;
		float totalMaxValueSoFar = 0f;
		float minMassSoFar = 0f;
		int num3 = 0;
		do
		{
			num3++;
			if (num3 > 10000)
			{
				Log.Error("Too many iterations.");
				break;
			}
			candidatesTmp.Clear();
			for (int num4 = 0; num4 < allowedThingStuffPairs.Count; num4++)
			{
				ThingStuffPairWithQuality candidate = allowedThingStuffPairs[num4];
				if (!allowNonStackableDuplicates && candidate.thing.stackLimit == 1 && chosen.Any((ThingStuffPairWithQuality c) => c.thing == candidate.thing))
				{
					continue;
				}
				if (maxMass != float.MaxValue && candidate.thing.category != ThingCategory.Pawn)
				{
					float nonTrashMass = GetNonTrashMass(candidate, trashThreshold, MinValue);
					if (minMassSoFar + nonTrashMass > maxMass || (chosen.Count < countRange.min && minMassSoFar + num2 * (float)(countRange.min - chosen.Count - 1) + nonTrashMass > maxMass))
					{
						continue;
					}
				}
				if (!(totalMinValueSoFar + Mathf.Max(MinValue(candidate), trashThreshold) > totalValue) && (chosen.Count >= countRange.min || !(totalMinValueSoFar + a * (float)(countRange.min - chosen.Count - 1) + Mathf.Max(MinValue(candidate), trashThreshold) > totalValue)))
				{
					candidatesTmp.Add(candidate);
				}
			}
			if (countRange.max != int.MaxValue && totalMaxValueSoFar < totalValue * 0.5f)
			{
				candidatesTmpNew.Clear();
				for (int num5 = 0; num5 < candidatesTmp.Count; num5++)
				{
					ThingStuffPairWithQuality thingStuffPairWithQuality = candidatesTmp[num5];
					if (totalMaxValueSoFar + num * (float)(countRange.max - chosen.Count - 1) + MaxValue(thingStuffPairWithQuality) >= totalValue * 0.5f)
					{
						candidatesTmpNew.Add(thingStuffPairWithQuality);
					}
				}
				if (candidatesTmpNew.Any())
				{
					candidatesTmp.Clear();
					candidatesTmp.AddRange(candidatesTmpNew);
				}
			}
			float maxCandidateMinValue = float.MinValue;
			for (int num6 = 0; num6 < candidatesTmp.Count; num6++)
			{
				ThingStuffPairWithQuality t = candidatesTmp[num6];
				maxCandidateMinValue = Mathf.Max(maxCandidateMinValue, Mathf.Max(MinValue(t), trashThreshold));
			}
			if (!candidatesTmp.TryRandomElementByWeight(delegate(ThingStuffPairWithQuality x)
			{
				float a2 = 1f;
				if (countRange.max != int.MaxValue && chosen.Count < countRange.max && totalValue >= totalMaxValueSoFar)
				{
					int num7 = countRange.max - chosen.Count;
					float b = (totalValue - totalMaxValueSoFar) / (float)num7;
					a2 = Mathf.InverseLerp(0f, b, MaxValue(x));
				}
				float b2 = 1f;
				if (chosen.Count < countRange.min && totalValue >= totalMinValueSoFar)
				{
					int num8 = countRange.min - chosen.Count;
					float num9 = (totalValue - totalMinValueSoFar) / (float)num8;
					float num10 = Mathf.Max(MinValue(x), trashThreshold);
					if (num10 > num9)
					{
						b2 = num9 / num10;
					}
				}
				float num11 = Mathf.Max(Mathf.Min(a2, b2), 1E-05f);
				if (weightSelector != null)
				{
					num11 *= weightSelector(x.thing);
				}
				if (totalValue > totalMaxValueSoFar)
				{
					int num12 = Mathf.Max(countRange.min - chosen.Count, 1);
					float num13 = Mathf.InverseLerp(0f, maxCandidateMinValue * 0.85f, GetMaxValueWithMaxMass(x, minMassSoFar, maxMass, MinValue, MaxValue) * (float)num12);
					num11 *= num13 * num13;
				}
				if (PawnWeaponGenerator.IsDerpWeapon(x.thing, x.stuff))
				{
					num11 *= 0.1f;
				}
				if (techLevel != TechLevel.Undefined)
				{
					TechLevel techLevel2 = x.thing.techLevel;
					if ((int)techLevel2 < (int)techLevel && (int)techLevel2 <= 2 && (x.thing.IsApparel || x.thing.IsWeapon))
					{
						num11 *= 0.1f;
					}
				}
				return num11;
			}, out var result))
			{
				break;
			}
			chosen.Add(result);
			totalMinValueSoFar += Mathf.Max(MinValue(result), trashThreshold);
			totalMaxValueSoFar += MaxValue(result);
			if (result.thing.category != ThingCategory.Pawn)
			{
				minMassSoFar += GetNonTrashMass(result, trashThreshold, MinValue);
			}
		}
		while (chosen.Count < countRange.max && (chosen.Count < countRange.min || !(totalMaxValueSoFar >= totalValue * 0.9f)));
		return chosen;
		float MaxValue(ThingStuffPairWithQuality thingStuffPairWithQuality2)
		{
			if (!maxValuesTmp.TryGetValue(thingStuffPairWithQuality2, out var value))
			{
				value = getMaxValue(thingStuffPairWithQuality2);
				maxValuesTmp[thingStuffPairWithQuality2] = value;
			}
			return value;
		}
		float MinValue(ThingStuffPairWithQuality thingStuffPairWithQuality2)
		{
			if (!minValuesTmp.TryGetValue(thingStuffPairWithQuality2, out var value))
			{
				value = getMinValue(thingStuffPairWithQuality2);
				minValuesTmp[thingStuffPairWithQuality2] = value;
			}
			return value;
		}
	}

	public static void IncreaseStackCountsToTotalValue(List<Thing> things, float totalValue, Func<Thing, float> getValue, float maxMass = float.MaxValue, bool satisfyMinRewardCount = false)
	{
		float currentTotalValue = 0f;
		float currentTotalMass = 0f;
		for (int i = 0; i < things.Count; i++)
		{
			currentTotalValue += getValue(things[i]) * (float)things[i].stackCount;
			if (!(things[i] is Pawn))
			{
				currentTotalMass += things[i].GetStatValue(StatDefOf.Mass) * (float)things[i].stackCount;
			}
		}
		if (currentTotalValue >= totalValue || currentTotalMass >= maxMass)
		{
			return;
		}
		things.SortByDescending((Thing x) => getValue(x) / x.GetStatValue(StatDefOf.Mass));
		DistributeEvenly(things, currentTotalValue + (totalValue - currentTotalValue) * 0.1f, ref currentTotalValue, ref currentTotalMass, getValue, (maxMass == float.MaxValue) ? float.MaxValue : (currentTotalMass + (maxMass - currentTotalMass) * 0.1f));
		if (currentTotalValue >= totalValue || currentTotalMass >= maxMass)
		{
			return;
		}
		if (satisfyMinRewardCount)
		{
			SatisfyMinRewardCount(things, totalValue, ref currentTotalValue, ref currentTotalMass, getValue, maxMass);
			if (currentTotalValue >= totalValue || currentTotalMass >= maxMass)
			{
				return;
			}
		}
		DistributeEvenly(things, totalValue, ref currentTotalValue, ref currentTotalMass, getValue, maxMass, useValueMassRatio: true);
		if (!(currentTotalValue >= totalValue) && !(currentTotalMass >= maxMass))
		{
			GiveRemainingValueToAnything(things, totalValue, ref currentTotalValue, ref currentTotalMass, getValue, maxMass);
		}
	}

	private static void DistributeEvenly(List<Thing> things, float totalValue, ref float currentTotalValue, ref float currentTotalMass, Func<Thing, float> getValue, float maxMass, bool useValueMassRatio = false)
	{
		float num = (totalValue - currentTotalValue) / (float)things.Count;
		float num2 = maxMass - currentTotalMass;
		float num3 = ((maxMass == float.MaxValue) ? float.MaxValue : (num2 / (float)things.Count));
		float num4 = 0f;
		if (useValueMassRatio)
		{
			for (int i = 0; i < things.Count; i++)
			{
				num4 += getValue(things[i]) / things[i].GetStatValue(StatDefOf.Mass);
			}
		}
		for (int j = 0; j < things.Count; j++)
		{
			float num5 = getValue(things[j]);
			int num6 = Mathf.Min(Mathf.FloorToInt(num / num5), things[j].def.stackLimit - things[j].stackCount);
			if (maxMass != float.MaxValue && !(things[j] is Pawn))
			{
				num6 = Mathf.Min(num6, Mathf.FloorToInt(Mathf.Min(b: (!useValueMassRatio) ? num3 : (num2 * (getValue(things[j]) / things[j].GetStatValue(StatDefOf.Mass) / num4)), a: maxMass - currentTotalMass) / things[j].GetStatValue(StatDefOf.Mass)));
			}
			if (num6 > 0)
			{
				things[j].stackCount += num6;
				currentTotalValue += num5 * (float)num6;
				if (!(things[j] is Pawn))
				{
					currentTotalMass += things[j].GetStatValue(StatDefOf.Mass) * (float)num6;
				}
			}
		}
	}

	private static void SatisfyMinRewardCount(List<Thing> things, float totalValue, ref float currentTotalValue, ref float currentTotalMass, Func<Thing, float> getValue, float maxMass)
	{
		for (int i = 0; i < things.Count; i++)
		{
			if (things[i].stackCount >= things[i].def.minRewardCount)
			{
				continue;
			}
			float num = getValue(things[i]);
			int num2 = Mathf.FloorToInt((totalValue - currentTotalValue) / num);
			int num3 = Mathf.Min(num2, things[i].def.stackLimit - things[i].stackCount, things[i].def.minRewardCount - things[i].stackCount);
			if (maxMass != float.MaxValue && !(things[i] is Pawn))
			{
				num3 = Mathf.Min(num3, Mathf.FloorToInt((maxMass - currentTotalMass) / things[i].GetStatValue(StatDefOf.Mass)));
			}
			if (num3 > 0)
			{
				things[i].stackCount += num3;
				currentTotalValue += num * (float)num3;
				if (!(things[i] is Pawn))
				{
					currentTotalMass += things[i].GetStatValue(StatDefOf.Mass) * (float)num3;
				}
			}
		}
	}

	private static void GiveRemainingValueToAnything(List<Thing> things, float totalValue, ref float currentTotalValue, ref float currentTotalMass, Func<Thing, float> getValue, float maxMass)
	{
		for (int i = 0; i < things.Count; i++)
		{
			float num = getValue(things[i]);
			int num2 = Mathf.Min(Mathf.FloorToInt((totalValue - currentTotalValue) / num), things[i].def.stackLimit - things[i].stackCount);
			if (maxMass != float.MaxValue && !(things[i] is Pawn))
			{
				num2 = Mathf.Min(num2, Mathf.FloorToInt((maxMass - currentTotalMass) / things[i].GetStatValue(StatDefOf.Mass)));
			}
			if (num2 > 0)
			{
				things[i].stackCount += num2;
				currentTotalValue += num * (float)num2;
				if (!(things[i] is Pawn))
				{
					currentTotalMass += things[i].GetStatValue(StatDefOf.Mass) * (float)num2;
				}
			}
		}
	}

	private static void CalculateAllowedThingStuffPairs(IEnumerable<ThingDef> allowed, TechLevel techLevel, QualityGenerator qualityGenerator)
	{
		allowedThingStuffPairs.Clear();
		foreach (ThingDef td in allowed)
		{
			for (int i = 0; i < 5; i++)
			{
				if (GenStuff.TryRandomStuffFor(td, out var stuff, techLevel, (ThingDef x) => !ThingSetMakerUtility.IsDerpAndDisallowed(td, x, qualityGenerator)))
				{
					QualityCategory quality = (td.HasComp(typeof(CompQuality)) ? QualityUtility.GenerateQuality(qualityGenerator) : QualityCategory.Normal);
					allowedThingStuffPairs.Add(new ThingStuffPairWithQuality(td, stuff, quality));
				}
			}
		}
	}

	private static float GetTrashThreshold(IntRange countRange, float totalValue, Func<ThingStuffPairWithQuality, float> getMaxValue)
	{
		float num = GenMath.Median(allowedThingStuffPairs, getMaxValue);
		int num2 = Mathf.Clamp(Mathf.CeilToInt(totalValue / num), countRange.min, countRange.max);
		return totalValue / (float)num2 * 0.2f;
	}

	private static float GetNonTrashMass(ThingStuffPairWithQuality t, float trashThreshold, Func<ThingStuffPairWithQuality, float> getSingleThingValue)
	{
		int num = Mathf.Clamp(Mathf.CeilToInt(trashThreshold / getSingleThingValue(t)), 1, t.thing.stackLimit);
		return t.GetStatValue(StatDefOf.Mass) * (float)num;
	}

	private static float GetMaxValueWithMaxMass(ThingStuffPairWithQuality t, float massSoFar, float maxMass, Func<ThingStuffPairWithQuality, float> getSingleThingValue, Func<ThingStuffPairWithQuality, float> getMaxValue)
	{
		if (maxMass == float.MaxValue)
		{
			return getMaxValue(t);
		}
		int num = Mathf.Clamp(Mathf.FloorToInt((maxMass - massSoFar) / t.GetStatValue(StatDefOf.Mass)), 1, t.thing.stackLimit);
		return getSingleThingValue(t) * (float)num;
	}
}
