using Verse;

namespace RimWorld;

public class RoomPart_Crate : RoomPartWorker
{
	public new RoomPart_CrateDef def => (RoomPart_CrateDef)base.def;

	public RoomPart_Crate(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		string signal = null;
		if (def.triggerThreatSignal)
		{
			signal = room.ThreatSignal;
		}
		RoomGenUtility.SpawnCratesInRoom(def.crateDef, room, map, def.thingSetMaker, IntRange.One, addRewards: true, signal, def.rotations.Random());
	}
}
