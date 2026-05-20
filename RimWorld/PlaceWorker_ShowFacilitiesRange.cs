using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_ShowFacilitiesRange : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		CompProperties_Facility compProperties = def.GetCompProperties<CompProperties_Facility>();
		if (compProperties != null)
		{
			GenDraw.DrawRadiusRing(center, compProperties.maxDistance - 0.1f);
		}
	}
}
