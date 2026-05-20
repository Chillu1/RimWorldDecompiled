using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LearningGiver_NatureRunning : LearningGiver
	{
		public override bool CanDo(Pawn pawn)
		{
			if (!base.CanDo(pawn))
			{
				return false;
			}
			LocalTargetInfo interestTarget;
			return NatureRunningUtility.TryFindNatureInterestTarget(pawn, out interestTarget);
		}

		public override Job TryGiveJob(Pawn pawn)
		{
			if (!NatureRunningUtility.TryFindNatureInterestTarget(pawn, out var interestTarget))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(def.jobDef, interestTarget);
			job.locomotionUrgency = LocomotionUrgency.Sprint;
			return job;
		}
	}
}
