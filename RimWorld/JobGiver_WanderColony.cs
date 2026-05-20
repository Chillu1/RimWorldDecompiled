using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_WanderColony : JobGiver_Wander
{
	public JobGiver_WanderColony()
	{
		wanderRadius = 7f;
		ticksBetweenWandersRange = new IntRange(125, 200);
		wanderDestValidator = (Pawn pawn, IntVec3 loc, IntVec3 root) => true;
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		return WanderUtility.GetColonyWanderRoot(pawn);
	}
}
