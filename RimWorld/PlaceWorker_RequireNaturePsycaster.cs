using Verse;

namespace RimWorld;

public class PlaceWorker_RequireNaturePsycaster : PlaceWorker
{
	public override bool IsBuildDesignatorVisible(BuildableDef def)
	{
		foreach (Pawn allMapsCaravansAndTravellingTransporters_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravellingTransporters_Alive_Colonists)
		{
			if (MeditationFocusDefOf.Natural.CanPawnUse(allMapsCaravansAndTravellingTransporters_Alive_Colonist))
			{
				return true;
			}
		}
		return false;
	}
}
