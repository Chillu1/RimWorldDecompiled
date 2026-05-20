using Verse;

namespace RimWorld;

public class RoomPart_Barricades : RoomPartWorker
{
	public new RoomPart_BarricadeDef def => (RoomPart_BarricadeDef)base.def;

	public RoomPart_Barricades(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		RoomGenUtility.SpawnDoorBarricades(def.wallDef, room, map, def.chancePerDoor, def.stuffDef, def.steps, def.offset);
	}
}
