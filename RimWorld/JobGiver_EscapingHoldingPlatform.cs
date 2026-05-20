using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_EscapingHoldingPlatform : JobGiver_AIFightEnemies
{
	public JobGiver_EscapingHoldingPlatform()
	{
		chaseTarget = true;
	}

	protected override Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
	{
		Job job = base.MeleeAttackJob(pawn, enemyTarget);
		job.attackDoorIfTargetLost = true;
		job.canBashDoors = true;
		return job;
	}

	protected override bool ShouldLoseTarget(Pawn pawn)
	{
		Thing enemyTarget = pawn.mindState.enemyTarget;
		if (!enemyTarget.Destroyed)
		{
			return ((IAttackTarget)enemyTarget).ThreatDisabled(pawn);
		}
		return true;
	}
}
