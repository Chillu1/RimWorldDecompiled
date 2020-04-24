using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalDowned : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.Downed;
		}
	}
}
