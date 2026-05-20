using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleeForDistance : ThinkNode_JobGiver
{
	protected FloatRange enemyDistToFleeRange = new FloatRange(2.9f, 7.9f);

	protected FloatRange fleeDistRange = new FloatRange(13.5f, 20f);

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (GenAI.EnemyIsNear(pawn, enemyDistToFleeRange.min, out var threat, meleeOnly: true, requireLos: true))
		{
			return null;
		}
		if (GenAI.EnemyIsNear(pawn, enemyDistToFleeRange.max, out threat, meleeOnly: true, requireLos: true))
		{
			return FleeUtility.FleeJob(pawn, threat, Mathf.CeilToInt(fleeDistRange.RandomInRange));
		}
		return null;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_FleeForDistance obj = (JobGiver_FleeForDistance)base.DeepCopy(resolve);
		obj.enemyDistToFleeRange = enemyDistToFleeRange;
		obj.fleeDistRange = fleeDistRange;
		return obj;
	}
}
