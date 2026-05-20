using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_StandAndBeSociallyActive : ThinkNode_JobGiver
	{
		public IntRange ticksRange = new IntRange(300, 600);

		public Direction8Way lookDirection;

		public bool maintainFacing;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_StandAndBeSociallyActive obj = (JobGiver_StandAndBeSociallyActive)base.DeepCopy(resolve);
			obj.ticksRange = ticksRange;
			obj.lookDirection = lookDirection;
			obj.maintainFacing = maintainFacing;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = JobMaker.MakeJob(JobDefOf.StandAndBeSociallyActive);
			job.expiryInterval = ticksRange.RandomInRange;
			job.lookDirection = lookDirection;
			job.forceMaintainFacing = maintainFacing;
			return job;
		}
	}
}
