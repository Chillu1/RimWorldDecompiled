using RimWorld;

namespace Verse;

public class HediffComp_SeverityFromGas : HediffComp
{
	public HediffCompProperties_SeverityFromGas Props => (HediffCompProperties_SeverityFromGas)props;

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (Props.gasType == GasType.ToxGas && !ModsConfig.BiotechActive)
		{
			return;
		}
		Pawn pawn = parent.pawn;
		if (!pawn.Spawned || !pawn.IsHashIntervalTick(Props.intervalTicks))
		{
			return;
		}
		if (pawn.Position.AnyGas(pawn.Map, Props.gasType))
		{
			float num = (float)(int)pawn.Position.GasDensity(pawn.Map, Props.gasType) / 255f * Props.severityGasDensityFactor;
			if (Props.exposureStatFactor != null)
			{
				num *= 1f - pawn.GetStatValue(Props.exposureStatFactor);
			}
			severityAdjustment += num;
		}
		else
		{
			severityAdjustment += Props.severityNotExposed;
		}
	}
}
