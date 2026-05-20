using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class Building_FrenzyInducer : Building
{
	private const int UpdateHediffInterval = 30;

	private const float Radius = 8.9f;

	private CompPowerTrader powerTraderComp;

	private CompPowerTrader PowerTraderComp => powerTraderComp ?? (powerTraderComp = GetComp<CompPowerTrader>());

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		if (!ModLister.CheckAnomaly("Frenzy inducer"))
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
			if (pawn.RaceProps.Humanlike && base.Position.InHorDistOf(pawn.PositionHeld, 8.9f))
			{
				((Hediff_FrenzyField)pawn.health.GetOrAddHediff(HediffDefOf.FrenzyField)).lastTickInRangeOfInducer = GenTicks.TicksGame;
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
		return text + "FrenzyInducerEffectDescription".Translate().Resolve().CapitalizeFirst();
	}
}
