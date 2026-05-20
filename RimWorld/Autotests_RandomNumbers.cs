using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class Autotests_RandomNumbers
{
	public static void Run()
	{
		Log.Message("Running random numbers tests.");
		CheckSimpleFloats();
		CheckIntsRange();
		CheckIntsDistribution();
		CheckSeed();
		Log.Message("Finished.");
	}

	private static void CheckSimpleFloats()
	{
		List<float> list = RandomFloats(500).ToList();
		if (list.Any((float x) => x < 0f || x > 1f))
		{
			Log.Error("Float out of range.");
		}
		if (!list.Any((float x) => x < 0.1f) || !list.Any((float x) => (double)x > 0.5 && (double)x < 0.6) || !list.Any((float x) => (double)x > 0.9))
		{
			Log.Warning("Possibly uneven distribution.");
		}
		list = RandomFloats(1300000).ToList();
		int num = list.Count((float x) => (double)x < 0.1);
		Log.Message("< 0.1 count (should be ~10%): " + (float)num / (float)list.Count() * 100f + "%");
		num = list.Count((float x) => (double)x < 0.0001);
		Log.Message("< 0.0001 count (should be ~0.01%): " + (float)num / (float)list.Count() * 100f + "%");
	}

	private static IEnumerable<float> RandomFloats(int count)
	{
		for (int i = 0; i < count; i++)
		{
			yield return Rand.Value;
		}
	}

	private static void CheckIntsRange()
	{
		int num = -7;
		int num2 = 4;
		int num3 = 0;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		while (true)
		{
			bool flag = true;
			for (int i = num; i <= num2; i++)
			{
				if (!dictionary.ContainsKey(i))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				break;
			}
			num3++;
			if (num3 == 200000)
			{
				Log.Error("Failed to find all numbers in a range.");
				return;
			}
			int num4 = Rand.RangeInclusive(num, num2);
			if (num4 < num || num4 > num2)
			{
				Log.Error("Value out of range.");
			}
			if (dictionary.ContainsKey(num4))
			{
				dictionary[num4]++;
			}
			else
			{
				dictionary.Add(num4, 1);
			}
		}
		Log.Message("Values between " + num + " and " + num2 + " (value: number of occurrences):");
		for (int j = num; j <= num2; j++)
		{
			Log.Message(j + ": " + dictionary[j]);
		}
	}

	private static void CheckIntsDistribution()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 1000000; i++)
		{
			int num = Rand.RangeInclusive(-2, 1);
			list.Add(num + 2);
		}
		Log.Message("Ints distribution (should be even):");
		int j;
		for (j = 0; j < 4; j++)
		{
			Log.Message(j + ": " + (float)list.Count((int x) => x == j) / (float)list.Count() * 100f + "%");
		}
	}

	private static void CheckSeed()
	{
		int seed = (Rand.Seed = 10);
		int num2 = Rand.Int;
		int num3 = Rand.Int;
		Rand.Seed = seed;
		int num4 = Rand.Int;
		int num5 = Rand.Int;
		if (num2 != num4 || num3 != num5)
		{
			Log.Error("Same seed, different values.");
		}
		TestPushSeed(15, 20);
		TestPushSeed(-2147483645, 20);
		TestPushSeed(6, int.MaxValue);
		TestPushSeed(-2147483645, 2147483642);
		TestPushSeed(-1947483645, 1147483642);
		TestPushSeed(455, 648023);
	}

	private static void TestPushSeed(int seed1, int seed2)
	{
		Rand.Seed = seed1;
		int num = Rand.Int;
		int num2 = Rand.Int;
		Rand.PushState();
		Rand.Seed = seed2;
		int num3 = Rand.Int;
		Rand.PopState();
		Rand.Seed = seed1;
		int num4 = Rand.Int;
		Rand.PushState();
		Rand.Seed = seed2;
		int num5 = Rand.Int;
		Rand.PopState();
		int num6 = Rand.Int;
		if (num != num4 || num2 != num6 || num3 != num5)
		{
			Log.Error("PushSeed broken.");
		}
	}
}
