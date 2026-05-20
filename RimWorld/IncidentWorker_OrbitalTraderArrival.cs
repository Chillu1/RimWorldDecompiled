using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_OrbitalTraderArrival : IncidentWorker
{
	private const int MaxShips = 5;

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		if (((Map)parms.target).passingShipManager.passingShips.Count >= 5)
		{
			return false;
		}
		return true;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (parms.traderKind == null && !DefDatabase<TraderKindDef>.AllDefs.Where((TraderKindDef x) => CanSpawn(map, x)).TryRandomElementByWeight((TraderKindDef traderDef) => traderDef.CalculatedCommonality, out parms.traderKind))
		{
			return false;
		}
		TradeShip tradeShip = new TradeShip(parms.traderKind, GetFaction(parms.traderKind));
		tradeShip.WasAnnounced = false;
		if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && (b.GetComp<CompPowerTrader>() == null || b.GetComp<CompPowerTrader>().PowerOn)))
		{
			SendStandardLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(tradeShip.name, tradeShip.def.label, (tradeShip.Faction == null) ? "TraderArrivalNoFaction".Translate() : "TraderArrivalFromFaction".Translate(tradeShip.Faction.Named("FACTION"))), LetterDefOf.PositiveEvent, parms, LookTargets.Invalid);
			tradeShip.WasAnnounced = true;
		}
		map.passingShipManager.AddShip(tradeShip);
		tradeShip.GenerateThings();
		return true;
	}

	private Faction GetFaction(TraderKindDef trader)
	{
		if (trader.faction == null)
		{
			return null;
		}
		if (!Find.FactionManager.AllFactions.Where((Faction f) => f.def == trader.faction).TryRandomElement(out var result))
		{
			return null;
		}
		return result;
	}

	private bool CanSpawn(Map map, TraderKindDef trader)
	{
		if (!trader.orbital)
		{
			return false;
		}
		if (trader.faction == null)
		{
			return true;
		}
		Faction faction = GetFaction(trader);
		if (faction == null)
		{
			return false;
		}
		foreach (Pawn freeColonist in map.mapPawns.FreeColonists)
		{
			if (freeColonist.CanTradeWith(faction, trader).Accepted)
			{
				return true;
			}
		}
		return false;
	}
}
