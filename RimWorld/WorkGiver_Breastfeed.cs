using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Breastfeed : WorkGiver_Scanner
	{
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.mapPawns.SpawnedHungryPawns;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
		{
			if (forced)
			{
				return null;
			}
			Pawn pawn2 = t as Pawn;
			if (!ChildcareUtility.CanMomAutoBreastfeedBabyNow(pawn, pawn2, forced, out var _))
			{
				return null;
			}
			if (pawn2.mindState.AutofeedSetting(pawn) != AutofeedMode.Childcare)
			{
				return null;
			}
			return ChildcareUtility.MakeBreastfeedJob(pawn2);
		}
	}
}
