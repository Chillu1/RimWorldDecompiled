using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_NarrowHalls : RoomContentsWorker
{
	private const int LightEachCells = 5;

	private static readonly IntRange PlatformRange = new IntRange(1, 2);

	private static readonly Dictionary<int, List<IntVec3>> pathwayLists = new Dictionary<int, List<IntVec3>>();

	public override void PreFillRooms(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		ComputePathways(GetPathwayList(room), map, room);
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		CellRect rect = room.rects[0];
		List<IntVec3> pathwayList = GetPathwayList(room);
		SpawnPathways(pathwayList, map, room);
		FillSpace(pathwayList, map, rect);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private List<IntVec3> GetPathwayList(LayoutRoom room)
	{
		if (!pathwayLists.TryGetValue(room.GetHashCode(), out var value))
		{
			value = new List<IntVec3>();
			pathwayLists[room.GetHashCode()] = value;
		}
		return value;
	}

	public override void PostFillRooms(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.PostFillRooms(map, room, faction, threatPoints);
		pathwayLists.Clear();
	}

	protected override bool CanRemoveWall(IntVec3 cell, Map map, LayoutRoom room)
	{
		foreach (KeyValuePair<int, List<IntVec3>> pathwayList in pathwayLists)
		{
			pathwayList.Deconstruct(out var _, out var value);
			foreach (IntVec3 item in value)
			{
				if (cell.AdjacentToDiagonal(item))
				{
					return false;
				}
			}
		}
		return true;
	}

	protected override bool IsValidCellBase(ThingDef thing, ThingDef stuff, IntVec3 cell, LayoutRoom room, Map map)
	{
		if (cell.Roofed(map))
		{
			return base.IsValidCellBase(thing, stuff, cell, room, map);
		}
		return false;
	}

	protected override bool IsValidWallAttachmentCell(LayoutWallAttatchmentParms parms, IntVec3 cell, Rot4 rot, LayoutRoom room, Map map)
	{
		if (GetPathwayList(room).Contains(cell))
		{
			return base.IsValidWallAttachmentCell(parms, cell, rot, room, map);
		}
		return false;
	}

	private static void ComputePathways(List<IntVec3> pathway, Map map, LayoutRoom room)
	{
		pathway.Clear();
		List<IntVec3> list = new List<IntVec3>();
		CellRect rect = room.rects[0];
		foreach (IntVec3 edgeCell in rect.EdgeCells)
		{
			if (edgeCell.GetDoor(map) != null || edgeCell.GetEdifice(map) == null)
			{
				list.Add(edgeCell);
			}
		}
		for (int i = 0; i < list.Count; i++)
		{
			IntVec3 intVec = list[i];
			Rot4 opposite = rect.GetClosestEdge(intVec).Opposite;
			IntVec3 pos = intVec + opposite.FacingCell;
			IntVec3 intVec2 = IntVec3.Invalid;
			int num = 0;
			foreach (IntVec3 item in list)
			{
				if (intVec == item)
				{
					continue;
				}
				Rot4 closestEdge = rect.GetClosestEdge(item);
				if (opposite.IsHorizontal != closestEdge.IsHorizontal)
				{
					int value = (opposite.IsHorizontal ? (item.x - intVec.x) : (item.z - intVec.z));
					value = Mathf.Abs(value);
					if (value > num)
					{
						num = value;
						intVec2 = (opposite.IsHorizontal ? new IntVec3(item.x, 0, intVec.z) : new IntVec3(intVec.x, 0, item.z));
					}
				}
			}
			if (intVec2 == IntVec3.Invalid)
			{
				break;
			}
			WalkPathway(rect, pos, pathway, intVec2, opposite);
		}
		if (pathway.Empty())
		{
			BackupPathways(list, rect, pathway);
		}
	}

	private static void BackupPathways(List<IntVec3> doors, CellRect rect, List<IntVec3> pathway)
	{
		List<IntVec3> list = new List<IntVec3>();
		for (int i = 0; i < doors.Count; i++)
		{
			IntVec3 intVec = doors[i];
			Rot4 opposite = rect.GetClosestEdge(intVec).Opposite;
			IntVec3 pos = intVec + opposite.FacingCell;
			IntVec3 centerCell = rect.CenterCell;
			IntVec3 intVec2 = (opposite.IsHorizontal ? new IntVec3(centerCell.x, 0, pos.z) : new IntVec3(pos.x, 0, centerCell.z));
			if (!list.Contains(intVec2))
			{
				list.Add(intVec2);
			}
			WalkPathway(rect, pos, pathway, intVec2, opposite);
			for (int j = 0; j < list.Count - 1; j++)
			{
				IntVec3 intVec3 = list[j];
				IntVec3 intVec4 = list[j + 1];
				if (!(intVec3 == intVec4))
				{
					opposite = ((intVec3.x != intVec4.x) ? ((intVec3.x < intVec4.x) ? Rot4.East : Rot4.West) : ((intVec3.z < intVec4.z) ? Rot4.North : Rot4.South));
					pos = intVec3 + opposite.FacingCell;
					WalkPathway(rect, pos, pathway, intVec2, opposite);
				}
			}
		}
	}

	private void SpawnPathways(List<IntVec3> pathway, Map map, LayoutRoom room)
	{
		int num = 5;
		foreach (IntVec3 item in pathway)
		{
			SpawnRoof(map, item);
			IntVec3[] adjacentCells = GenAdj.AdjacentCells;
			foreach (IntVec3 intVec in adjacentCells)
			{
				IntVec3 intVec2 = item + intVec;
				SpawnRoof(map, intVec2);
				if (!pathway.Contains(intVec2) && intVec2.GetFirstBuilding(map) == null && !room.rects[0].EdgeCells.Contains(intVec2))
				{
					SpawnWall(map, intVec2);
				}
			}
			if (--num <= 0)
			{
				num = 5;
				RoomGenUtility.TrySpawnWallAttatchment(room.sketch.layoutSketch.WallLampThing, item, map, out var _);
			}
		}
	}

	private static void FillSpace(List<IntVec3> pathway, Map map, CellRect rect)
	{
		CellRect cellRect = rect.ContractedBy(1).ContractedBy(PlatformRange.RandomInRange, PlatformRange.RandomInRange, PlatformRange.RandomInRange, PlatformRange.RandomInRange);
		foreach (IntVec3 cell in rect.ContractedBy(1).Cells)
		{
			if (!pathway.Contains(cell) && cell.GetFirstBuilding(map) == null)
			{
				if (cellRect.Contains(cell))
				{
					TerrainDef newTerr = (map.Biome.inVacuum ? TerrainDefOf.Space : TerrainDefOf.PackedDirt);
					map.terrainGrid.SetTerrain(cell, newTerr);
				}
				map.roofGrid.SetRoof(cell, null);
			}
		}
	}

	private static void SpawnWall(Map map, IntVec3 cell)
	{
		GenSpawn.Spawn(ThingDefOf.AncientFortifiedWall, cell, map);
	}

	private static void WalkPathway(CellRect rect, IntVec3 pos, List<IntVec3> pathway, IntVec3 end, Rot4 dir)
	{
		while (rect.ContractedBy(1).Contains(pos))
		{
			pathway.Add(pos);
			if (!(pos == end))
			{
				pos += dir.FacingCell;
				continue;
			}
			break;
		}
	}
}
