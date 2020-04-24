using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_Haul : WorkGiver_Scanner
	{
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling();
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling().Count == 0;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced))
			{
				return null;
			}
			return HaulAIUtility.HaulToStorageJob(pawn, t);
		}
	}
}
