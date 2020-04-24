using RimWorld;

namespace Verse.AI
{
	public class JobGiver_ForcedGoto : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			IntVec3 forcedGotoPosition = pawn.mindState.forcedGotoPosition;
			if (!forcedGotoPosition.IsValid)
			{
				return null;
			}
			if (!pawn.CanReach(forcedGotoPosition, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				pawn.mindState.forcedGotoPosition = IntVec3.Invalid;
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Goto, forcedGotoPosition);
			job.locomotionUrgency = LocomotionUrgency.Walk;
			return job;
		}
	}
}
