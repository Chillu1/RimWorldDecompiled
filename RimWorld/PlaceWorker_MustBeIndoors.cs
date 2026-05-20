using Verse;

namespace RimWorld
{
	public class PlaceWorker_MustBeIndoors : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			Room room = loc.GetRoom(map);
			if (room == null || room.TouchesMapEdge)
			{
				return "MustBePlacedIndoors".Translate();
			}
			return AcceptanceReport.WasAccepted;
		}
	}
}
