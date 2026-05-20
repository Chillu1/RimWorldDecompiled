using RimWorld;
using UnityEngine;

namespace Verse;

public static class GhostDrawer
{
	public static void DrawGhostThing(IntVec3 center, Rot4 rot, ThingDef thingDef, Graphic baseGraphic, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null, bool drawPlaceWorkers = true, ThingDef stuff = null)
	{
		if (baseGraphic == null)
		{
			baseGraphic = thingDef.graphic;
		}
		Graphic graphic = GhostUtility.GhostGraphicFor(baseGraphic, thingDef, ghostCol, stuff);
		Vector3 loc = GenThing.TrueCenter(center, rot, thingDef.Size, drawAltitude.AltitudeFor());
		graphic.DrawFromDef(loc, rot, thingDef);
		for (int i = 0; i < thingDef.comps.Count; i++)
		{
			thingDef.comps[i].DrawGhost(center, rot, thingDef, ghostCol, drawAltitude, thing);
		}
		if (drawPlaceWorkers && thingDef.PlaceWorkers != null)
		{
			for (int j = 0; j < thingDef.PlaceWorkers.Count; j++)
			{
				thingDef.PlaceWorkers[j].DrawGhost(thingDef, center, rot, ghostCol, thing);
			}
		}
	}
}
