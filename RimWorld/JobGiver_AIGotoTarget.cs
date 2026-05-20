using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_AIGotoTarget : ThinkNode_JobGiver
{
	private bool ignoreNonCombatants;

	private bool humanlikesOnly;

	private int overrideExpiryInterval = -1;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_AIGotoTarget obj = (JobGiver_AIGotoTarget)base.DeepCopy(resolve);
		obj.ignoreNonCombatants = ignoreNonCombatants;
		obj.humanlikesOnly = humanlikesOnly;
		obj.overrideExpiryInterval = overrideExpiryInterval;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Thing thing = pawn.mindState?.enemyTarget;
		if (thing == null)
		{
			return null;
		}
		if (thing.PositionHeld == pawn.PositionHeld || pawn.CanReachImmediate(thing, PathEndMode.Touch))
		{
			return null;
		}
		if (!IsTargetStillValid(thing, pawn))
		{
			pawn.mindState.enemyTarget = null;
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Goto, thing);
		if (overrideExpiryInterval > 0)
		{
			job.expiryInterval = overrideExpiryInterval;
		}
		else
		{
			job.intervalScalingTarget = TargetIndex.A;
		}
		job.checkOverrideOnExpire = true;
		job.collideWithPawns = true;
		return job;
	}

	private bool IsTargetStillValid(Thing target, Pawn pawn)
	{
		Pawn pawn2 = target as Pawn;
		if (target == null)
		{
			return false;
		}
		if (pawn2 != null && pawn2.ThreatDisabled(pawn))
		{
			return false;
		}
		if (!pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
		{
			return false;
		}
		return true;
	}
}
