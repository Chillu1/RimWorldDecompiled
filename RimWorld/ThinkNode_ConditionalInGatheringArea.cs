using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalInGatheringArea : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty == null)
			{
				return false;
			}
			IntVec3 cell = pawn.mindState.duty.focus.Cell;
			return GatheringsUtility.InGatheringArea(pawn.Position, cell, pawn.Map);
		}
	}
}
