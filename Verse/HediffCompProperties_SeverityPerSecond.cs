namespace Verse;

public class HediffCompProperties_SeverityPerSecond : HediffCompProperties
{
	public float severityPerSecond;

	public FloatRange severityPerSecondRange = FloatRange.Zero;

	public HediffCompProperties_SeverityPerSecond()
	{
		compClass = typeof(HediffComp_SeverityPerSecond);
	}

	public float CalculateSeverityPerSecond()
	{
		return severityPerSecond + severityPerSecondRange.RandomInRange;
	}
}
