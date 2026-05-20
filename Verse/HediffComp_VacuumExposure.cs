using RimWorld;

namespace Verse;

public class HediffComp_VacuumExposure : HediffComp
{
	private const int SeverityUpdateInterval = 60;

	public HediffCompProperties_VacuumExposure Props => (HediffCompProperties_VacuumExposure)props;

	public override void CompPostTickInterval(ref float severityAdjustment, int delta)
	{
		if (ModsConfig.OdysseyActive && base.Pawn.IsHashIntervalTick(60, delta))
		{
			float num = Props.severityPerSecondUnexposed;
			if (base.Pawn.Spawned && base.Pawn.Position.GetVacuum(base.Pawn.Map) >= 0.5f && base.Pawn.GetStatValue(StatDefOf.VacuumResistance) < 1f)
			{
				num = 0f;
			}
			severityAdjustment += num;
		}
	}
}
