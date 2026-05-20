using Verse;

namespace RimWorld;

public class RoomContents_Ramblings : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		if (room.TryGetRandomCellInRoom(map, out var cell, 1))
		{
			GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.FloorEtchingRambling), cell, map);
		}
	}
}
