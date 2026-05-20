namespace Verse;

public class HediffCompProperties_SeverityPerDay : HediffCompProperties
{
	public float severityPerDay;

	public bool showDaysToRecover;

	public bool showHoursToRecover;

	public float mechanitorFactor = 1f;

	public float reverseSeverityChangeChance;

	public FloatRange severityPerDayRange = FloatRange.Zero;

	public float minAge;

	public HediffCompProperties_SeverityPerDay()
	{
		compClass = typeof(HediffComp_SeverityPerDay);
	}

	public float CalculateSeverityPerDay()
	{
		float num = severityPerDay + severityPerDayRange.RandomInRange;
		if (Rand.Chance(reverseSeverityChangeChance))
		{
			num *= -1f;
		}
		return num;
	}
}
