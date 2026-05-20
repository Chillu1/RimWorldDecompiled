using RimWorld;

namespace Verse.AI;

public class JobGiver_WanderMapEdge : JobGiver_Wander
{
	public JobGiver_WanderMapEdge()
	{
		wanderRadius = 7f;
		ticksBetweenWandersRange = new IntRange(50, 125);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		if (RCellFinder.TryFindBestExitSpot(pawn, out var spot))
		{
			return spot;
		}
		return pawn.Position;
	}
}
