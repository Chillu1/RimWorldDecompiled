using Verse;
using Verse.AI;

namespace RimWorld;

public class ThinkNode_ConditionalRevenantState : ThinkNode_Conditional
{
	public RevenantState state;

	protected override bool Satisfied(Pawn pawn)
	{
		CompRevenant compRevenant = pawn.TryGetComp<CompRevenant>();
		if (compRevenant != null)
		{
			return compRevenant.revenantState == state;
		}
		return false;
	}
}
