using Verse;

namespace RimWorld;

public class RoomContents_SleepingFleshbeast : RoomContentsWorker
{
	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		if (room.TryGetRandomCellInRoom(map, out var cell, 1))
		{
			if (GenSpawn.Spawn(PawnGenerator.GeneratePawn(PawnKindDefOf.Fingerspike, Faction.OfEntities), cell, map).TryGetComp(out CompCanBeDormant comp))
			{
				comp.ToSleep();
			}
			for (int i = 0; i < 5; i++)
			{
				FilthMaker.TryMakeFilth(room.Cells.RandomElement(), map, ThingDefOf.Filth_Blood);
			}
		}
	}
}
