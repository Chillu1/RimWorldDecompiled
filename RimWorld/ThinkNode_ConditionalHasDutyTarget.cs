using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalHasDutyTarget : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.duty != null)
			{
				return pawn.mindState.duty.focus.IsValid;
			}
			return false;
		}
	}
}
