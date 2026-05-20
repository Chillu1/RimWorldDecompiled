using Verse;

namespace RimWorld
{
	public class JobGiver_AIDefendOverseer : JobGiver_AIDefendPawn
	{
		protected override Pawn GetDefendee(Pawn pawn)
		{
			return pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);
		}

		protected override float GetFlagRadius(Pawn pawn)
		{
			return 5f;
		}
	}
}
