using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenRadial
	{
		public static IntVec3[] ManualRadialPattern;

		public static IntVec3[] RadialPattern;

		private static float[] RadialPatternRadii;

		private const int RadialPatternCount = 10000;

		private static List<IntVec3> tmpCells;

		private static bool working;

		public static float MaxRadialPatternRadius => RadialPatternRadii[RadialPatternRadii.Length - 1];

		static GenRadial()
		{
			ManualRadialPattern = new IntVec3[49];
			RadialPattern = new IntVec3[10000];
			RadialPatternRadii = new float[10000];
			tmpCells = new List<IntVec3>();
			working = false;
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
			for (int i = -60; i < 60; i++)
			{
				for (int j = -60; j < 60; j++)
				{
					list.Add(new IntVec3(i, 0, j));
				}
			}
			list.Sort(delegate(IntVec3 A, IntVec3 B)
			{
				float num = A.LengthHorizontalSquared;
				float num2 = B.LengthHorizontalSquared;
				if (num < num2)
				{
					return -1;
				}
				return (num != num2) ? 1 : 0;
			});
			for (int k = 0; k < 10000; k++)
			{
				RadialPattern[k] = list[k];
				RadialPatternRadii[k] = list[k].LengthHorizontal;
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
				Log.Error("Not enough squares to get to radius " + radius + ". Max is " + MaxRadialPatternRadius);
				return 10000;
			}
			float num = radius + float.Epsilon;
			for (int i = 0; i < 10000; i++)
			{
				if (RadialPatternRadii[i] > num)
				{
					return i;
				}
			}
			return 10000;
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
			for (int i = (!useCenter) ? 1 : 0; i < numSquares; i++)
			{
				yield return RadialPattern[i] + center;
			}
		}

		public static IEnumerable<IntVec3> RadialCellsAround(IntVec3 center, float minRadius, float maxRadius)
		{
			int numSquares = NumCellsInRadius(maxRadius);
			for (int i = 0; i < numSquares; i++)
			{
				if (RadialPattern[i].LengthHorizontal >= minRadius)
				{
					yield return RadialPattern[i] + center;
				}
			}
		}

		public static IEnumerable<Thing> RadialDistinctThingsAround(IntVec3 center, Map map, float radius, bool useCenter)
		{
			int numCells = NumCellsInRadius(radius);
			HashSet<Thing> returnedThings = null;
			for (int j = (!useCenter) ? 1 : 0; j < numCells; j++)
			{
				IntVec3 c = RadialPattern[j] + center;
				if (!c.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing.def.size.x > 1 && thing.def.size.z > 1)
					{
						if (returnedThings == null)
						{
							returnedThings = new HashSet<Thing>();
						}
						if (returnedThings.Contains(thing))
						{
							continue;
						}
						returnedThings.Add(thing);
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
					if (map == null || intVec.InBounds(map))
					{
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
}
