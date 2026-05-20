using RimWorld;

namespace Verse.AI;

public class JobGiver_IdleForever : ThinkNode_JobGiver
{
	private const int checkCrawlInterval = 2500;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Wait_Downed);
		if (pawn.Deathresting)
		{
			job.forceSleep = true;
		}
		else
		{
			job.expiryInterval = 2500;
		}
		return job;
	}
}
