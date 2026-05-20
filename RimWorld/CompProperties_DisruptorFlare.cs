using Verse;

namespace RimWorld;

public class CompProperties_DisruptorFlare : CompProperties
{
	public float radius;

	public EffecterDef destroyWarningEffecterDef;

	public EffecterDef effecterDef;

	public CompProperties_DisruptorFlare()
	{
		compClass = typeof(CompDisruptorFlare);
	}
}
