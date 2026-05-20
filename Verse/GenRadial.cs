using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public static class GenRadial
{
	public static IntVec3[] ManualRadialPattern;

	public static IntVec3[] RadialPattern;

	public static float[] RadialPatternRadii;

	private static int[] LengthSquaredToIndexArray;

	private const int RadialPatternCount = 20000;

	private const int MAX_RADIUS = 80;

	private static readonly List<IntVec3> tmpCells;

	private static bool working;

	public static float MaxRadialPatternRadius => RadialPatternRadii[^1];

	static GenRadial()
	{
		ManualRadialPattern = new IntVec3[49];
		RadialPattern = new IntVec3[20000];
		RadialPatternRadii = new float[20000];
		tmpCells = new List<IntVec3>();
		SetupManualRadialPattern();
		SetupRadialPattern();
	}

	private static void SetupManualRadialPattern()
	{
		ManualRadialPattern[0] = new IntVec3(0, 0, 0);
		ManualRadialPattern[1] = new IntVec3(0, 0, -1);
		ManualRadialPattern[2] = new IntVec3(1, 0, 0);
		ManualRadialPattern[3] = new IntVec3(0, 0, 1);
		ManualRadialPattern[4] = new IntVec3(-1, 0, 0);
		ManualRadialPattern[5] = new IntVec3(1, 0, -1);
		ManualRadialPattern[6] = new IntVec3(1, 0, 1);
		ManualRadialPattern[7] = new IntVec3(-1, 0, 1);
		ManualRadialPattern[8] = new IntVec3(-1, 0, -1);
		ManualRadialPattern[9] = new IntVec3(2, 0, 0);
		ManualRadialPattern[10] = new IntVec3(-2, 0, 0);
		ManualRadialPattern[11] = new IntVec3(0, 0, 2);
		ManualRadialPattern[12] = new IntVec3(0, 0, -2);
		ManualRadialPattern[13] = new IntVec3(2, 0, 1);
		ManualRadialPattern[14] = new IntVec3(2, 0, -1);
		ManualRadialPattern[15] = new IntVec3(-2, 0, 1);
		ManualRadialPattern[16] = new IntVec3(-2, 0, -1);
		ManualRadialPattern[17] = new IntVec3(-1, 0, 2);
		ManualRadialPattern[18] = new IntVec3(1, 0, 2);
		ManualRadialPattern[19] = new IntVec3(-1, 0, -2);
		ManualRadialPattern[20] = new IntVec3(1, 0, -2);
		ManualRadialPattern[21] = new IntVec3(2, 0, 2);
		ManualRadialPattern[22] = new IntVec3(-2, 0, -2);
		ManualRadialPattern[23] = new IntVec3(2, 0, -2);
		ManualRadialPattern[24] = new IntVec3(-2, 0, 2);
		ManualRadialPattern[25] = new IntVec3(3, 0, 0);
		ManualRadialPattern[26] = new IntVec3(0, 0, 3);
		ManualRadialPattern[27] = new IntVec3(-3, 0, 0);
		ManualRadialPattern[28] = new IntVec3(0, 0, -3);
		ManualRadialPattern[29] = new IntVec3(3, 0, 1);
		ManualRadialPattern[30] = new IntVec3(-3, 0, -1);
		ManualRadialPattern[31] = new IntVec3(1, 0, 3);
		ManualRadialPattern[32] = new IntVec3(-1, 0, -3);
		ManualRadialPattern[33] = new IntVec3(-3, 0, 1);
		ManualRadialPattern[34] = new IntVec3(3, 0, -1);
		ManualRadialPattern[35] = new IntVec3(-1, 0, 3);
		ManualRadialPattern[36] = new IntVec3(1, 0, -3);
		ManualRadialPattern[37] = new IntVec3(3, 0, 2);
		ManualRadialPattern[38] = new IntVec3(-3, 0, -2);
		ManualRadialPattern[39] = new IntVec3(2, 0, 3);
		ManualRadialPattern[40] = new IntVec3(-2, 0, -3);
		ManualRadialPattern[41] = new IntVec3(-3, 0, 2);
		ManualRadialPattern[42] = new IntVec3(3, 0, -2);
		ManualRadialPattern[43] = new IntVec3(-2, 0, 3);
		ManualRadialPattern[44] = new IntVec3(2, 0, -3);
		ManualRadialPattern[45] = new IntVec3(3, 0, 3);
		ManualRadialPattern[46] = new IntVec3(3, 0, -3);
		ManualRadialPattern[47] = new IntVec3(-3, 0, 3);
		ManualRadialPattern[48] = new IntVec3(-3, 0, -3);
	}

	private static void SetupRadialPattern()
	{
		List<IntVec3> list = new List<IntVec3>();
		for (int i = -80; i < 80; i++)
		{
			for (int j = -80; j < 80; j++)
			{
				list.Add(new IntVec3(i, 0, j));
			}
		}
		list.Sort(delegate(IntVec3 a, IntVec3 b)
		{
			float num2 = a.LengthHorizontalSquared;
			float num3 = b.LengthHorizontalSquared;
			if (num2 < num3)
			{
				return -1;
			}
			return (num2 != num3) ? 1 : 0;
		});
		for (int num = 0; num < 20000; num++)
		{
			RadialPattern[num] = list[num];
			RadialPatternRadii[num] = list[num].LengthHorizontal;
		}
		BuildLengthSquaredIndex();
	}

	private static void BuildLengthSquaredIndex()
	{
		int num = 6400;
		LengthSquaredToIndexArray = new int[num + 1];
		for (int i = 0; i <= num; i++)
		{
			LengthSquaredToIndexArray[i] = -1;
		}
		for (int j = 0; j < 20000; j++)
		{
			int lengthHorizontalSquared = RadialPattern[j].LengthHorizontalSquared;
			if (LengthSquaredToIndexArray[lengthHorizontalSquared] == -1)
			{
				LengthSquaredToIndexArray[lengthHorizontalSquared] = j;
			}
		}
		int num2 = 0;
		for (int k = 0; k <= num; k++)
		{
			if (LengthSquaredToIndexArray[k] != -1)
			{
				num2 = LengthSquaredToIndexArray[k];
			}
			else
			{
				LengthSquaredToIndexArray[k] = num2;
			}
		}
	}

	public static int NumCellsToFillForRadius_ManualRadialPattern(int radius)
	{
		switch (radius)
		{
		case 0:
			return 1;
		case 1:
			return 9;
		case 2:
			return 21;
		case 3:
			return 37;
		default:
			Log.Error("NumSquares radius error");
			return 0;
		}
	}

	public static int NumCellsInRadius(float radius)
	{
		if (radius >= MaxRadialPatternRadius)
		{
			Log.Error($"Not enough squares to get to radius {radius}. Max is {MaxRadialPatternRadius}");
			return 20000;
		}
		float num = radius + float.Epsilon;
		int num2 = (int)Math.Floor(num * num);
		int num3 = 6400;
		if (num2 > num3)
		{
			num2 = num3;
		}
		for (int i = LengthSquaredToIndexArray[num2]; i < 20000; i++)
		{
			if (RadialPatternRadii[i] > num)
			{
				return i;
			}
		}
		return 20000;
	}

	public static float RadiusOfNumCells(int numCells)
	{
		return RadialPatternRadii[numCells];
	}

	public static IEnumerable<IntVec3> RadialPatternInRadius(float radius)
	{
		int numSquares = NumCellsInRadius(radius);
		for (int i = 0; i < numSquares; i++)
		{
			yield return RadialPattern[i];
		}
	}

	public static IEnumerable<IntVec3> RadialCellsAround(IntVec3 center, float radius, bool useCenter)
	{
		int numSquares = NumCellsInRadius(radius);
		for (int i = ((!useCenter) ? 1 : 0); i < numSquares; i++)
		{
			yield return RadialPattern[i] + center;
		}
	}

	public static IEnumerable<IntVec3> RadialCellsAround(IntVec3 center, float minRadius, float maxRadius)
	{
		int numSquares = NumCellsInRadius(maxRadius);
		float minRadiusSquared = minRadius * minRadius;
		for (int i = 0; i < numSquares; i++)
		{
			if ((float)RadialPattern[i].LengthHorizontalSquared >= minRadiusSquared)
			{
				yield return RadialPattern[i] + center;
			}
		}
	}

	public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
	{
		int numCells = NumCellsInRadius(radius);
		HashSet<Thing> returnedThings = null;
		for (int i = ((!useCenter) ? 1 : 0); i < numCells; i++)
		{
			IntVec3 c = RadialPattern[i] + center;
			if (!c.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				Thing thing = thingList[j];
				if (thing.def.size.x > 1 || thing.def.size.z > 1)
				{
					if (returnedThings == null)
					{
						returnedThings = new HashSet<Thing>();
					}
					if (!returnedThings.Add(thing))
					{
						continue;
					}
				}
				yield return thing;
			}
		}
	}

	public static void ProcessEquidistantCells(IntVec3 center, float radius, Func<List<IntVec3>, bool> processor, Map map = null)
	{
		if (working)
		{
			Log.Error("Nested calls to ProcessEquidistantCells() are not allowed.");
			return;
		}
		tmpCells.Clear();
		working = true;
		try
		{
			float num = -1f;
			int num2 = NumCellsInRadius(radius);
			for (int i = 0; i < num2; i++)
			{
				IntVec3 intVec = center + RadialPattern[i];
				if (map != null && !intVec.InBounds(map))
				{
					continue;
				}
				float num3 = intVec.DistanceToSquared(center);
				if (Mathf.Abs(num3 - num) > 0.0001f)
				{
					if (tmpCells.Any() && processor(tmpCells))
					{
						return;
					}
					num = num3;
					tmpCells.Clear();
				}
				tmpCells.Add(intVec);
			}
			if (tmpCells.Any())
			{
				processor(tmpCells);
			}
		}
		finally
		{
			tmpCells.Clear();
			working = false;
		}
	}
}
