using RimWorld;

namespace Verse;

public class Hediff_PsychicLove : HediffWithTarget
{
	public override string LabelBase => base.LabelBase + " " + def.targetPrefix + " " + target?.LabelShortCap;

	public override void Notify_RelationAdded(Pawn otherPawn, PawnRelationDef relationDef)
	{
		if (otherPawn == target && (relationDef == PawnRelationDefOf.Lover || relationDef == PawnRelationDefOf.Fiance || relationDef == PawnRelationDefOf.Spouse))
		{
			pawn.health.RemoveHediff(this);
		}
	}
}
