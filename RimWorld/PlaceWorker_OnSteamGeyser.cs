using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PlaceWorker_OnSteamGeyser : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		Thing thing2 = map.thingGrid.ThingAt(loc, ThingDefOf.SteamGeyser);
		if (thing2 == null || thing2.Position != loc)
		{
			return "MustPlaceOnSteamGeyser".Translate();
		}
		return true;
	}

	public override bool ForceAllowPlaceOver(BuildableDef otherDef)
	{
		return otherDef == ThingDefOf.SteamGeyser;
	}

	public override void DrawMouseAttachments(BuildableDef def)
	{
		List<Thing> list = Find.CurrentMap.listerThings.ThingsOfDef(ThingDefOf.SteamGeyser);
		for (int i = 0; i < list.Count; i++)
		{
			TargetHighlighter.Highlight(list[i]);
		}
	}
}
