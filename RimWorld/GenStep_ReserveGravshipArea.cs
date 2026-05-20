using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class GenStep_ReserveGravshipArea : GenStep
{
	private const int ClearEdgeRadius = 2;

	public override int SeedPart => 2031751232;

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		Gravship gravship = parms.gravship;
		if (gravship == null || (MapGenerator.TryGetVar<bool>("GravshipSpawnSet", out var var) && var))
		{
			return;
		}
		List<CellRect> orGenerateVar = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
		HashSet<IntVec3> cellsAdjacentToSubstructure = GravshipPlacementUtility.GetCellsAdjacentToSubstructure(gravship.OccupiedRects, 2);
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		for (int i = 0; i < cellsAdjacentToSubstructure.Count / 10; i++)
		{
			List<IntVec3> list = GridShapeMaker.IrregularLumpRelative(Rand.Range(50, 180));
			IntVec3 intVec = cellsAdjacentToSubstructure.RandomElement();
			foreach (IntVec3 item in list)
			{
				hashSet.Add(item + intVec);
			}
		}
		HashSet<IntVec3> hashSet2 = new HashSet<IntVec3>();
		hashSet2.AddRange(cellsAdjacentToSubstructure);
		hashSet2.AddRange(hashSet);
		List<IntVec3> exclusionCells = new List<IntVec3>();
		foreach (var (thing2, data2) in gravship.ThrusterPlacements)
		{
			thing2.def.GetCompProperties<CompProperties_GravshipThruster>().GetExclusionZone(data2.local, data2.rotation, ref exclusionCells);
			hashSet2.AddRange(exclusionCells);
		}
		bool enabled = map.regionAndRoomUpdater.Enabled;
		map.regionAndRoomUpdater.Enabled = true;
		if (!MapGenerator.PlayerStartSpotValid)
		{
			SetStartSpot(map, hashSet2, orGenerateVar);
		}
		IntVec3 playerStartSpot = MapGenerator.PlayerStartSpot;
		foreach (CellRect occupiedRect in gravship.OccupiedRects)
		{
			orGenerateVar.Add(occupiedRect.MovedBy(playerStartSpot).ExpandedBy(2));
		}
		GravshipPlacementUtility.ClearArea(map, playerStartSpot, cellsAdjacentToSubstructure, GravshipPlacementUtility.ClearMode.AllButNonTreePlants);
		GravshipPlacementUtility.ClearArea(map, playerStartSpot, hashSet, GravshipPlacementUtility.ClearMode.NaturalRockOnly);
		map.regionAndRoomUpdater.Enabled = enabled;
	}

	public static void SetStartSpot(Map map, HashSet<IntVec3> cells, List<CellRect> usedRects)
	{
		int i = 0;
		int num = int.MaxValue;
		IntVec3 intVec = IntVec3.Invalid;
		for (; i < 50; i++)
		{
			IntVec3 intVec2 = CellFinderLoose.TryFindCentralCell(map, 2, 30, null, returnInvalidOnFail: true);
			if (!intVec2.IsValid)
			{
				continue;
			}
			int num2 = CostToPlaceAt(intVec2, cells, map, usedRects);
			if (num2 < num)
			{
				num = num2;
				intVec = intVec2;
				if (num == 0)
				{
					break;
				}
			}
		}
		if (num > cells.Count * 50)
		{
			for (i = 0; i < 100; i++)
			{
				if (!CellFinder.TryFindRandomCell(map, (IntVec3 c) => !usedRects.Any((CellRect r) => r.Contains(c)), out var result))
				{
					continue;
				}
				int num3 = CostToPlaceAt(result, cells, map, usedRects);
				if (num3 < num)
				{
					num = num3;
					intVec = result;
					if (num == 0)
					{
						break;
					}
				}
			}
		}
		if (intVec != IntVec3.Invalid)
		{
			MapGenerator.PlayerStartSpot = intVec;
		}
	}

	private static int CostToPlaceAt(IntVec3 center, HashSet<IntVec3> cells, Map map, List<CellRect> usedRects)
	{
		int num = 0;
		int num2 = 0;
		foreach (IntVec3 cell2 in cells)
		{
			IntVec3 cell = center + cell2;
			if (!cell.InBounds(map) || cell.InNoBuildEdgeArea(map) || map.landingBlockers.Any((CellRect x) => x.Contains(cell)))
			{
				return int.MaxValue;
			}
			if (usedRects.Any((CellRect x) => x.Contains(cell)))
			{
				num2 += 100000;
			}
			if (cell.GetRoof(map) != null)
			{
				num2 += 20;
			}
			if (cell.Fogged(map))
			{
				num2 += 20;
			}
			Building edifice = cell.GetEdifice(map);
			if (edifice != null)
			{
				num2 += (edifice.def.building.isNaturalRock ? 80 : 40);
				continue;
			}
			TerrainDef terrain = cell.GetTerrain(map);
			if (terrain == TerrainDefOf.Space)
			{
				num++;
				num2 += 5;
			}
			else if (terrain.passability == Traversability.Impassable)
			{
				num2 += 200;
			}
			else if (terrain.dangerous)
			{
				num2 += 80;
			}
			else if (terrain.IsWater)
			{
				num2 += 60;
			}
			else if (!cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy) && !cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Medium) && !cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Light))
			{
				num2 += 30;
			}
		}
		float num3 = (float)num / (float)cells.Count;
		if (num == cells.Count)
		{
			num2 += 10000;
		}
		else if (num3 > 0.9f)
		{
			num2 += 1000;
		}
		return num2;
	}
}
