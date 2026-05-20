using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class JobGiver_AIFollowPawn : ThinkNode_JobGiver
{
	protected virtual int FollowJobExpireInterval => 140;

	protected abstract Pawn GetFollowee(Pawn pawn);

	protected abstract float GetRadius(Pawn pawn);

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn followee = GetFollowee(pawn);
		if (followee == null)
		{
			Log.Error(GetType()?.ToString() + " has null followee. pawn=" + pawn.ToStringSafe());
			return null;
		}
		if (!followee.Spawned || !pawn.CanReach(followee, PathEndMode.OnCell, Danger.Deadly))
		{
			return null;
		}
		float radius = GetRadius(pawn);
		if (!JobDriver_FollowClose.FarEnoughAndPossibleToStartJob(pawn, followee, radius))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.FollowClose, followee);
		job.expiryInterval = FollowJobExpireInterval;
		job.checkOverrideOnExpire = true;
		job.followRadius = radius;
		return job;
	}
}
