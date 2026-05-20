using Verse;

namespace RimWorld;

public class RoomPart_CornerThing : RoomPartWorker
{
	public new RoomPart_ThingDef def => (RoomPart_ThingDef)base.def;

	public RoomPart_CornerThing(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		RoomGenUtility.TryPlaceInRandomCorner(map, room, def.thingDef, faction ?? Faction.OfAncientsHostile, def.stuffDef);
	}
}
