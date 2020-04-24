using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalReleased : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (pawn.guest != null)
			{
				return pawn.guest.Released;
			}
			return false;
		}
	}
}
