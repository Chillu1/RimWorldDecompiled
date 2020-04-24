using RimWorld;

namespace Verse.AI
{
	public class JobGiver_Idle : ThinkNode_JobGiver
	{
		public int ticks = 50;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_Idle obj = (JobGiver_Idle)base.DeepCopy(resolve);
			obj.ticks = ticks;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Wait);
			job.expiryInterval = ticks;
			return job;
		}
	}
}
