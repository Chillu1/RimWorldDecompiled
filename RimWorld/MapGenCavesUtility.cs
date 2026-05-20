using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public static class MapGenCavesUtility
{
	public struct CaveGenParms
	{
		public float openTunnelsPer10k;

		public float closedTunnelsPer10k;

		public int maxOpenTunnelsPerRockGroup;

		public int maxClosedTunnelsPerRockGroup;

		public float directionChangeSpeed;

		public int minRocksToGenerateAnyTunnel;

		public int allowBranchingAfterThisManyCells;

		public float widthOffsetPerCell;

		public float minTunnelWidth;

		public float branchChance;

		public float widthNoiseFrequency;

		public float widthNoiseAmplitude;

		public FloatRange branchedTunnelWidthOffset;

		public SimpleCurve tunnelsWidthPerRockCount;

		public static CaveGenParms Default => new CaveGenParms
		{
			openTunnelsPer10k = 5.8f,
			closedTunnelsPer10k = 2.5f,
			maxOpenTunnelsPerRockGroup = 3,
			maxClosedTunnelsPerRockGroup = 1,
			directionChangeSpeed = 8f,
			minRocksToGenerateAnyTunnel = 300,
			widthOffsetPerCell = 0.034f,
			minTunnelWidth = 1.4f,
			branchChance = 0.1f,
			widthNoiseFrequency = 0.05f,
			widthNoiseAmplitude = 0f,
			branchedTunnelWidthOffset = new FloatRange(0.2f, 0.4f),
			tunnelsWidthPerRockCount = new SimpleCurve
			{
				new CurvePoint(100f, 2f),
				new CurvePoint(300f, 4f),
				new CurvePoint(3000f, 5.5f)
			}
		};
	}

	private static readonly List<IntVec3> tmpCells = new List<IntVec3>();

	private static readonly HashSet<IntVec3> tmpGroupSet = new HashSet<IntVec3>();

	private static readonly HashSet<IntVec3> groupSet = new HashSet<IntVec3>();

	private static readonly HashSet<IntVec3> groupVisited = new HashSet<IntVec3>();

	private static readonly List<IntVec3> subGroup = new List<IntVec3>();

	public static void GenerateCaves(Map map, BoolGrid visited, List<IntVec3> group, ModuleBase directionNoise, CaveGenParms parms, Predicate<IntVec3> isRock)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!visited[allCell] && isRock(allCell))
			{
				group.Clear();
				map.floodFiller.FloodFill(allCell, isRock, delegate(IntVec3 x)
				{
					visited[x] = true;
					group.Add(x);
				});
				Trim(group, map);
				RemoveSmallDisconnectedSubGroups(group, map, parms);
				if (group.Count >= parms.minRocksToGenerateAnyTunnel)
				{
					DoOpenTunnels(group, map, directionNoise, parms, isRock);
					DoClosedTunnels(group, map, directionNoise, parms, isRock);
				}
			}
		}
	}

	private static void DoOpenTunnels(List<IntVec3> group, Map map, ModuleBase directionNoise, CaveGenParms parms, Predicate<IntVec3> isRock)
	{
		int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * parms.openTunnelsPer10k / 10000f);
		a = Mathf.Min(a, parms.maxOpenTunnelsPerRockGroup);
		if (a > 0)
		{
			a = Rand.RangeInclusive(1, a);
		}
		float num = parms.tunnelsWidthPerRockCount.Evaluate(group.Count);
		for (int i = 0; i < a; i++)
		{
			IntVec3 start = IntVec3.Invalid;
			float num2 = -1f;
			float dir = -1f;
			float num3 = -1f;
			for (int j = 0; j < 10; j++)
			{
				IntVec3 intVec = FindRandomEdgeCellForTunnel(group, map);
				float distToCave = GetDistToCave(intVec, group, map, 40f, treatOpenSpaceAsCave: false);
				float dist;
				float num4 = FindBestInitialDir(intVec, group, out dist);
				if (!start.IsValid || distToCave > num2 || (Mathf.Approximately(distToCave, num2) && dist > num3))
				{
					start = intVec;
					num2 = distToCave;
					dir = num4;
					num3 = dist;
				}
			}
			float width = Rand.Range(num * 0.8f, num);
			Dig(start, dir, width, group, map, closed: false, directionNoise, parms, isRock);
		}
	}

	private static void Trim(List<IntVec3> group, Map map)
	{
		GenMorphology.Open(group, 6, map);
	}

	private static void DoClosedTunnels(List<IntVec3> group, Map map, ModuleBase directionNoise, CaveGenParms parms, Predicate<IntVec3> isRock)
	{
		int a = GenMath.RoundRandom((float)group.Count * Rand.Range(0.9f, 1.1f) * parms.closedTunnelsPer10k / 10000f);
		a = Mathf.Min(a, parms.maxClosedTunnelsPerRockGroup);
		if (a > 0)
		{
			a = Rand.RangeInclusive(0, a);
		}
		float num = parms.tunnelsWidthPerRockCount.Evaluate(group.Count);
		for (int i = 0; i < a; i++)
		{
			IntVec3 start = IntVec3.Invalid;
			float num2 = -1f;
			for (int j = 0; j < 7; j++)
			{
				IntVec3 intVec = group.RandomElement();
				float distToCave = GetDistToCave(intVec, group, map, 30f, treatOpenSpaceAsCave: true);
				if (!start.IsValid || distToCave > num2)
				{
					start = intVec;
					num2 = distToCave;
				}
			}
			float width = Rand.Range(num * 0.8f, num);
			Dig(start, Rand.Range(0f, 360f), width, group, map, closed: true, directionNoise, parms, isRock);
		}
	}

	public static IntVec3 FindRandomEdgeCellForTunnel(List<IntVec3> group, Map map)
	{
		MapGenFloatGrid caves = MapGenerator.Caves;
		IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
		tmpCells.Clear();
		tmpGroupSet.Clear();
		tmpGroupSet.AddRange(group);
		for (int i = 0; i < group.Count; i++)
		{
			if (group[i].DistanceToEdge(map) < 3 || caves[group[i]] > 0f)
			{
				continue;
			}
			for (int j = 0; j < 4; j++)
			{
				IntVec3 item = group[i] + cardinalDirections[j];
				if (!tmpGroupSet.Contains(item))
				{
					tmpCells.Add(group[i]);
					break;
				}
			}
		}
		if (!tmpCells.Any())
		{
			Log.Warning("Could not find any valid edge cell.");
			return group.RandomElement();
		}
		return tmpCells.RandomElement();
	}

	public static float FindBestInitialDir(IntVec3 start, List<IntVec3> group, out float dist)
	{
		float num = GetDistToNonRock(start, group, IntVec3.East, 40);
		float num2 = GetDistToNonRock(start, group, IntVec3.West, 40);
		float num3 = GetDistToNonRock(start, group, IntVec3.South, 40);
		float num4 = GetDistToNonRock(start, group, IntVec3.North, 40);
		float num5 = GetDistToNonRock(start, group, IntVec3.NorthWest, 40);
		float num6 = GetDistToNonRock(start, group, IntVec3.NorthEast, 40);
		float num7 = GetDistToNonRock(start, group, IntVec3.SouthWest, 40);
		float num8 = GetDistToNonRock(start, group, IntVec3.SouthEast, 40);
		dist = Mathf.Max(num, num2, num3, num4, num5, num6, num7, num8);
		return GenMath.MaxByRandomIfEqual(0f, num + num8 / 2f + num6 / 2f, 45f, num8 + num3 / 2f + num / 2f, 90f, num3 + num8 / 2f + num7 / 2f, 135f, num7 + num3 / 2f + num2 / 2f, 180f, num2 + num7 / 2f + num5 / 2f, 225f, num5 + num4 / 2f + num2 / 2f, 270f, num4 + num6 / 2f + num5 / 2f, 315f, num6 + num4 / 2f + num / 2f);
	}

	public static void Dig(IntVec3 start, float dir, float width, List<IntVec3> group, Map map, bool closed, ModuleBase directionNoise, CaveGenParms parms, Predicate<IntVec3> isRock, HashSet<IntVec3> visited = null)
	{
		Vector3 vect = start.ToVector3Shifted();
		IntVec3 intVec = start;
		float num = 0f;
		MapGenFloatGrid caves = MapGenerator.Caves;
		bool flag = false;
		bool flag2 = false;
		ModuleBase moduleBase = new Perlin(parms.widthNoiseFrequency, 2.0, 0.5, 4, Rand.Int, QualityMode.Medium);
		if (visited == null)
		{
			visited = new HashSet<IntVec3>();
		}
		tmpGroupSet.Clear();
		tmpGroupSet.AddRange(group);
		int num2 = 0;
		while (true)
		{
			if (closed)
			{
				int num3 = GenRadial.NumCellsInRadius(width / 2f + 1.5f);
				for (int i = 0; i < num3; i++)
				{
					IntVec3 intVec2 = intVec + GenRadial.RadialPattern[i];
					if (!visited.Contains(intVec2) && (!tmpGroupSet.Contains(intVec2) || caves[intVec2] > 0f))
					{
						return;
					}
				}
			}
			if (num2 >= parms.allowBranchingAfterThisManyCells && width > parms.minTunnelWidth + parms.branchedTunnelWidthOffset.max)
			{
				if (!flag && Rand.Chance(parms.branchChance))
				{
					DigInBestDirection(intVec, dir, new FloatRange(40f, 90f), width - parms.branchedTunnelWidthOffset.RandomInRange, group, map, closed, directionNoise, parms, isRock, visited);
					flag = true;
				}
				if (!flag2 && Rand.Chance(parms.branchChance))
				{
					DigInBestDirection(intVec, dir, new FloatRange(-90f, -40f), width - parms.branchedTunnelWidthOffset.RandomInRange, group, map, closed, directionNoise, parms, isRock, visited);
					flag2 = true;
				}
			}
			float tunnelWidth = Mathf.Max(width + moduleBase.GetValue(intVec) * parms.widthNoiseAmplitude, 1f);
			SetCaveAround(intVec, tunnelWidth, map, visited, out var hitAnotherTunnel, isRock);
			if (hitAnotherTunnel)
			{
				break;
			}
			while (vect.ToIntVec3() == intVec)
			{
				vect += Vector3Utility.FromAngleFlat(dir) * 0.5f;
				num += 0.5f;
			}
			if (tmpGroupSet.Contains(vect.ToIntVec3()))
			{
				IntVec3 intVec3 = new IntVec3(intVec.x, 0, vect.ToIntVec3().z);
				if (isRock(intVec3))
				{
					caves[intVec3] = Mathf.Max(caves[intVec3], width);
					visited.Add(intVec3);
				}
				intVec = vect.ToIntVec3();
				dir += (float)directionNoise.GetValue(num * 60f, (float)start.x * 200f, (float)start.z * 200f) * parms.directionChangeSpeed;
				width -= parms.widthOffsetPerCell;
				if (!(width < parms.minTunnelWidth))
				{
					num2++;
					continue;
				}
				break;
			}
			break;
		}
	}

	private static void DigInBestDirection(IntVec3 curIntVec, float curDir, FloatRange dirOffset, float width, List<IntVec3> group, Map map, bool closed, ModuleBase directionNoise, CaveGenParms parms, Predicate<IntVec3> isRock, HashSet<IntVec3> visited = null)
	{
		int num = -1;
		float dir = -1f;
		for (int i = 0; i < 6; i++)
		{
			float num2 = curDir + dirOffset.RandomInRange;
			int distToNonRock = GetDistToNonRock(curIntVec, group, num2, 50);
			if (distToNonRock > num)
			{
				num = distToNonRock;
				dir = num2;
			}
		}
		if (num >= 18)
		{
			Dig(curIntVec, dir, width, group, map, closed, directionNoise, parms, isRock, visited);
		}
	}

	private static void SetCaveAround(IntVec3 around, float tunnelWidth, Map map, HashSet<IntVec3> visited, out bool hitAnotherTunnel, Predicate<IntVec3> isRock)
	{
		hitAnotherTunnel = false;
		int num = GenRadial.NumCellsInRadius(tunnelWidth / 2f);
		MapGenFloatGrid caves = MapGenerator.Caves;
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = around + GenRadial.RadialPattern[i];
			if (isRock(intVec))
			{
				if (caves[intVec] > 0f && !visited.Contains(intVec))
				{
					hitAnotherTunnel = true;
				}
				caves[intVec] = Mathf.Max(caves[intVec], tunnelWidth);
				visited.Add(intVec);
			}
		}
	}

	private static int GetDistToNonRock(IntVec3 from, List<IntVec3> group, IntVec3 offset, int maxDist)
	{
		groupSet.Clear();
		groupSet.AddRange(group);
		for (int i = 0; i <= maxDist; i++)
		{
			IntVec3 item = from + offset * i;
			if (!groupSet.Contains(item))
			{
				return i;
			}
		}
		return maxDist;
	}

	private static int GetDistToNonRock(IntVec3 from, List<IntVec3> group, float dir, int maxDist)
	{
		groupSet.Clear();
		groupSet.AddRange(group);
		Vector3 vector = Vector3Utility.FromAngleFlat(dir);
		for (int i = 0; i <= maxDist; i++)
		{
			IntVec3 item = (from.ToVector3Shifted() + vector * i).ToIntVec3();
			if (!groupSet.Contains(item))
			{
				return i;
			}
		}
		return maxDist;
	}

	public static float GetDistToCave(IntVec3 cell, List<IntVec3> group, Map map, float maxDist, bool treatOpenSpaceAsCave)
	{
		MapGenFloatGrid caves = MapGenerator.Caves;
		tmpGroupSet.Clear();
		tmpGroupSet.AddRange(group);
		int num = GenRadial.NumCellsInRadius(maxDist);
		IntVec3[] radialPattern = GenRadial.RadialPattern;
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = cell + radialPattern[i];
			if ((treatOpenSpaceAsCave && !tmpGroupSet.Contains(intVec)) || (intVec.InBounds(map) && caves[intVec] > 0f))
			{
				return cell.DistanceTo(intVec);
			}
		}
		return maxDist;
	}

	private static void RemoveSmallDisconnectedSubGroups(List<IntVec3> group, Map map, CaveGenParms parms)
	{
		groupSet.Clear();
		groupSet.AddRange(group);
		groupVisited.Clear();
		for (int i = 0; i < group.Count; i++)
		{
			if (groupVisited.Contains(group[i]) || !groupSet.Contains(group[i]))
			{
				continue;
			}
			subGroup.Clear();
			map.floodFiller.FloodFill(group[i], (IntVec3 x) => groupSet.Contains(x), delegate(IntVec3 x)
			{
				subGroup.Add(x);
				groupVisited.Add(x);
			});
			if (subGroup.Count < parms.minRocksToGenerateAnyTunnel || (float)subGroup.Count < 0.05f * (float)group.Count)
			{
				for (int num = 0; num < subGroup.Count; num++)
				{
					groupSet.Remove(subGroup[num]);
				}
			}
		}
		group.Clear();
		group.AddRange(groupSet);
	}
}
