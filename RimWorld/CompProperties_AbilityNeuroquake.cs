namespace RimWorld;

public class CompProperties_AbilityNeuroquake : CompProperties_AbilityEffect
{
	public int goodwillImpactForNeuroquake;

	public int goodwillImpactForBerserk;

	public int worldRangeTiles;

	public float mentalStateRadius;

	public CompProperties_AbilityNeuroquake()
	{
		compClass = typeof(CompAbilityEffect_Neuroquake);
	}
}
