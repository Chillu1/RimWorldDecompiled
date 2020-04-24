using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_WanderInGatheringArea : JobGiver_Wander
	{
		protected override IntVec3 GetExactWanderDest(Pawn pawn)
		{
			if (!GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, out IntVec3 result))
			{
				return IntVec3.Invalid;
			}
			return result;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			throw new NotImplementedException();
		}
	}
}
