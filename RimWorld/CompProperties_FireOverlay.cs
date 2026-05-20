using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_FireOverlay : CompProperties
{
	public float fireSize = 1f;

	public float finalFireSize = 1f;

	public float fireGrowthDurationTicks = -1f;

	public Vector3 offset;

	public Vector3? offsetNorth;

	public Vector3? offsetSouth;

	public Vector3? offsetWest;

	public Vector3? offsetEast;

	public CompProperties_FireOverlay()
	{
		compClass = typeof(CompFireOverlay);
	}

	public Vector3 DrawOffsetForRot(Rot4 rot)
	{
		return rot.AsInt switch
		{
			0 => offsetNorth ?? offset, 
			1 => offsetEast ?? offset, 
			2 => offsetSouth ?? offset, 
			3 => offsetWest ?? offset, 
			_ => offset, 
		};
	}

	public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
	{
		Graphic graphic = GhostUtility.GhostGraphicFor(CompFireOverlay.FireGraphic, thingDef, ghostCol);
		Vector3 loc = center.ToVector3ShiftedWithAltitude(drawAltitude) + thingDef.graphicData.DrawOffsetForRot(rot) + DrawOffsetForRot(rot);
		graphic.DrawFromDef(loc, rot, thingDef);
	}
}
