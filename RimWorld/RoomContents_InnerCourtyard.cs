using System.Linq;
using Verse;

namespace RimWorld;

public class RoomContents_InnerCourtyard : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		CellRect rect = room.rects[0];
		foreach (IntVec3 edgeCell in rect.ContractedBy(3).EdgeCells)
		{
			if (!ContainedWithinOtherRect(rect, edgeCell, room, 1))
			{
				SpawnWall(edgeCell, map, room);
			}
		}
		foreach (IntVec3 cell in rect.ContractedBy(4).Cells)
		{
			if (!ContainedWithinOtherRect(rect, cell, room, 1))
			{
				TerrainDef newTerr = (map.Biome.inVacuum ? TerrainDefOf.Space : TerrainDefOf.PackedDirt);
				map.terrainGrid.SetTerrain(cell, newTerr);
				map.roofGrid.SetRoof(cell, null);
			}
		}
		TryDivideRoom(map, room, rect);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private static void TryDivideRoom(Map map, LayoutRoom room, CellRect rect)
	{
		CellRect cellRect = rect.ContractedBy(3);
		for (int i = 0; i < 2; i++)
		{
			Rot4 rot = ((i == 0) ? Rot4.North : Rot4.East);
			Rot4 rot2 = rot.Rotated(RotationDirection.Clockwise);
			if (cellRect.GetSideLength(rot) <= 8)
			{
				continue;
			}
			IntVec3 centerCellOnEdge = cellRect.GetCenterCellOnEdge(rot.Opposite);
			CellRect cellRect2 = CellRect.FromLimits(cellRect.GetCenterCellOnEdge(rot2.Opposite), cellRect.GetCenterCellOnEdge(rot2));
			for (; cellRect.Contains(centerCellOnEdge); centerCellOnEdge += rot.FacingCell)
			{
				map.terrainGrid.SetTerrain(centerCellOnEdge, room.sketch.layoutSketch.floor);
				map.roofGrid.SetRoof(centerCellOnEdge, RoofDefOf.RoofConstructed);
				centerCellOnEdge.GetEdifice(map)?.Destroy();
				for (int j = 0; j < 2; j++)
				{
					IntVec3 intVec = centerCellOnEdge + ((j == 0) ? rot2 : rot2.Opposite).FacingCell;
					if (!cellRect2.Contains(intVec) || cellRect.GetSideLength(rot2) <= 8)
					{
						SpawnWall(intVec, map, room);
					}
				}
			}
		}
	}

	protected override bool IsValidCellBase(ThingDef thing, ThingDef stuff, IntVec3 cell, LayoutRoom room, Map map)
	{
		if (map.roofGrid.RoofAt(cell) != null)
		{
			return base.IsValidCellBase(thing, stuff, cell, room, map);
		}
		return false;
	}

	private static void SpawnWall(IntVec3 cell, Map map, LayoutRoom room)
	{
		map.terrainGrid.SetTerrain(cell, room.sketch.layoutSketch.floor);
		map.roofGrid.SetRoof(cell, RoofDefOf.RoofConstructed);
		GenSpawn.Spawn(ThingDefOf.AncientFortifiedWall, cell, map);
	}

	private static bool ContainedWithinOtherRect(CellRect rect, IntVec3 cell, LayoutRoom room, int contractedBy)
	{
		if (!rect.EdgeCells.Contains(cell))
		{
			return false;
		}
		return true;
	}
}
