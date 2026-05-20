using Verse;

namespace RimWorld;

public class CompProperties_AbilityEffecterOnTarget : CompProperties_AbilityEffect
{
	public EffecterDef effecterDef;

	public int maintainForTicks = -1;

	public float scale = 1f;

	public CompProperties_AbilityEffecterOnTarget()
	{
		compClass = typeof(CompAbilityEffect_EffecterOnTarget);
	}
}
