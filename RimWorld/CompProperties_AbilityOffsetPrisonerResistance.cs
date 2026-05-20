namespace RimWorld;

public class CompProperties_AbilityOffsetPrisonerResistance : CompProperties_AbilityEffect
{
	public float offset;

	public CompProperties_AbilityOffsetPrisonerResistance()
	{
		compClass = typeof(CompAbilityEffect_OffsetPrisonerResistance);
	}
}
