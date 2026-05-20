using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNodeConditional_EscapingHoldingPlatform : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.TryGetComp<CompHoldingPlatformTarget>()?.isEscaping ?? false;
	}
}
