namespace Verse;

public class HediffCompProperties_Invisibility : HediffCompProperties
{
	public bool visibleToPlayer;

	public int fadeDurationTicks;

	public int recoverFromDisruptedTicks;

	public bool affectedByDisruptor = true;

	public HediffCompProperties_Invisibility()
	{
		compClass = typeof(HediffComp_Invisibility);
	}
}
