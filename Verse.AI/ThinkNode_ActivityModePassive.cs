using RimWorld;

namespace Verse.AI;

public class ThinkNode_ActivityModePassive : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.GetComp<CompActivity>() != null)
		{
			return pawn.GetComp<CompActivity>().IsDormant;
		}
		return false;
	}
}
