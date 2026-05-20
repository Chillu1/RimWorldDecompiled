using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_ShelfedCorridor : RoomContents_Corridor
{
	private static readonly FloatRange ShelvesPer10EdgeCells = new FloatRange(0.35f, 0.35f);

	private static readonly IntRange ShelfGroupSizeRange = new IntRange(1, 2);

	protected override ThingDef DoorThing => ThingDefOf.AncientBlastDoor;

	protected override IntRange ExteriorDoorCount => new IntRange(2, 3);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		SpawnShelves(map, room);
	}

	private static void SpawnShelves(Map map, LayoutRoom room)
	{
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(ShelvesPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.Shelf, count, ShelfGroupSizeRange, room, map, null, null, 1, 0, ThingDefOf.Steel);
	}
}
