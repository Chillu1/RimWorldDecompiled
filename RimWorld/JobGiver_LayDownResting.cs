using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_LayDownResting : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, (IntVec3 c) => pawn.CanReserveAndReach(c, PathEndMode.Touch, Danger.None), out var result))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.LayDownResting, result);
			job.locomotionUrgency = LocomotionUrgency.Amble;
			return job;
		}
	}
}
