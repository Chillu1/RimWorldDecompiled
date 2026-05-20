using Verse;

namespace RimWorld;

public class CompProperties_DestroyAfterEffect : CompProperties
{
	public EffecterDef effecterDef;

	public CompProperties_DestroyAfterEffect()
	{
		compClass = typeof(CompDestroyAfterEffect);
	}
}
