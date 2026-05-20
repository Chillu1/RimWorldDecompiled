using RimWorld;

namespace Verse.AI;

public class ThinkNode_ConditionalWorkModeTag : ThinkNode_Conditional
{
	protected override bool Satisfied(Pawn pawn)
	{
		if (pawn.RaceProps.IsMechanoid && pawn.Faction == Faction.OfPlayer && pawn.relations != null)
		{
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer)?.mechanitor.GetControlGroup(pawn).GetTag(pawn) == tag;
		}
		return false;
	}
}
