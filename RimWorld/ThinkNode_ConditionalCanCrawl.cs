using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class ThinkNode_ConditionalCanCrawl : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		Lord lord = pawn.GetLord();
		if (lord != null && lord.LordJob is LordJob_PsychicRitual)
		{
			return false;
		}
		return pawn.health.CanCrawl;
	}
}
