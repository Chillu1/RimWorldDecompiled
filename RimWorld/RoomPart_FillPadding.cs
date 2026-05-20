using System;
using Verse;

namespace RimWorld;

public class RoomPart_FillPadding : RoomPartWorker
{
	public new RoomPart_ThingDef def => (RoomPart_ThingDef)base.def;

	public RoomPart_FillPadding(RoomPartDef def)
		: base(def)
	{
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float threatPoints)
	{
		ThingDef thingDef = def.thingDef;
		Map map2 = map;
		Func<IntVec3, Rot4, CellRect, bool> validator = Validator;
		ThingDef stuffDef = def.stuffDef;
		RoomGenUtility.FillWithPadding(thingDef, 1, room, map2, null, validator, null, 1, stuffDef);
		bool Validator(IntVec3 pos, Rot4 rot, CellRect rect)
		{
			return RoomGenUtility.IsClearAndNotAdjacentToDoor(def.thingDef, pos, map, rot);
		}
	}
}
