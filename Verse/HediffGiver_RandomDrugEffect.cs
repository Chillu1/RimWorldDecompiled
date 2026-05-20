namespace Verse;

public class HediffGiver_RandomDrugEffect : HediffGiver
{
	public SimpleCurve severityToMtbDaysCurve;

	public float baseMtbDays;

	public float minSeverity;

	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		if ((severityToMtbDaysCurve != null || !(cause.Severity <= minSeverity)) && (severityToMtbDaysCurve == null || !(cause.Severity <= severityToMtbDaysCurve.Points[0].x)))
		{
			float num = ((severityToMtbDaysCurve != null) ? severityToMtbDaysCurve.Evaluate(cause.Severity) : baseMtbDays);
			float num2 = ChanceFactor(pawn);
			if (num2 != 0f && Rand.MTBEventOccurs(num / num2, 60000f, 60f) && TryApply(pawn))
			{
				SendLetter(pawn, cause);
			}
		}
	}
}
