using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class RoomContents_EndlessPit : RoomContentsWorker
{
	private IEnumerable<ThingDef> PitDefs
	{
		get
		{
			yield return ThingDefOf.EndlessPit2x2c;
			yield return ThingDefOf.EndlessPit3x2c;
			yield return ThingDefOf.EndlessPit3x3c;
		}
	}

	public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
	{
		base.FillRoom(map, room, faction, threatPoints);
		ThingDef thingDef = PitDefs.RandomElement();
		if (room.TryGetRandomCellInRoom(thingDef, map, out var cell, null, 3, 1))
		{
			GenSpawn.Spawn(ThingMaker.MakeThing(thingDef), cell, map, thingDef.rotatable ? Rot4.Random : Rot4.North);
		}
	}
}
