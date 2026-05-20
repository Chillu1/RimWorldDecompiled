using Verse;

namespace RimWorld;

public class RoomContents_StockpileHermeticCrateRoom : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		SpawnCrate(map, room);
		base.FillRoom(map, room, faction, threatPoints);
	}

	private static void SpawnCrate(Map map, LayoutRoom room)
	{
		if (!room.TryGetRandomCellInRoom(map, out var cell, 3))
		{
			cell = room.rects[0].CenterCell;
		}
		RoomGenUtility.SpawnHermeticCrate(cell, map, Rot4.Random, ThingSetMakerDefOf.MapGen_HighValueCrate);
	}
}
