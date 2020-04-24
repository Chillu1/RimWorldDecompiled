using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_HaulGeneral : WorkGiver_Haul
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is Corpse)
			{
				return null;
			}
			return base.JobOnThing(pawn, t, forced);
		}
	}
}
