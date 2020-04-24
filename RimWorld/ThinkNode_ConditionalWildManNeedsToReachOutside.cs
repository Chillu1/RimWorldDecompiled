using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalWildManNeedsToReachOutside : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return WildManUtility.WildManShouldReachOutsideNow(pawn);
		}
	}
}
