using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_ArtificialBuildingsNear : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		FocusStrengthOffset_ArtificialBuildings focusStrengthOffset_ArtificialBuildings = ((CompProperties_MeditationFocus)def.CompDefFor<CompMeditationFocus>()).offsets.OfType<FocusStrengthOffset_ArtificialBuildings>().FirstOrDefault();
		if (focusStrengthOffset_ArtificialBuildings != null)
		{
			MeditationUtility.DrawArtificialBuildingOverlay(center, def, Find.CurrentMap, focusStrengthOffset_ArtificialBuildings.radius);
		}
	}
}
