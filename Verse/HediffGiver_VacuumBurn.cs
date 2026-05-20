using RimWorld;

namespace Verse;

public class HediffGiver_VacuumBurn : HediffGiver
{
	private static readonly IntRange BurnDamageRange = new IntRange(1, 2);

	private static readonly SimpleCurve VacuumSecondsBurnRate = new SimpleCurve
	{
		new CurvePoint(0.5f, 20f),
		new CurvePoint(1f, 5f)
	};

	public override void OnIntervalPassed(Pawn pawn, Hediff cause)
	{
		if (!ModsConfig.OdysseyActive || !pawn.Spawned || !pawn.Map.Biome.inVacuum || !pawn.HarmedByVacuum)
		{
			return;
		}
		float vacuum = pawn.Position.GetVacuum(pawn.Map);
		if (vacuum < 0.5f)
		{
			return;
		}
		int num = GenTicks.TicksGame - pawn.lastVacuumBurntTick;
		int num2 = VacuumSecondsBurnRate.Evaluate(vacuum).SecondsToTicks();
		if (num >= num2)
		{
			pawn.lastVacuumBurntTick = GenTicks.TicksGame;
			if (VacuumUtility.TryGetVacuumBurnablePart(pawn, out var p))
			{
				DamageInfo dinfo = new DamageInfo(DamageDefOf.VacuumBurn, BurnDamageRange.RandomInRange, 999f, -1f, null, p);
				pawn.TakeDamage(dinfo);
			}
		}
	}
}
