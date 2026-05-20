namespace Verse;

public class HediffComp_SeverityFromGasDensityDirect : HediffComp
{
	public HediffCompProperties_SeverityFromGasDensityDirect Props => (HediffCompProperties_SeverityFromGasDensityDirect)props;

	public override bool CompShouldRemove
	{
		get
		{
			if (!base.Pawn.Dead && (base.Pawn.Spawned || base.Pawn.CarriedBy != null))
			{
				return !base.Pawn.PositionHeld.AnyGas(base.Pawn.MapHeld, Props.gasType);
			}
			return true;
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		UpdateSeverity();
	}

	public override void CompPostTick(ref float severityAdjustment)
	{
		if (base.Pawn.IsHashIntervalTick(Props.intervalTicks))
		{
			UpdateSeverity();
		}
	}

	private void UpdateSeverity()
	{
		if (!base.Pawn.Spawned || (Props.gasType == GasType.ToxGas && !ModsConfig.BiotechActive))
		{
			return;
		}
		float num = base.Pawn.MapHeld.gasGrid.DensityPercentAt(base.Pawn.PositionHeld, Props.gasType);
		for (int i = 0; i <= Props.densityStages.Count; i++)
		{
			if (num <= Props.densityStages[i])
			{
				parent.Severity = i + 1;
				break;
			}
		}
	}
}
