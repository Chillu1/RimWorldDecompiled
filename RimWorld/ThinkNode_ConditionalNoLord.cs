using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ThinkNode_ConditionalNoLord : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		return pawn.GetLord() == null;
	}
}
