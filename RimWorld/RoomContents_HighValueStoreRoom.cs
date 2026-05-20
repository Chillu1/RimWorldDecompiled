using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class RoomContents_HighValueStoreRoom : RoomContentsWorker
{
	private static readonly FloatRange ShelvesPer10EdgeCells = new FloatRange(1f, 3f);

	private static readonly IntRange ShelfGroupSizeRange = new IntRange(2, 3);

	private static readonly IntRange GravlitePanelCount = new IntRange(150, 200);

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnShelves(map, room);
		RoomGenUtility.SpawnHermeticCrateInRoom(room, map, ThingSetMakerDefOf.MapGen_HighValueCrate);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private static void SpawnShelves(Map map, LayoutRoom room)
	{
		List<IntVec3> possibleSpots = new List<IntVec3>(6);
		float num = (float)room.rects.Sum((CellRect r) => r.ContractedBy(1).EdgeCellsCount) / 10f;
		int count = Mathf.Max(Mathf.RoundToInt(ShelvesPer10EdgeCells.RandomInRange * num), 1);
		RoomGenUtility.FillAroundEdges(ThingDefOf.Shelf, count, ShelfGroupSizeRange, room, map, null, null, 1, 0, null, avoidDoors: true, RotationDirection.Opposite, SpawnAction);
		int num2 = GravlitePanelCount.RandomInRange;
		while (num2 > 0 && possibleSpots.Any())
		{
			IntVec3 intVec = possibleSpots.RandomElement();
			possibleSpots.Remove(intVec);
			Thing thing = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
			thing.stackCount = Mathf.Min(num2, thing.def.stackLimit);
			num2 -= thing.stackCount;
			GenPlace.TryPlaceThing(thing, intVec, map, ThingPlaceMode.Near);
		}
		Thing SpawnAction(IntVec3 pos, Rot4 rot, Map _)
		{
			Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Shelf, ThingDefOf.Steel);
			GenSpawn.Spawn(thing2, pos, map, rot);
			possibleSpots.AddRange(thing2.OccupiedRect().Cells);
			return thing2;
		}
	}
}
