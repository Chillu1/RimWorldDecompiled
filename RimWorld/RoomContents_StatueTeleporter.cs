using Verse;

namespace RimWorld;

public class RoomContents_StatueTeleporter : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		if (!room.TryGetRandomCellInRoom(ThingDefOf.GrayStatueTeleporter, map, out var cell, null, 4))
		{
			cell = room.rects[0].CenterCell;
		}
		GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.GrayStatueTeleporter), cell, map);
	}
}
