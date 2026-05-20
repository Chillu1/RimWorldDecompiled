using RimWorld;

namespace Verse.AI;

public class ThinkNode_ActivityModeActive : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.GetComp<CompActivity>() != null)
		{
			return pawn.GetComp<CompActivity>().IsActive;
		}
		return false;
	}
}
