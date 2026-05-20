namespace Verse;

public class HediffCompProperties_PainField : HediffCompProperties
{
	public float painDistance = 5.9f;

	public float painInRange = 0.25f;

	public SimpleCurve activityMultiplier;

	public float psychicSensitivityMultiplier = 1f;

	public float activatedMinimum = 0.2f;

	public bool disableWhenSuppressed = true;

	public HediffCompProperties_PainField()
	{
		compClass = typeof(HediffComp_PainField);
	}
}
