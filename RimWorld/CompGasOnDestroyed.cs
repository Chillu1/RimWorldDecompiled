using Verse;

namespace RimWorld;

public class CompGasOnDestroyed : ThingComp
{
	private CompProperties_GasOnDestroyed Props => (CompProperties_GasOnDestroyed)props;

	public override void PostDestroy(DestroyMode mode, Map previousMap)
	{
		base.PostDestroy(mode, previousMap);
		if (mode != DestroyMode.Vanish)
		{
			GasUtility.AddGas(parent.PositionHeld, previousMap, Props.gasType, Props.amount);
		}
	}
}
