using Verse;

namespace RimWorld;

public class CompProperties_TargetEffect_MoteOnTarget : CompProperties
{
	public ThingDef moteDef;

	public CompProperties_TargetEffect_MoteOnTarget()
	{
		compClass = typeof(ComTargetEffect_MoteOnTarget);
	}
}
