namespace RimWorld;

public class CompProperties_AbilityRequiresTrainable : CompProperties_AbilityEffect
{
	public TrainableDef trainableDef;

	public bool aiCanCastWithoutTrainable;

	public CompProperties_AbilityRequiresTrainable()
	{
		compClass = typeof(CompAbilityEffect_RequiresTrainable);
	}
}
