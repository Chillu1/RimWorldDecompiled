using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Placeworker_AttachedToWall : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		List<Thing> thingList = loc.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			Thing thing2 = thingList[i];
			ThingDef thingDef = GenConstruct.BuiltDefOf(thing2.def) as ThingDef;
			if (thingDef?.building != null)
			{
				if (thingDef.Fillage == FillCategory.Full)
				{
					return false;
				}
				if (thingDef.building.isAttachment && thing2.Rotation == rot)
				{
					return "SomethingPlacedOnThisWall".Translate();
				}
			}
		}
		IntVec3 c = loc + GenAdj.CardinalDirections[rot.AsInt];
		if (!c.InBounds(map))
		{
			return false;
		}
		thingList = c.GetThingList(map);
		bool flag = false;
		for (int j = 0; j < thingList.Count; j++)
		{
			if (GenConstruct.BuiltDefOf(thingList[j].def) is ThingDef { building: not null } thingDef2)
			{
				if (!thingDef2.building.supportsWallAttachments)
				{
					flag = true;
				}
				else if (thingDef2.Fillage == FillCategory.Full)
				{
					return true;
				}
			}
		}
		if (flag)
		{
			return "CannotSupportAttachment".Translate();
		}
		return "MustPlaceOnWall".Translate();
	}
}
