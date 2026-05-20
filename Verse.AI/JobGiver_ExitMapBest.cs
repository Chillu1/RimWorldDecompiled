using RimWorld;

namespace Verse.AI;

public class JobGiver_ExitMapBest : JobGiver_ExitMap
{
	protected override bool TryFindGoodExitDest(Pawn pawn, bool canDig, bool canBash, out IntVec3 spot)
	{
		TraverseMode mode = (canDig ? TraverseMode.PassAllDestroyableThings : TraverseMode.ByPawn);
		return RCellFinder.TryFindBestExitSpot(pawn, out spot, mode, canBash);
	}
}
