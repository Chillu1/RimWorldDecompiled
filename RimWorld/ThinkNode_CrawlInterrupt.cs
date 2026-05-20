using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_CrawlInterrupt : ThinkNode_Priority
{
	private IntRange crawlDurationTicksRange = new IntRange(720, 1440);

	private IntRange ticksBetweenCrawlsRange = new IntRange(240, 480);

	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		pawn.mindState.nextMoveOrderIsWait = false;
		ThinkResult thinkResult = base.TryIssueJobPackage(pawn, jobParams);
		if (thinkResult == ThinkResult.NoJob)
		{
			return ThinkResult.NoJob;
		}
		if (pawn.mindState.nextMoveOrderIsCrawlBreak)
		{
			pawn.mindState.nextMoveOrderIsCrawlBreak = false;
			Job job = JobMaker.MakeJob(JobDefOf.Wait_Downed);
			job.expiryInterval = ticksBetweenCrawlsRange.RandomInRange;
			return new ThinkResult(job, this);
		}
		pawn.mindState.nextMoveOrderIsCrawlBreak = true;
		thinkResult.Job.expiryInterval = crawlDurationTicksRange.RandomInRange;
		return thinkResult;
	}
}
