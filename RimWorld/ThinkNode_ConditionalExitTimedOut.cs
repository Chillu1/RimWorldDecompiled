using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalExitTimedOut : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.mindState.exitMapAfterTick >= 0)
			{
				return Find.TickManager.TicksGame > pawn.mindState.exitMapAfterTick;
			}
			return false;
		}
	}
}
