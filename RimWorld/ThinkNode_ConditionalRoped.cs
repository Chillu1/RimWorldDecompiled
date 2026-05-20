using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalRoped : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.roping?.IsRoped ?? false;
		}
	}
}
