using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompProperties_DarklightOverlay : CompProperties_FireOverlay
	{
		public CompProperties_DarklightOverlay()
		{
			compClass = typeof(CompDarklightOverlay);
		}

		public override void DrawGhost(IntVec3 center, Rot4 rot, ThingDef thingDef, Color ghostCol, AltitudeLayer drawAltitude, Thing thing = null)
		{
			GhostUtility.GhostGraphicFor(CompDarklightOverlay.DarklightGraphic, thingDef, ghostCol).DrawFromDef(center.ToVector3ShiftedWithAltitude(drawAltitude), rot, thingDef);
		}
	}
}
