using Verse;

namespace RimWorld;

public class CompProperties_ProducesBioferrite : CompProperties
{
	public float bioferriteDensity = 1f;

	public CompProperties_ProducesBioferrite()
	{
		compClass = typeof(CompProducesBioferrite);
	}
}
