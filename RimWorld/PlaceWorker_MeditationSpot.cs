using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_MeditationSpot : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		MeditationUtility.DrawMeditationSpotOverlay(center, Find.CurrentMap);
	}
}
