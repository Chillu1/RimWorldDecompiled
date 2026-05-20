using Verse;

namespace RimWorld;

public class CompProperties_DropMechPods : CompProperties_EffectWithDest
{
	public IntRange numPods;

	public CompProperties_DropMechPods()
	{
		compClass = typeof(CompAbilityEffect_DropMechPods);
	}
}
