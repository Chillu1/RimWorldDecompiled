using RimWorld;

namespace Verse.AI;

public class JobGiver_IdleError : ThinkNode_JobGiver
{
	private const int WaitTime = 100;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Log.ErrorOnce(pawn?.ToString() + " issued IdleError wait job. The behavior tree should never get here.", 532983);
		Job job = JobMaker.MakeJob(JobDefOf.Wait);
		job.expiryInterval = 100;
		return job;
	}
}
