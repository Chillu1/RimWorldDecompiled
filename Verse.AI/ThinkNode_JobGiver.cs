namespace Verse.AI;

public abstract class ThinkNode_JobGiver : ThinkNode
{
	[MustTranslate]
	protected string reportStringOverride;

	[MustTranslate]
	protected string crawlingReportStringOverride;

	public virtual string CrawlingReportStringOverride => crawlingReportStringOverride;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		ThinkNode_JobGiver obj = (ThinkNode_JobGiver)base.DeepCopy(resolve);
		obj.reportStringOverride = reportStringOverride;
		obj.crawlingReportStringOverride = crawlingReportStringOverride;
		return obj;
	}

	protected abstract Job TryGiveJob(Pawn pawn);

	public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
	{
		Job job = TryGiveJob(pawn);
		if (job == null)
		{
			return ThinkResult.NoJob;
		}
		if (reportStringOverride != null)
		{
			job.reportStringOverride = reportStringOverride;
		}
		if (CrawlingReportStringOverride != null)
		{
			job.crawlingReportStringOverride = CrawlingReportStringOverride;
		}
		return new ThinkResult(job, this);
	}
}
