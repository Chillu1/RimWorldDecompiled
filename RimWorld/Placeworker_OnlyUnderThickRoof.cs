using Verse;

namespace RimWorld
{
	public class Placeworker_OnlyUnderThickRoof : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			RoofDef roofDef = map.roofGrid.RoofAt(loc);
			if (roofDef == null || !roofDef.isThickRoof)
			{
				return new AcceptanceReport("MustPlaceUnderThickRoof".Translate());
			}
			return true;
		}
	}
}
