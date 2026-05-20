using Verse;

namespace RimWorld;

public class CompProperties_EquippableAbility : CompProperties
{
	public AbilityDef abilityDef;

	public CompProperties_EquippableAbility()
	{
		compClass = typeof(CompEquippableAbility);
	}
}
