namespace RimWorld;

public class CompProperties_AbilitySocialInteraction : CompProperties_AbilityEffect
{
	public InteractionDef interactionDef;

	public bool canApplyToMentallyBroken;

	public bool canApplyToUnconscious;

	public bool canApplyToAsleep;

	public CompProperties_AbilitySocialInteraction()
	{
		compClass = typeof(CompAbilityEffect_SocialInteraction);
	}
}
