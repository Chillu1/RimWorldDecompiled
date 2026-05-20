using Verse;

namespace RimWorld;

public class CompProperties_AbilityEffecterOnCaster : CompProperties_AbilityEffect
{
	public EffecterDef effecterDef;

	public float scale = 1f;

	public int maintainTicks;

	public CompProperties_AbilityEffecterOnCaster()
	{
		compClass = typeof(CompAbilityEffect_EffecterOnCaster);
	}
}
