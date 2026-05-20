using Verse;

namespace RimWorld;

public class PlaceWorker_NotUnderRoof : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (checkingDef.Size.x == 1 && checkingDef.Size.z == 1)
		{
			if (map.roofGrid.Roofed(loc))
			{
				return new AcceptanceReport("MustPlaceUnroofed".Translate());
			}
		}
		else
		{
			foreach (IntVec3 item in GenAdj.OccupiedRect(loc, rot, checkingDef.Size))
			{
				if (map.roofGrid.Roofed(item))
				{
					return new AcceptanceReport("MustPlaceUnroofed".Translate());
				}
			}
		}
		return true;
	}
}
