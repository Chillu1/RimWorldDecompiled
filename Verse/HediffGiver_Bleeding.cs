namespace Verse
{
	public class HediffGiver_Bleeding : HediffGiver
	{
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			HediffSet hediffSet = pawn.health.hediffSet;
			if (hediffSet.BleedRateTotal >= 0.1f)
			{
				HealthUtility.AdjustSeverity(pawn, hediff, hediffSet.BleedRateTotal * 0.001f);
			}
			else
			{
				HealthUtility.AdjustSeverity(pawn, hediff, -0.00033333333f);
			}
		}
	}
}
