using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_DeliverPawnToCell : JobGiver_GotoTravelDestination
	{
		public bool addArrivalTagIfTargetIsDead;

		public bool addArrivalTagIfInvalidTarget;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_DeliverPawnToCell obj = (JobGiver_DeliverPawnToCell)base.DeepCopy(resolve);
			obj.addArrivalTagIfTargetIsDead = addArrivalTagIfTargetIsDead;
			obj.addArrivalTagIfInvalidTarget = addArrivalTagIfInvalidTarget;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
			if ((addArrivalTagIfTargetIsDead && pawn2.Dead) || pawn2.Position == pawn.mindState.duty.focus.Cell || (addArrivalTagIfInvalidTarget && !pawn.mindState.duty.focus.IsValid))
			{
				RitualUtility.AddArrivalTag(pawn);
				RitualUtility.AddArrivalTag(pawn2);
				return null;
			}
			if (!pawn.CanReach(pawn2, PathEndMode.Touch, PawnUtility.ResolveMaxDanger(pawn, maxDanger)))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.DeliverToCell, pawn2, pawn.mindState.duty.focus, pawn.mindState.duty.focusThird);
			job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, locomotionUrgency);
			job.expiryInterval = jobMaxDuration;
			job.count = 1;
			job.ritualTag = "Arrived";
			return job;
		}
	}
}
