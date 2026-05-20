using Verse;

namespace RimWorld;

public class RoomPart_SentryDrone : RoomPartWorker
{
	public RoomPart_SentryDrone(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		if (ModsConfig.OdysseyActive)
		{
			if (!room.TryGetRandomCellInRoom(map, out var cell, 0, 0, Validator))
			{
				Log.Error("Failed to find cell to spawn sentry drone.");
			}
			else
			{
				GenSpawn.Spawn(PawnGenerator.GeneratePawn(PawnKindDefOf.Drone_Sentry, faction), cell, map);
			}
		}
		bool Validator(IntVec3 c)
		{
			if (c.Standable(map))
			{
				return c.GetThingList(map).Count == 0;
			}
			return false;
		}
	}
}
