using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class LargeBuildingCellFinder
{
	private static int lastCalculatedCandidatesTick = -99999;

	private static List<LocationCandidate> locationCandidates = new List<LocationCandidate>();

	private const float MinRequiredScorePerCell = 0.5f;

	private const float SafeBuildingMarketValue = 50f;

	private static List<Pair<IntVec3, float>> tmpHeartChanceCellColors;

	private static float MinRequiredScore(IntVec2 size)
	{
		return 0.5f * (float)size.Area;
	}

	public static bool AnyCellFast(Map map, LargeBuildingSpawnParms parms)
	{
		for (int i = 0; i < 1000; i++)
		{
			if (GetAreaScore(CellFinder.RandomCell(map), map, parms) >= MinRequiredScore(parms.Size))
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryFindCell(out IntVec3 cell, Map map, LargeBuildingSpawnParms parms, List<CellRect> usedRects = null, Predicate<IntVec3> extraValidator = null, bool forceRecalculate = false)
	{
		if (lastCalculatedCandidatesTick != Find.TickManager.TicksGame || forceRecalculate)
		{
			using (new ProfilerBlock("CalculateLocationCandidates"))
			{
				CalculateLocationCandidates(map, parms);
			}
		}
		int num = 0;
		cell = IntVec3.Invalid;
		while (cell == IntVec3.Invalid && num < 1000)
		{
			num++;
			if (locationCandidates.TryRandomElementByWeight((LocationCandidate x) => x.score, out var result) && !IsRectUsed(GenAdj.OccupiedRect(result.cell, Rot4.North, parms.Size), usedRects, parms) && (extraValidator == null || extraValidator(result.cell)))
			{
				cell = result.cell;
			}
		}
		if (num >= 1000)
		{
			return false;
		}
		return true;
	}

	public static bool TryFindCellNear(IntVec3 root, Map map, int squareRadius, LargeBuildingSpawnParms parms, out IntVec3 cell, bool forceRecalculate = false)
	{
		if (lastCalculatedCandidatesTick != Find.TickManager.TicksGame || forceRecalculate)
		{
			using (new ProfilerBlock("CalculateLocationCandidates"))
			{
				CalculateLocationCandidates(map, parms);
			}
		}
		int minX = root.x - squareRadius;
		int maxX = root.x + squareRadius;
		int minZ = root.z - squareRadius;
		int maxZ = root.z + squareRadius;
		if (!locationCandidates.TryRandomElementByWeight((LocationCandidate x) => (x.cell.x < minX || x.cell.x > maxX || x.cell.z < minZ || x.cell.z > maxZ) ? 0f : x.score, out var result))
		{
			cell = IntVec3.Invalid;
			return false;
		}
		cell = result.cell;
		return true;
	}

	private static bool IsRectUsed(CellRect rect, List<CellRect> usedRects, LargeBuildingSpawnParms parms)
	{
		if (usedRects.NullOrEmpty())
		{
			return false;
		}
		foreach (CellRect usedRect in usedRects)
		{
			if (usedRect.Overlaps(rect))
			{
				return true;
			}
		}
		if (parms.minDistanceFromUsedRects > 0f)
		{
			foreach (CellRect usedRect2 in usedRects)
			{
				if (usedRect2.CenterCell.InHorDistOf(rect.CenterCell, parms.minDistanceFromUsedRects))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static List<LocationCandidate> CalculateLocationCandidates(Map map, LargeBuildingSpawnParms parms)
	{
		locationCandidates.Clear();
		if (parms.maxDistanceToColonyBuilding > 0f || parms.minDistanceToColonyBuilding > 0f || parms.preferFarFromColony)
		{
			CellFinderUtility.CalculateDistanceToColonyBuildingGrid(map);
		}
		int num = Mathf.Max(parms.Size.x / 2, 1);
		int num2 = Mathf.Max(parms.Size.z / 2, 1);
		for (int i = 0; i < map.Size.z; i += num2)
		{
			for (int j = 0; j < map.Size.x; j += num)
			{
				IntVec3 intVec = new IntVec3(j, 0, i);
				float areaScore = GetAreaScore(intVec, map, parms);
				if (areaScore >= MinRequiredScore(parms.Size))
				{
					locationCandidates.Add(new LocationCandidate(intVec + (parms.Size / 2).ToIntVec3, areaScore));
				}
			}
		}
		lastCalculatedCandidatesTick = Find.TickManager.TicksGame;
		return locationCandidates;
	}

	private static float GetAreaScore(IntVec3 topLeftCell, Map map, LargeBuildingSpawnParms parms)
	{
		float num = 0f;
		IntVec2 size = parms.Size;
		for (int i = 0; i < size.x; i++)
		{
			for (int j = 0; j < size.z; j++)
			{
				IntVec3 intVec = topLeftCell + new IntVec3(i, 0, j);
				if (!intVec.InBounds(map))
				{
					return 0f;
				}
				if (!parms.canSpawnOnImpassable && !intVec.Walkable(map))
				{
					return 0f;
				}
				float cellScore = GetCellScore(intVec, map, parms);
				if (cellScore <= 0f)
				{
					return 0f;
				}
				num += cellScore;
			}
		}
		if (parms.preferFarFromColony && (float)(int)CellFinderUtility.DistToColonyBuilding[topLeftCell] < 50f)
		{
			num *= 0.5f;
		}
		return num;
	}

	private static float GetCellScore(IntVec3 cell, Map map, LargeBuildingSpawnParms parms)
	{
		float num = 1f;
		if (!parms.ignoreTerrainAffordance && !cell.SupportsStructureType(map, parms.thingDef.terrainAffordanceNeeded))
		{
			return 0f;
		}
		if (!parms.ignoreFoundations && map.terrainGrid.FoundationAt(cell) != null)
		{
			return 0f;
		}
		if (parms.maxDistanceToColonyBuilding > 0f && (float)(int)CellFinderUtility.DistToColonyBuilding[cell] > parms.maxDistanceToColonyBuilding)
		{
			return 0f;
		}
		if (parms.minDistanceToColonyBuilding > 0f && (float)(int)CellFinderUtility.DistToColonyBuilding[cell] < parms.minDistanceToColonyBuilding)
		{
			return 0f;
		}
		if (parms.minDistToEdge > 0 && cell.DistanceToEdge(map) < parms.minDistToEdge)
		{
			return 0f;
		}
		if (parms.maxDistanceFromPlayerStartPosition > 0f && !cell.InHorDistOf(MapGenerator.PlayerStartSpot, parms.maxDistanceFromPlayerStartPosition))
		{
			return 0f;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && edifice.def?.building?.isNaturalRock == true)
		{
			return 0f;
		}
		if (!parms.allowFogged && cell.Fogged(map))
		{
			return 0f;
		}
		if (cell.GetTerrain(map).IsWater)
		{
			return 0f;
		}
		if (!parms.canSpawnOnImpassable && !cell.Walkable(map))
		{
			return 0f;
		}
		foreach (Thing thing in cell.GetThingList(map))
		{
			if (thing.def.preventSkyfallersLandingOn)
			{
				return 0f;
			}
			if (thing is Building building)
			{
				if (!building.def.Minifiable && building.MarketValue > 50f)
				{
					return 0f;
				}
				if (!building.def.destroyable || !building.def.building.IsDeconstructible)
				{
					return 0f;
				}
				if (parms.attemptNotUnderBuildings)
				{
					num *= 0.5f;
				}
			}
		}
		switch (parms.attemptSpawnLocationType)
		{
		case SpawnLocationType.Indoors:
			if (cell.GetRoom(map).PsychologicallyOutdoors)
			{
				num *= 0.01f;
			}
			break;
		case SpawnLocationType.Outdoors:
			if (!cell.GetRoom(map).PsychologicallyOutdoors)
			{
				num *= 0.01f;
			}
			break;
		}
		return num;
	}

	public static void DebugDraw()
	{
		if (DebugViewSettings.drawFleshmassHeartChance)
		{
			if (tmpHeartChanceCellColors == null)
			{
				tmpHeartChanceCellColors = new List<Pair<IntVec3, float>>();
			}
			if (Time.frameCount % 60 == 0)
			{
				tmpHeartChanceCellColors.Clear();
				Map currentMap = Find.CurrentMap;
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				currentViewRect.ClipInsideMap(currentMap);
				currentViewRect = currentViewRect.ExpandedBy(1);
				CellFinderUtility.CalculateDistanceToColonyBuildingGrid(currentMap);
				List<LocationCandidate> list = CalculateLocationCandidates(currentMap, IncidentWorker_FleshmassHeart.HeartSpawnParms.ForThing(ThingDefOf.FleshmassHeart));
				if (list.NullOrEmpty())
				{
					return;
				}
				float inTo = list.Max((LocationCandidate c) => c.score);
				foreach (LocationCandidate item in list)
				{
					if (currentViewRect.Contains(item.cell) && !(item.score < MinRequiredScore(ThingDefOf.FleshmassHeart.size)))
					{
						float second = GenMath.LerpDouble(MinRequiredScore(ThingDefOf.FleshmassHeart.size) - 1f, inTo, 0.25f, 0.5f, item.score);
						tmpHeartChanceCellColors.Add(new Pair<IntVec3, float>(item.cell, second));
					}
				}
			}
			for (int num = 0; num < tmpHeartChanceCellColors.Count; num++)
			{
				IntVec3 first = tmpHeartChanceCellColors[num].First;
				float second2 = tmpHeartChanceCellColors[num].Second;
				CellRenderer.RenderCell(first, SolidColorMaterials.SimpleSolidColorMaterial(new Color(0f, 0f, 1f, second2)));
			}
		}
		else
		{
			tmpHeartChanceCellColors = null;
		}
	}
}
