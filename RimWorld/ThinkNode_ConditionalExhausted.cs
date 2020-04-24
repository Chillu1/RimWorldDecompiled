using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalExhausted : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.needs.rest != null)
			{
				return (int)pawn.needs.rest.CurCategory >= 3;
			}
			return false;
		}
	}
}
