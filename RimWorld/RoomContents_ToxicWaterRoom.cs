using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_ToxicWaterRoom : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnPool(map, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnPool(Map map, LayoutRoom room)
	{
		CellRect cellRect = room.rects.MaxBy((CellRect r) => r.Area);
		int numCells = Mathf.RoundToInt(Mathf.Min(cellRect.Width, cellRect.Height) * 2);
		List<IntVec3> list = GridShapeMaker.IrregularLump(cellRect.CenterCell, map, numCells, Validator);
		for (int num = 0; num < list.Count; num++)
		{
			map.terrainGrid.SetTerrain(list[num], TerrainDefOf.ToxicWaterShallow);
		}
		foreach (IntVec3 cell in room.Cells)
		{
			if (cell.GetEdifice(map) == null && room.Contains(cell, 1) && !cell.GetTerrain(map).IsWater && RoomGenUtility.IsAdjacentTo(cell, IsWaterCell))
			{
				map.terrainGrid.SetTerrain(cell, TerrainDefOf.Gravel);
			}
		}
		bool IsWaterCell(IntVec3 p)
		{
			if (p.GetTerrain(map).IsWater)
			{
				return room.Contains(p, 1);
			}
			return false;
		}
		bool Validator(IntVec3 cell)
		{
			return room.Contains(cell, 2);
		}
	}
}
