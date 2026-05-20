using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_InduceSlaveToRebel : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = SlaveRebellionUtility.FindSlaveForRebellion(pawn);
			if (pawn2 == null || !pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.InduceSlaveToRebel, pawn2);
			job.interaction = InteractionDefOf.SparkSlaveRebellion;
			return job;
		}
	}
}
