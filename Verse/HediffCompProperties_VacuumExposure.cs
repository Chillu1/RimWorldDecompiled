namespace Verse;

public class HediffCompProperties_VacuumExposure : HediffCompProperties
{
	public float severityPerSecondUnexposed;

	public HediffCompProperties_VacuumExposure()
	{
		compClass = typeof(HediffComp_VacuumExposure);
	}
}
