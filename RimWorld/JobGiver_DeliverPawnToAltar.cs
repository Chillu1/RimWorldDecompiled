using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DeliverPawnToAltar : JobGiver_GotoTravelDestination
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!ModLister.CheckIdeology("Deliver to altar"))
			{
				return null;
			}
			Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
			if (!pawn.CanReach(pawn2.Position, PathEndMode.OnCell, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.DeliverToAltar, pawn2, pawn.mindState.duty.focus, pawn.mindState.duty.focusThird);
			job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
			job.expiryInterval = jobMaxDuration;
			job.count = 1;
			return job;
		}
	}
}
