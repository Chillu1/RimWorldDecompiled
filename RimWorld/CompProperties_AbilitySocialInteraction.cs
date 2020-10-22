namespace RimWorld
{
	public class CompProperties_AbilitySocialInteraction : CompProperties_AbilityEffect
	{
		public InteractionDef interactionDef;

		public bool canApplyToMentallyBroken;

		public CompProperties_AbilitySocialInteraction()
		{
			compClass = typeof(CompAbilityEffect_SocialInteraction);
		}
	}
}
