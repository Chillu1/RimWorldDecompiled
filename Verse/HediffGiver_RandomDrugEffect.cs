namespace Verse
{
	public class HediffGiver_RandomDrugEffect : HediffGiver
	{
		public float baseMtbDays;

		public float minSeverity;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (!(cause.Severity < minSeverity) && Rand.MTBEventOccurs(baseMtbDays, 60000f, 60f) && TryApply(pawn))
			{
				SendLetter(pawn, cause);
			}
		}
	}
}
