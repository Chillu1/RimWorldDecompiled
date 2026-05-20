using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Berserk : ThinkNode_JobGiver
{
	private const float WaitChance = 0.5f;

	private const int WaitTicks = 90;

	private const int MinMeleeChaseTicks = 420;

	private const int MaxMeleeChaseTicks = 900;

	private float maxAttackDistance = 40f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (Rand.Value < 0.5f)
		{
			Job job = JobMaker.MakeJob(JobDefOf.Wait_Combat);
			job.expiryInterval = 90;
			job.canUseRangedWeapon = false;
			return job;
		}
		if (pawn.TryGetAttackVerb(null) == null)
		{
			return null;
		}
		Thing thing = FindAttackTarget(pawn);
		if (thing != null)
		{
			Job job2 = JobMaker.MakeJob(JobDefOf.AttackMelee, thing);
			job2.maxNumMeleeAttacks = 1;
			job2.expiryInterval = Rand.Range(420, 900);
			job2.canBashDoors = true;
			return job2;
		}
		return null;
	}

	private Thing FindAttackTarget(Pawn pawn)
	{
		return (Thing)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedReachable, IsGoodTarget, 0f, maxAttackDistance, default(IntVec3), float.MaxValue, canBashDoors: true);
	}

	protected virtual bool IsGoodTarget(Thing thing)
	{
		if (thing is Pawn { Spawned: not false, Downed: false } pawn)
		{
			return !pawn.IsPsychologicallyInvisible();
		}
		return false;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_Berserk obj = (JobGiver_Berserk)base.DeepCopy(resolve);
		obj.maxAttackDistance = maxAttackDistance;
		return obj;
	}
}
