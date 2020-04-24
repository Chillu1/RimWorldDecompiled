namespace Verse.AI
{
	public class ThinkNode_QueuedJob : ThinkNode
	{
		public bool inBedOnly;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_QueuedJob obj = (ThinkNode_QueuedJob)base.DeepCopy(resolve);
			obj.inBedOnly = inBedOnly;
			return obj;
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			JobQueue jobQueue = pawn.jobs.jobQueue;
			if (pawn.Downed || jobQueue.AnyCanBeginNow(pawn, inBedOnly))
			{
				while (jobQueue.Count > 0 && !jobQueue.Peek().job.CanBeginNow(pawn, inBedOnly))
				{
					QueuedJob queuedJob = jobQueue.Dequeue();
					pawn.ClearReservationsForJob(queuedJob.job);
					if (pawn.jobs.debugLog)
					{
						pawn.jobs.DebugLogEvent("   Throwing away queued job that I cannot begin now: " + queuedJob.job);
					}
				}
			}
			if (jobQueue.Count > 0 && jobQueue.Peek().job.CanBeginNow(pawn, inBedOnly))
			{
				QueuedJob queuedJob2 = jobQueue.Dequeue();
				if (pawn.jobs.debugLog)
				{
					pawn.jobs.DebugLogEvent("   Returning queued job: " + queuedJob2.job);
				}
				return new ThinkResult(queuedJob2.job, this, queuedJob2.tag, fromQueue: true);
			}
			return ThinkResult.NoJob;
		}
	}
}
