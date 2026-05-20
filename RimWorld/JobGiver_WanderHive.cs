using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_WanderHive : JobGiver_Wander
{
	public JobGiver_WanderHive()
	{
		wanderRadius = 7.5f;
		ticksBetweenWandersRange = new IntRange(125, 200);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		if (!(pawn.mindState.duty.focus.Thing is Hive { Spawned: not false } hive))
		{
			return pawn.Position;
		}
		return hive.Position;
	}
}
