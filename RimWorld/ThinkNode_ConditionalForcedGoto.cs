using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalForcedGoto : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.mindState.forcedGotoPosition.IsValid;
		}
	}
}
