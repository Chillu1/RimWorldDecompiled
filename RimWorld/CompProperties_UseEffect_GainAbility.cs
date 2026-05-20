namespace RimWorld;

public class CompProperties_UseEffect_GainAbility : CompProperties_UseEffect
{
	public AbilityDef ability;

	public CompProperties_UseEffect_GainAbility()
	{
		compClass = typeof(CompUseEffect_GainAbility);
	}
}
