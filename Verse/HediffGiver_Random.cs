namespace Verse
{
	public class HediffGiver_Random : HediffGiver
	{
		public float mtbDays;

		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			if (Rand.MTBEventOccurs(mtbDays, 60000f, 60f) && TryApply(pawn))
			{
				SendLetter(pawn, cause);
			}
		}
	}
}
