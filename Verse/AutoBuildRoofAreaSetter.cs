using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class AutoBuildRoofAreaSetter
{
	private Map map;

	private List<Room> queuedGenerateRooms = new List<Room>();

	private HashSet<IntVec3> cellsToRoof = new HashSet<IntVec3>();

	private HashSet<IntVec3> innerCells = new HashSet<IntVec3>();

	private List<IntVec3> justRoofedCells = new List<IntVec3>();

	public AutoBuildRoofAreaSetter(Map map)
	{
		this.map = map;
	}

	public void TryGenerateAreaFor(Room room)
	{
		queuedGenerateRooms.Add(room);
	}

	public void AutoBuildRoofAreaSetterTick_First()
	{
		ResolveQueuedGenerateRoofs();
	}

	public void ResolveQueuedGenerateRoofs()
	{
		for (int i = 0; i < queuedGenerateRooms.Count; i++)
		{
			TryGenerateAreaNow(queuedGenerateRooms[i]);
		}
		queuedGenerateRooms.Clear();
	}

	private void TryGenerateAreaNow(Room room)
	{
		map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
		if (room.Dereferenced || room.TouchesMapEdge || room.RegionCount > 26 || room.CellCount > 320 || room.IsDoorway)
		{
			return;
		}
		bool flag = false;
		foreach (IntVec3 borderCell in room.BorderCells)
		{
			Thing roofHolderOrImpassable = borderCell.GetRoofHolderOrImpassable(map);
			if (roofHolderOrImpassable != null)
			{
				if ((roofHolderOrImpassable.Faction != null && roofHolderOrImpassable.Faction != Faction.OfPlayer) || (roofHolderOrImpassable.def.building != null && !roofHolderOrImpassable.def.building.allowAutoroof))
				{
					return;
				}
				if (roofHolderOrImpassable.Faction == Faction.OfPlayer)
				{
					flag = true;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		innerCells.Clear();
		foreach (IntVec3 cell in room.Cells)
		{
			if (!innerCells.Contains(cell))
			{
				innerCells.Add(cell);
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c = cell + GenAdj.AdjacentCells[i];
				if (!c.InBounds(map))
				{
					continue;
				}
				Thing roofHolderOrImpassable2 = c.GetRoofHolderOrImpassable(map);
				if (roofHolderOrImpassable2 == null || (roofHolderOrImpassable2.def.size.x <= 1 && roofHolderOrImpassable2.def.size.z <= 1))
				{
					continue;
				}
				CellRect cellRect = roofHolderOrImpassable2.OccupiedRect();
				cellRect.ClipInsideMap(map);
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					for (int k = cellRect.minX; k <= cellRect.maxX; k++)
					{
						IntVec3 item = new IntVec3(k, 0, j);
						if (!innerCells.Contains(item))
						{
							innerCells.Add(item);
						}
					}
				}
			}
		}
		cellsToRoof.Clear();
		foreach (IntVec3 innerCell in innerCells)
		{
			for (int l = 0; l < 9; l++)
			{
				IntVec3 intVec = innerCell + GenAdj.AdjacentCellsAndInside[l];
				if (intVec.InBounds(map) && (l == 8 || intVec.GetRoofHolderOrImpassable(map) != null) && !cellsToRoof.Contains(intVec))
				{
					cellsToRoof.Add(intVec);
				}
			}
		}
		justRoofedCells.Clear();
		foreach (IntVec3 item2 in cellsToRoof)
		{
			if (map.roofGrid.RoofAt(item2) == null && !justRoofedCells.Contains(item2) && !map.areaManager.NoRoof[item2] && RoofCollapseUtility.WithinRangeOfRoofHolder(item2, map, assumeNonNoRoofCellsAreRoofed: true))
			{
				map.areaManager.BuildRoof[item2] = true;
				justRoofedCells.Add(item2);
			}
		}
	}
}
