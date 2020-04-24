using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalAtDutyLocation : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty != null)
			{
				return pawn.Position == pawn.mindState.duty.focus.Cell;
			}
			return false;
		}
	}
}
