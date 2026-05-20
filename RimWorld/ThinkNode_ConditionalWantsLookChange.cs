using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalWantsLookChange : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (!ModsConfig.IdeologyActive)
			{
				return false;
			}
			if (pawn.style != null)
			{
				return pawn.style.LookChangeDesired;
			}
			return false;
		}
	}
}
