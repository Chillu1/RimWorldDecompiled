using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalInNonPlayerHomeMap : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.MapHeld != null)
			{
				return !pawn.MapHeld.IsPlayerHome;
			}
			return false;
		}
	}
}
