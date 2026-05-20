namespace Verse;

public class HediffCompProperties_KillAfterDays : HediffCompProperties
{
	public int days;

	public HediffCompProperties_KillAfterDays()
	{
		compClass = typeof(HediffComp_KillAfterDays);
	}
}
