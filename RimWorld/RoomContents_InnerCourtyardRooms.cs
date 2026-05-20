using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_InnerCourtyardRooms : RoomContentsWorker
{
	private const int NormalCrates = 1;

	private const int CratesPer10Cells = 1;

	private const int InnerWallContract = 3;

	private int placedCrates;

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		placedCrates = 0;
		CellRect rect = room.rects[0];
		foreach (IntVec3 edgeCell in rect.ContractedBy(2).EdgeCells)
		{
			if (!ContainedWithinOtherRect(rect, edgeCell, room, 1))
			{
				SpawnWall(edgeCell, map);
			}
		}
		SpawnInteriorRooms(map, rect, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private void SpawnInteriorRooms(Map map, CellRect rect, LayoutRoom room)
	{
		CellRect cellRect = rect.ContractedBy(3);
		Queue<CellRect> queue = RoomGenUtility.SubdividedIntoRooms_NewTemp(map, cellRect, 4, 1, 2, ThingDefOf.AncientFortifiedWall);
		CellRect result;
		while (queue.TryDequeue(out result))
		{
			DoSubroomRoomInterior(map, result, cellRect, room);
		}
	}

	private void DoSubroomRoomInterior(Map map, CellRect rect, CellRect container, LayoutRoom room)
	{
		foreach (IntVec3 cell in rect.Cells)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.AncientTile);
		}
		float num = (float)rect.Area / 10f;
		int num2 = 0;
		int num3 = Mathf.Max(Mathf.RoundToInt(1f * num), 1);
		foreach (IntVec3 item2 in rect.ContractedBy(1).Cells.InRandomOrder())
		{
			if (IsValidCrateCell(item2, map))
			{
				SpawnBox(item2, map);
				if (++num2 >= num3)
				{
					break;
				}
			}
		}
		CellRect item = rect.ExpandedBy(1);
		ThingDef def = (ModsConfig.OdysseyActive ? ThingDefOf.AncientBlastDoor : ThingDefOf.Door);
		ThingDef stuff = (ModsConfig.OdysseyActive ? null : ThingDefOf.Steel);
		foreach (IntVec3 item3 in item.EdgeCells.InRandomOrder())
		{
			if (RoomGenUtility.IsGoodForDoor(item3, map) && !container.ContractedBy(1).Contains(item3))
			{
				GenSpawn.Spawn(ThingMaker.MakeThing(def, stuff), item3, map);
				break;
			}
		}
		RoomGenUtility.SpawnWallAttatchments(room.sketch.layoutSketch.WallLampThing, map, new List<CellRect>(1) { item }, IntRange.Between(1, 3));
	}

	private void SpawnBox(IntVec3 cell, Map map)
	{
		RoomGenUtility.SpawnHermeticCrate(cell, map, ThingSetMakerDefOf.MapGen_HighValueCrate, placedCrates++ < 1);
	}

	private static bool IsValidCrateCell(IntVec3 pos, Map map)
	{
		foreach (IntVec3 cell in pos.RectAbout(ThingDefOf.AncientHermeticCrate.Size, Rot4.South).Cells)
		{
			if (cell.GetEdifice(map) != null)
			{
				return false;
			}
			foreach (IntVec3 item in GenAdjFast.AdjacentCells8Way(cell))
			{
				if (item.GetEdifice(map) != null)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void SpawnWall(IntVec3 cell, Map map)
	{
		GenSpawn.Spawn(ThingDefOf.AncientFortifiedWall, cell, map);
	}

	private static bool ContainedWithinOtherRect(CellRect rect, IntVec3 cell, LayoutRoom room, int contractedBy)
	{
		if (!rect.EdgeCells.Contains(cell))
		{
			return false;
		}
		foreach (CellRect rect2 in room.rects)
		{
			if (!(rect == rect2) && rect2.ContractedBy(contractedBy).Contains(cell))
			{
				return false;
			}
		}
		return true;
	}
}
