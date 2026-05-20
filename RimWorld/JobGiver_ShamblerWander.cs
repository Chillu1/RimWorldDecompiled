using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ShamblerWander : JobGiver_Wander
{
	private static float ChanceToWander = 0.2f;

	public JobGiver_ShamblerWander()
	{
		locomotionUrgency = LocomotionUrgency.Amble;
		ticksBetweenWandersRange = new IntRange(480, 900);
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		pawn.mindState.nextMoveOrderIsWait = !Rand.Chance(ChanceToWander);
		return base.TryGiveJob(pawn);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		if (pawn.IsGhoul)
		{
			return pawn.Position;
		}
		return GetWanderRootHerd(pawn);
	}

	private IntVec3 GetWanderRootHerd(Pawn pawn)
	{
		return WanderUtility.GetHerdWanderRoot(isHerdValidator: delegate(Thing t)
		{
			if (!((Pawn)t).IsShambler || t == pawn)
			{
				return false;
			}
			return (t.Faction == pawn.Faction) ? true : false;
		}, pawn: pawn);
	}
}
