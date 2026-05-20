namespace Verse;

public class HediffCompProperties_DisappearsDisableable : HediffCompProperties_Disappears
{
	public bool initiallyDisabled;

	public HediffCompProperties_DisappearsDisableable()
	{
		compClass = typeof(HediffComp_DisappearsDisableable);
	}
}
