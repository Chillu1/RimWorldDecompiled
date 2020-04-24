using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalStarving : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.needs.food != null)
			{
				return (int)pawn.needs.food.CurCategory >= 3;
			}
			return false;
		}
	}
}
