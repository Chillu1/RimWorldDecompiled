using Verse;

namespace RimWorld
{
	public class PlaceWorker_HeadOnShipBeam : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			IntVec3 c = loc + rot.FacingCell * -1;
			if (!c.InBounds(map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice == null || edifice.def != ThingDefOf.Ship_Beam)
			{
				return "MustPlaceHeadOnShipBeam".Translate();
			}
			return true;
		}
	}
}
