using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ForceSleepNow : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position);
			job.forceSleep = true;
			return job;
		}
	}
}
