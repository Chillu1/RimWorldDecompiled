using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SightstealerAttack : ThinkNode_JobGiver
{
	private const int MinMeleeChaseTicks = 420;

	private const int MaxMeleeChaseTicks = 900;

	private int targetAcquireRadius = -1;

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_SightstealerAttack obj = (JobGiver_SightstealerAttack)base.DeepCopy(resolve);
		obj.targetAcquireRadius = targetAcquireRadius;
		return obj;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = FindPawnTarget(pawn);
		if (pawn2 != null)
		{
			return MeleeAttackJob(pawn2, canBashFences: false);
		}
		return null;
	}

	private Job MeleeAttackJob(Thing target, bool canBashFences)
	{
		Job job = JobMaker.MakeJob(JobDefOf.AttackMelee, target);
		job.maxNumMeleeAttacks = 1;
		job.expiryInterval = Rand.Range(420, 900);
		job.attackDoorIfTargetLost = true;
		job.canBashFences = canBashFences;
		return job;
	}

	private Pawn FindPawnTarget(Pawn pawn)
	{
		return (Pawn)AttackTargetFinder.BestAttackTarget(pawn, TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable, delegate(Thing thing)
		{
			if (thing.Isnt<Pawn>() || (int)thing.def.race.intelligence < 1)
			{
				return false;
			}
			return (targetAcquireRadius <= 0 || !(pawn.Position.DistanceTo(thing.PositionHeld) > (float)targetAcquireRadius)) ? true : false;
		});
	}
}
