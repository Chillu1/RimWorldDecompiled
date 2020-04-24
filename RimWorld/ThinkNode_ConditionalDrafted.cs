using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalDrafted : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Drafted;
		}
	}
}
