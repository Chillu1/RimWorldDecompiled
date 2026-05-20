using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_SubstructureFootprint : CompProperties
{
	public float radius;

	public bool displaySubstructureOverlayWhenSelected = true;

	public CompProperties_SubstructureFootprint()
	{
		compClass = typeof(CompSubstructureFootprint);
	}

	public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
	{
		SubstructureGrid.DrawSubstructureFootprintWithExtra(this, center, thing);
	}
}
