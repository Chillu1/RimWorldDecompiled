using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ThinkNode_ConditionalHasLordDuty : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.GetLord() != null)
		{
			return pawn.GetLord().CurLordToil.AssignsDuties;
		}
		return false;
	}
}
