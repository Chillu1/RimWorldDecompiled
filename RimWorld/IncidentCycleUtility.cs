using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class IncidentCycleUtility
{
	private static List<int> hits = new List<int>();

	private static int QueueIntervalsPassed => Find.TickManager.TicksSinceSettle / 1000;

	public static int IncidentCountThisInterval(IIncidentTarget target, int randSeedSalt, float minDaysPassed, float onDays, float offDays, float minSpacingDays, float minIncidents, float maxIncidents, float acceptFraction = 1f)
	{
		int num = DaysToIntervals(minDaysPassed);
		int num2 = QueueIntervalsPassed - num;
		if (num2 < 0)
		{
			return 0;
		}
		int num3 = DaysToIntervals(onDays);
		int num4 = DaysToIntervals(offDays);
		int minSpacingIntervals = DaysToIntervals(minSpacingDays);
		int num5 = num3 + num4;
		int num6 = num2 / num5;
		int fixedHit = -9999999;
		for (int i = 0; i <= num6; i++)
		{
			int seed = Gen.HashCombineInt(Find.World.info.persistentRandomValue, target.ConstantRandSeed, randSeedSalt, i);
			int start = i * num5;
			if (hits.Count > 0)
			{
				List<int> list = hits;
				fixedHit = list[list.Count - 1];
			}
			hits.Clear();
			GenerateHitList(seed, start, num3, minIncidents, maxIncidents, minSpacingIntervals, acceptFraction, fixedHit);
		}
		int num7 = 0;
		for (int j = 0; j < hits.Count; j++)
		{
			if (hits[j] == num2)
			{
				num7++;
			}
		}
		hits.Clear();
		return num7;
	}

	private static void GenerateHitList(int seed, int start, int length, float minIncidents, float maxIncidents, int minSpacingIntervals, float acceptFraction, int fixedHit)
	{
		if (hits.Count > 0)
		{
			throw new Exception();
		}
		Rand.PushState();
		Rand.Seed = seed;
		int num = GenMath.RoundRandom(Rand.Range(minIncidents, maxIncidents));
		int num2 = 0;
		do
		{
			hits.Clear();
			if (num2++ > 100)
			{
				Log.ErrorOnce("Too many tries finding incident time. minSpacingDays is too high.", 12612131);
				break;
			}
			for (int i = 0; i < num; i++)
			{
				int item = Rand.Range(0, length) + start;
				hits.Add(item);
			}
			hits.Sort();
		}
		while (!RelaxToSatisfyMinDiff(hits, minSpacingIntervals, fixedHit, start + length));
		if (acceptFraction < 1f)
		{
			int num3 = GenMath.RoundRandom((float)hits.Count * acceptFraction);
			hits.Shuffle();
			hits.RemoveRange(num3, hits.Count - num3);
		}
		Rand.PopState();
	}

	private static bool RelaxToSatisfyMinDiff(List<int> values, int minDiff, int fixedValue, int max)
	{
		if (values.Count == 0)
		{
			return true;
		}
		for (int i = 0; i < values.Count; i++)
		{
			int num = ((i == 0) ? Mathf.Abs(values[i] - fixedValue) : Mathf.Abs(values[i] - values[i - 1]));
			if (num >= minDiff)
			{
				continue;
			}
			values[i] += minDiff - num;
			for (int j = i + 1; j < values.Count; j++)
			{
				if (values[j] < values[i] + j)
				{
					values[j] = values[i] + j;
				}
			}
		}
		return values[values.Count - 1] <= max;
	}

	private static int DaysToIntervals(float days)
	{
		return Mathf.RoundToInt(days * 60f);
	}
}
