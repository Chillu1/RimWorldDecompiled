namespace Verse
{
	public class HediffGiver_Refresh : HediffGiver
	{
		public override void OnIntervalPassed(Pawn pawn, Hediff cause)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
			if (firstHediffOfDef != null)
			{
				firstHediffOfDef.ageTicks = 0;
			}
			else if (TryApply(pawn))
			{
				SendLetter(pawn, cause);
			}
		}
	}
}
