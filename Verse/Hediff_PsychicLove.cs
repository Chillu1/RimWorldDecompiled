using RimWorld;

namespace Verse
{
	public class Hediff_PsychicLove : HediffWithTarget
	{
		public override void Notify_RelationAdded(Pawn otherPawn, PawnRelationDef relationDef)
		{
			if (otherPawn == target && (relationDef == PawnRelationDefOf.Lover || relationDef == PawnRelationDefOf.Fiance || relationDef == PawnRelationDefOf.Spouse))
			{
				pawn.health.RemoveHediff(this);
			}
		}
	}
}
