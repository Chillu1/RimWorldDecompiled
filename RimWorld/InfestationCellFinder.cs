using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class InfestationCellFinder
{
	private static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();

	private static Dictionary<Region, float> regionsDistanceToUnroofed = new Dictionary<Region, float>();

	private static ByteGrid closedAreaSize;

	private const float MinRequiredScore = 7.5f;

	private const float MinMountainousnessScore = 0.17f;

	private const int MountainousnessScoreRadialPatternIdx = 700;

	private const int MountainousnessScoreRadialPatternSkip = 10;

	private const float MountainousnessScorePerRock = 1f;

	private const float MountainousnessScorePerThickRoof = 0.5f;

	private const float MinCellTempToSpawnHive = -17f;

	private const float MaxDistanceToColonyBuilding = 30f;

	private static List<Pair<IntVec3, float>> tmpCachedInfestationChanceCellColors;

	private static HashSet<Region> tempUnroofedRegions = new HashSet<Region>();

	public static bool TryFindCell(out IntVec3 cell, Map map)
	{
		CalculateLocationCandidates(map);
		if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out var result))
		{
			cell = IntVec3.Invalid;
			return false;
		}
		cell = CellFinder.FindNoWipeSpawnLocNear(result.cell, map, ThingDefOf.Hive, Rot4.North, 2, (IntVec3 x) => GetScoreAt(x, map) > 0f && x.GetFirstThing(map, ThingDefOf.Hive) == null && x.GetFirstThing(map, ThingDefOf.TunnelHiveSpawner) == null);
		return true;
	}

	private static float GetScoreAt(IntVec3 cell, Map map)
	{
		if ((float)(int)CellFinderUtility.DistToColonyBuilding[cell] > 30f)
		{
			return 0f;
		}
		if (!cell.Walkable(map))
		{
			return 0f;
		}
		if (cell.Fogged(map))
		{
			return 0f;
		}
		if (CellHasBlockingThings(cell, map))
		{
			return 0f;
		}
		if (!cell.Roofed(map) || !cell.GetRoof(map).isThickRoof)
		{
			return 0f;
		}
		Region region = cell.GetRegion(map);
		if (region == null)
		{
			return 0f;
		}
		if (closedAreaSize[cell] < 2)
		{
			return 0f;
		}
		float temperature = cell.GetTemperature(map);
		if (temperature < -17f)
		{
			return 0f;
		}
		float mountainousnessScoreAt = GetMountainousnessScoreAt(cell, map);
		if (mountainousnessScoreAt < 0.17f)
		{
			return 0f;
		}
		int num = StraightLineDistToUnroofed(cell, map);
		float f = (regionsDistanceToUnroofed.TryGetValue(region, out f) ? Mathf.Min(f, (float)num * 4f) : ((float)num * 1.15f));
		f = Mathf.Pow(f, 1.55f);
		float num2 = Mathf.InverseLerp(0f, 12f, num);
		float num3 = Mathf.Lerp(1f, 0.18f, map.glowGrid.GroundGlowAt(cell));
		float num4 = 1f - Mathf.Clamp(DistToBlocker(cell, map) / 11f, 0f, 0.6f);
		float num5 = Mathf.InverseLerp(-17f, -7f, temperature);
		float f2 = f * num2 * num4 * mountainousnessScoreAt * num3 * num5;
		f2 = Mathf.Pow(f2, 1.2f);
		if (f2 < 7.5f)
		{
			return 0f;
		}
		return f2;
	}

	public static void DebugDraw()
	{
		if (DebugViewSettings.drawInfestationChance)
		{
			if (tmpCachedInfestationChanceCellColors == null)
			{
				tmpCachedInfestationChanceCellColors = new List<Pair<IntVec3, float>>();
			}
			if (Time.frameCount % 8 == 0)
			{
				tmpCachedInfestationChanceCellColors.Clear();
				Map currentMap = Find.CurrentMap;
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				currentViewRect.ClipInsideMap(currentMap);
				currentViewRect = currentViewRect.ExpandedBy(1);
				CalculateTraversalDistancesToUnroofed(currentMap);
				CalculateClosedAreaSizeGrid(currentMap);
				CellFinderUtility.CalculateDistanceToColonyBuildingGrid(currentMap);
				float num = 0.001f;
				for (int i = 0; i < currentMap.Size.z; i++)
				{
					for (int j = 0; j < currentMap.Size.x; j++)
					{
						float scoreAt = GetScoreAt(new IntVec3(j, 0, i), currentMap);
						if (scoreAt > num)
						{
							num = scoreAt;
						}
					}
				}
				for (int k = 0; k < currentMap.Size.z; k++)
				{
					for (int l = 0; l < currentMap.Size.x; l++)
					{
						IntVec3 intVec = new IntVec3(l, 0, k);
						if (currentViewRect.Contains(intVec))
						{
							float scoreAt2 = GetScoreAt(intVec, currentMap);
							if (!(scoreAt2 <= 7.5f))
							{
								float second = GenMath.LerpDouble(7.5f, num, 0f, 1f, scoreAt2);
								tmpCachedInfestationChanceCellColors.Add(new Pair<IntVec3, float>(intVec, second));
							}
						}
					}
				}
			}
			for (int m = 0; m < tmpCachedInfestationChanceCellColors.Count; m++)
			{
				IntVec3 first = tmpCachedInfestationChanceCellColors[m].First;
				float second2 = tmpCachedInfestationChanceCellColors[m].Second;
				CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2)));
			}
		}
		else
		{
			tmpCachedInfestationChanceCellColors = null;
		}
	}

	private static void CalculateLocationCandidates(Map map)
	{
		locationCandidates.Clear();
		CalculateTraversalDistancesToUnroofed(map);
		CalculateClosedAreaSizeGrid(map);
		CellFinderUtility.CalculateDistanceToColonyBuildingGrid(map);
		for (int i = 0; i < map.Size.z; i++)
		{
			for (int j = 0; j < map.Size.x; j++)
			{
				IntVec3 cell = new IntVec3(j, 0, i);
				float scoreAt = GetScoreAt(cell, map);
				if (!(scoreAt <= 0f))
				{
					locationCandidates.Add(new LocationCandidate(cell, scoreAt));
				}
			}
		}
	}

	private static bool CellHasBlockingThings(IntVec3 cell, Map map)
	{
		List<Thing> thingList = cell.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Pawn || thingList[i] is Hive || thingList[i] is TunnelHiveSpawner)
			{
				return true;
			}
			if (thingList[i].def.category == ThingCategory.Building && thingList[i].def.passability == Traversability.Impassable && GenSpawn.SpawningWipes(ThingDefOf.Hive, thingList[i].def))
			{
				return true;
			}
		}
		return false;
	}

	private static int StraightLineDistToUnroofed(IntVec3 cell, Map map)
	{
		int num = int.MaxValue;
		for (int i = 0; i < 4; i++)
		{
			int num2 = 0;
			IntVec3 facingCell = new Rot4(i).FacingCell;
			int num3 = 0;
			while (true)
			{
				IntVec3 intVec = cell + facingCell * num3;
				if (!intVec.InBounds(map))
				{
					num2 = int.MaxValue;
					break;
				}
				num2 = num3;
				if (NoRoofAroundAndWalkable(intVec, map))
				{
					break;
				}
				num3++;
			}
			if (num2 < num)
			{
				num = num2;
			}
		}
		if (num == int.MaxValue)
		{
			return map.Size.x;
		}
		return num;
	}

	private static float DistToBlocker(IntVec3 cell, Map map)
	{
		int num = int.MinValue;
		int num2 = int.MinValue;
		for (int i = 0; i < 4; i++)
		{
			int num3 = 0;
			IntVec3 facingCell = new Rot4(i).FacingCell;
			int num4 = 0;
			while (true)
			{
				IntVec3 c = cell + facingCell * num4;
				num3 = num4;
				if (!c.InBounds(map) || !c.Walkable(map))
				{
					break;
				}
				num4++;
			}
			if (num3 > num)
			{
				num2 = num;
				num = num3;
			}
			else if (num3 > num2)
			{
				num2 = num3;
			}
		}
		return Mathf.Min(num, num2);
	}

	private static bool NoRoofAroundAndWalkable(IntVec3 cell, Map map)
	{
		if (!cell.Walkable(map))
		{
			return false;
		}
		if (cell.Roofed(map))
		{
			return false;
		}
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = new Rot4(i).FacingCell + cell;
			if (c.InBounds(map) && c.Roofed(map))
			{
				return false;
			}
		}
		return true;
	}

	private static float GetMountainousnessScoreAt(IntVec3 cell, Map map)
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < 700; i += 10)
		{
			IntVec3 c = cell + GenRadial.RadialPattern[i];
			if (c.InBounds(map))
			{
				Building edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.category == ThingCategory.Building && edifice.def.building.isNaturalRock)
				{
					num += 1f;
				}
				else if (c.Roofed(map) && c.GetRoof(map).isThickRoof)
				{
					num += 0.5f;
				}
				num2++;
			}
		}
		return num / (float)num2;
	}

	private static void CalculateTraversalDistancesToUnroofed(Map map)
	{
		tempUnroofedRegions.Clear();
		for (int i = 0; i < map.Size.z; i++)
		{
			for (int j = 0; j < map.Size.x; j++)
			{
				IntVec3 intVec = new IntVec3(j, 0, i);
				Region region = intVec.GetRegion(map);
				if (region != null && NoRoofAroundAndWalkable(intVec, map))
				{
					tempUnroofedRegions.Add(region);
				}
			}
		}
		Dijkstra<Region>.Run(tempUnroofedRegions, (Region x) => x.Neighbors, (Region a, Region b) => Mathf.Sqrt(a.extentsClose.CenterCell.DistanceToSquared(b.extentsClose.CenterCell)), regionsDistanceToUnroofed);
		tempUnroofedRegions.Clear();
	}

	private static void CalculateClosedAreaSizeGrid(Map map)
	{
		if (closedAreaSize == null)
		{
			closedAreaSize = new ByteGrid(map);
		}
		else
		{
			closedAreaSize.ClearAndResizeTo(map);
		}
		for (int i = 0; i < map.Size.z; i++)
		{
			for (int j = 0; j < map.Size.x; j++)
			{
				IntVec3 intVec = new IntVec3(j, 0, i);
				if (closedAreaSize[j, i] == 0 && !intVec.Impassable(map))
				{
					int area = 0;
					map.floodFiller.FloodFill(intVec, (Predicate<IntVec3>)((IntVec3 c) => !c.Impassable(map)), (Action<IntVec3>)delegate
					{
						area++;
					}, int.MaxValue, rememberParents: false, (IEnumerable<IntVec3>)null);
					area = Mathf.Min(area, 255);
					map.floodFiller.FloodFill(intVec, (IntVec3 c) => !c.Impassable(map), delegate(IntVec3 c)
					{
						closedAreaSize[c] = (byte)area;
					});
				}
			}
		}
	}
}
