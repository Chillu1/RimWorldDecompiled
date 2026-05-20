using Verse;

namespace RimWorld;

public class IngestionOutcomeDoer_Psilocap : IngestionOutcomeDoer
{
	public float chanceBreakdown = 0.01f;

	public float chanceInspiration = 0.02f;

	protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
	{
		if (Rand.Value < chanceBreakdown)
		{
			pawn.mindState.mentalBreaker.TryDoMentalBreak("MentalBreakReason_Psilocap".Translate(), MentalBreakDefOf.Catatonic);
		}
		else if (Rand.Value < chanceInspiration)
		{
			pawn.mindState.inspirationHandler.TryStartInspiration(InspirationDefOf.Inspired_Creativity, "LetterInspirationBeginPsilocap".Translate());
		}
	}
}
