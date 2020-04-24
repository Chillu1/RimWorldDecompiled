namespace Verse.AI
{
	public abstract class ThinkNode_JobGiver : ThinkNode
	{
		protected abstract Job TryGiveJob(Pawn pawn);

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			Job job = TryGiveJob(pawn);
			if (job == null)
			{
				return ThinkResult.NoJob;
			}
			return new ThinkResult(job, this);
		}
	}
}
