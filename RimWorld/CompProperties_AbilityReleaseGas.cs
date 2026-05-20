using Verse;

namespace RimWorld;

public class CompProperties_AbilityReleaseGas : CompProperties_AbilityEffect
{
	public GasType gasType;

	public int cellsToFill;

	public CompProperties_AbilityReleaseGas()
	{
		compClass = typeof(CompAbilityEffect_ReleaseGas);
	}
}
