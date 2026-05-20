using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomContents_Corridor : RoomContentsWorker
{
	protected virtual TerrainDef StripCorridorTerrain => null;

	protected virtual IntRange ExteriorDoorCount => new IntRange(1, 3);

	protected virtual ThingDef DoorThing => null;

	protected virtual ThingDef DoorStuff => null;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnStrip(map, room);
		SpawnDoors(map, room, ExteriorDoorCount);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnStrip(Map map, LayoutRoom room)
	{
		if (StripCorridorTerrain == null)
		{
			return;
		}
		foreach (CellRect rect in room.rects)
		{
			Rot4 rot = ((rect.Width > rect.Height) ? Rot4.East : Rot4.North);
			for (IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot.Opposite); rect.Contains(centerCellOnEdge); centerCellOnEdge += rot.FacingCell)
			{
				TrySpawnAdjacentCorridorTiles(map, room, rect, rot, centerCellOnEdge);
				map.terrainGrid.SetTerrain(centerCellOnEdge, StripCorridorTerrain);
			}
		}
	}

	private void SpawnDoors(Map map, LayoutRoom room, IntRange countRange)
	{
		int count = countRange.RandomInRange;
		if (count <= 0)
		{
			return;
		}
		List<IntVec3> list = new List<IntVec3>();
		foreach (CellRect rect in room.rects)
		{
			Rot4 rot = ((rect.Width > rect.Height) ? Rot4.East : Rot4.North);
			IntVec3 centerCellOnEdge = rect.GetCenterCellOnEdge(rot);
			IntVec3 centerCellOnEdge2 = rect.GetCenterCellOnEdge(rot.Opposite);
			ProcessPossibleDoorCell(centerCellOnEdge, room, map, list, ref count);
			ProcessPossibleDoorCell(centerCellOnEdge2, room, map, list, ref count);
		}
		if (list.Empty())
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			if (!list.Any())
			{
				break;
			}
			IntVec3 intVec = list.RandomElement();
			list.Remove(intVec);
			ThingDef obj = DoorThing ?? room.sketch.layoutSketch.door ?? ThingDefOf.Door;
			ThingDef stuff = ((!obj.MadeFromStuff) ? null : (DoorStuff ?? room.sketch.layoutSketch.doorStuff ?? ThingDefOf.Steel));
			GenSpawn.Spawn(ThingMaker.MakeThing(obj, stuff), intVec, map);
		}
	}

	private static void ProcessPossibleDoorCell(IntVec3 cell, LayoutRoom room, Map map, List<IntVec3> possible, ref int count)
	{
		if (cell.GetDoor(map) != null)
		{
			count--;
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = cell + GenAdj.CardinalDirections[i];
			if (!room.sketch.AnyRoomContains(c))
			{
				possible.Add(cell);
				break;
			}
		}
	}

	private static void TrySpawnAdjacentCorridorTiles(Map map, LayoutRoom room, CellRect rect, Rot4 rot, IntVec3 pos)
	{
		if (room.rects.Count <= 1)
		{
			return;
		}
		Rot4 rot2 = ((rot == Rot4.North) ? Rot4.East : Rot4.North);
		foreach (CellRect rect2 in room.rects)
		{
			if (!(rect2 == rect) && !(((rect2.Width > rect2.Height) ? Rot4.East : Rot4.North) == rot) && (!rot.IsHorizontal || pos.x == rect2.CenterCell.x) && (!rot.IsVertical || pos.z == rect2.CenterCell.z))
			{
				IntVec3 c = (((!rot.IsVertical || rect2.CenterCell.x <= pos.x) && (!rot.IsHorizontal || rect2.CenterCell.z <= pos.z)) ? (pos - rot2.FacingCell) : (pos + rot2.FacingCell));
				map.terrainGrid.SetTerrain(c, TerrainDefOf.MetalTile);
			}
		}
	}
}
