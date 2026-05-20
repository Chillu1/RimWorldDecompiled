using Verse;

namespace RimWorld;

public class RoomContents_ComputerRoom : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		if (room.TryGetRandomCellInRoom(ThingDefOf.AncientMachine, map, out var cell, null, 0, 1))
		{
			GenSpawn.Spawn(ThingDefOf.AncientMachine, cell, map);
		}
		base.FillRoom(map, room, faction, threatPoints);
	}
}
