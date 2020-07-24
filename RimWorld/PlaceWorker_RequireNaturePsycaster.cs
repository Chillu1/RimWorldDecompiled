using Verse;

namespace RimWorld
{
	public class PlaceWorker_RequireNaturePsycaster : PlaceWorker
	{
		public override bool IsBuildDesignatorVisible(BuildableDef def)
		{
			foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_Colonist in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
			{
				if (MeditationFocusDefOf.Natural.CanPawnUse(allMapsCaravansAndTravelingTransportPods_Alive_Colonist))
				{
					return true;
				}
			}
			return false;
		}
	}
}
