using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalPrisonerInPrisonCell : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			if (!pawn.IsPrisoner)
			{
				return false;
			}
			return pawn.GetRoom()?.isPrisonCell ?? false;
		}
	}
}
