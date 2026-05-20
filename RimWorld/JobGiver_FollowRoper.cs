using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_FollowRoper : ThinkNode_JobGiver
	{
		private const int FollowJobExpireInterval = 140;

		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = pawn?.roping?.RopedByPawn;
			if (pawn2 == null)
			{
				return null;
			}
			if (!(pawn2.jobs.curDriver is JobDriver_RopeToDestination))
			{
				return null;
			}
			if (!pawn2.CurJob.GetTarget(TargetIndex.B).Cell.IsValid)
			{
				return null;
			}
			if (!pawn2.Spawned || !pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.FollowRoper, pawn2);
			job.expiryInterval = 140;
			job.checkOverrideOnExpire = true;
			return job;
		}
	}
}
