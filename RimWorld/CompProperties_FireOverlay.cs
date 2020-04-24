using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_FireOverlay : CompProperties
	{
		public float fireSize = 1f;

		public Vector3 offset;

		public CompProperties_FireOverlay()
		{
			compClass = typeof(CompFireOverlay);
		}

		public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
		{
			GhostUtility.GhostGraphicFor(CompFireOverlay.FireGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef);
		}
	}
}
