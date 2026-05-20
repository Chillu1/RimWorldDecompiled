namespace Verse;

public class HediffCompProperties_ChanceToRemove : HediffCompProperties
{
	public int intervalTicks;

	public float chance;

	public HediffCompProperties_ChanceToRemove()
	{
		compClass = typeof(HediffComp_ChanceToRemove);
	}
}
