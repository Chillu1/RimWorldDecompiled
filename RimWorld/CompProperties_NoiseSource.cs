using Verse;

namespace RimWorld;

public class CompProperties_NoiseSource : CompProperties
{
	public float radius;

	public CompProperties_NoiseSource()
	{
		compClass = typeof(CompNoiseSource);
	}
}
