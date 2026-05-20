using Verse;

namespace RimWorld;

public class CompProperties_GrayStatueGas : CompProperties_GrayStatue
{
	public GasType gas = GasType.DeadlifeDust;

	public CompProperties_GrayStatueGas()
	{
		compClass = typeof(CompGrayStatueGas);
	}
}
