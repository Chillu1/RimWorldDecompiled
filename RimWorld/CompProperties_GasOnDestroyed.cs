using Verse;

namespace RimWorld;

public class CompProperties_GasOnDestroyed : CompProperties
{
	public GasType gasType;

	public int amount;

	public CompProperties_GasOnDestroyed()
	{
		compClass = typeof(CompGasOnDestroyed);
	}
}
