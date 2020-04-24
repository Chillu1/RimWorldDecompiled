using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThinkNode_ConditionalBurning : ThinkNode_Conditional
	{
		protected override bool Satisfied(Pawn pawn)
		{
			return pawn.HasAttachment(ThingDefOf.Fire);
		}
	}
}
