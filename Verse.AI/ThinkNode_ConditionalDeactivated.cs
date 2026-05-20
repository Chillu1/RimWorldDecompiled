using RimWorld;

namespace Verse.AI;

public class ThinkNode_ConditionalDeactivated : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.IsDeactivated();
	}
}
