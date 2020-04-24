using RimWorld;

namespace Verse.AI
{
	public class MentalBreakWorker_Catatonic : MentalBreakWorker
	{
		public override bool BreakCanOccur(Pawn pawn)
		{
			if (pawn.IsColonist && pawn.Spawned)
			{
				return base.BreakCanOccur(pawn);
			}
			return false;
		}

		public override bool TryStart(Pawn pawn, string reason, bool causedByMood)
		{
			pawn.health.AddHediff(HediffDefOf.CatatonicBreakdown);
			TrySendLetter(pawn, "LetterCatatonicMentalBreak", reason);
			return true;
		}
	}
}
