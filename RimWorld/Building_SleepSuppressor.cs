using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_SleepSuppressor : Building
{
	private const int UpdateHediffInterval = 30;

	private CompPowerTrader powerTraderComp;

	private CompNoiseSource noiseSourceComp;

	private CompPowerTrader PowerTraderComp => powerTraderComp ?? (powerTraderComp = GetComp<CompPowerTrader>());

	private CompNoiseSource NoiseSourceComp => noiseSourceComp ?? (noiseSourceComp = GetComp<CompNoiseSource>());

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Sleep suppressor"))
		{
			Destroy();
		}
		else
		{
			base.SpawnSetup(map, respawningAfterLoad);
		}
	}

	protected override void Tick()
	{
		base.Tick();
		if (!this.IsHashIntervalTick(30) || !PowerTraderComp.PowerOn)
		{
			return;
		}
		List<Pawn> allHumanlikeSpawned = base.Map.mapPawns.AllHumanlikeSpawned;
		for (int i = 0; i < allHumanlikeSpawned.Count; i++)
		{
			Pawn pawn = allHumanlikeSpawned[i];
			if (pawn.RaceProps.Humanlike && base.Position.InHorDistOf(pawn.PositionHeld, NoiseSourceComp.Props.radius))
			{
				((Hediff_SleepSuppression)pawn.health.GetOrAddHediff(HediffDefOf.SleepSuppression)).lastTickInRangeOfSuppressor = GenTicks.TicksGame;
			}
		}
	}

	public override string GetInspectString()
	{
		string text = base.GetInspectString();
		if (!PowerTraderComp.PowerOn)
		{
			return text;
		}
		if (!text.NullOrEmpty())
		{
			text += "\n";
		}
		return text + "SleepSuppressorEffectDescription".Translate().Resolve().CapitalizeFirst();
	}
}
