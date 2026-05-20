using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_HiveDefense : JobGiver_AIFightEnemies
{
	protected override IntVec3 GetFlagPosition(Pawn pawn)
	{
		if (pawn.mindState.duty.focus.Thing is Hive { Spawned: not false } hive)
		{
			return hive.Position;
		}
		return pawn.Position;
	}

	protected override float GetFlagRadius(Pawn pawn)
	{
		return pawn.mindState.duty.radius;
	}

	protected override Job MeleeAttackJob(Pawn pawn, Thing enemyTarget)
	{
		Job job = base.MeleeAttackJob(pawn, enemyTarget);
		job.attackDoorIfTargetLost = true;
		return job;
	}
}
