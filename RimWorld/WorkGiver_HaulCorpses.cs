using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_HaulCorpses : WorkGiver_Haul
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!(t is Corpse))
			{
				return null;
			}
			return base.JobOnThing(pawn, t, forced);
		}

		public override string PostProcessedGerund(Job job)
		{
			if (job.GetTarget(TargetIndex.B).Thing is Building_Grave)
			{
				return "Burying".Translate();
			}
			return base.PostProcessedGerund(job);
		}
	}
}
