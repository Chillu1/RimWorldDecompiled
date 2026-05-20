using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class IncidentWorker_TraderCaravanArrival : IncidentWorker_NeutralGroup
{
	public const string SlaverTraderKindCategory = "Slaver";

	protected override PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Trader;

	public override bool FactionCanBeGroupSource(Faction f, IncidentParms parms, bool desperate = false)
	{
		if (!base.FactionCanBeGroupSource(f, parms, desperate) || f.def.caravanTraderKinds.Count == 0)
		{
			return false;
		}
		Map map = (Map)parms.target;
		return f.def.caravanTraderKinds.Any((TraderKindDef t) => TraderKindCommonality(t, map, f) > 0f);
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms))
		{
			return false;
		}
		Map map = (Map)parms.target;
		if (parms.faction != null && NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, parms.faction))
		{
			return false;
		}
		return true;
	}

	protected override bool TryResolveParmsGeneral(IncidentParms parms)
	{
		if (!base.TryResolveParmsGeneral(parms))
		{
			return false;
		}
		if (parms.traderKind == null)
		{
			Map map = (Map)parms.target;
			if (!parms.faction.def.caravanTraderKinds.TryRandomElementByWeight((TraderKindDef traderDef) => TraderKindCommonality(traderDef, map, parms.faction), out parms.traderKind))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual float TraderKindCommonality(TraderKindDef traderKind, Map map, Faction faction)
	{
		if (traderKind.faction != null && faction.def != traderKind.faction)
		{
			return 0f;
		}
		if (ModsConfig.IdeologyActive && faction.ideos != null && traderKind.category == "Slaver")
		{
			foreach (Ideo allIdeo in faction.ideos.AllIdeos)
			{
				if (!allIdeo.IdeoApprovesOfSlavery())
				{
					return 0f;
				}
			}
		}
		if (traderKind.permitRequiredForTrading != null && !map.mapPawns.FreeColonists.Any((Pawn p) => p.royalty != null && p.royalty.HasPermit(traderKind.permitRequiredForTrading, faction)))
		{
			return 0f;
		}
		return traderKind.CalculatedCommonality;
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = (Map)parms.target;
		if (!TryResolveParms(parms))
		{
			return false;
		}
		if (parms.faction.HostileTo(Faction.OfPlayer))
		{
			return false;
		}
		List<Pawn> pawns = SpawnPawns(parms);
		if (pawns.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].needs != null && pawns[i].needs.food != null)
			{
				pawns[i].needs.food.CurLevel = pawns[i].needs.food.MaxLevel;
			}
		}
		TraderKindDef traderKind = null;
		for (int j = 0; j < pawns.Count; j++)
		{
			Pawn pawn = pawns[j];
			if (pawn.TraderKind != null)
			{
				traderKind = pawn.TraderKind;
				break;
			}
		}
		SendLetter(parms, pawns, traderKind);
		if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0].Position, pawns[0].MapHeld, pawns[0], out var result, delegate(IntVec3 c)
		{
			for (int k = 0; k < pawns.Count; k++)
			{
				if (!pawns[k].CanReach(c, PathEndMode.OnCell, Danger.Deadly))
				{
					return false;
				}
			}
			return true;
		}))
		{
			return false;
		}
		LordJob_TradeWithColony lordJob = new LordJob_TradeWithColony(parms.faction, result);
		LordMaker.MakeNewLord(parms.faction, lordJob, map, pawns);
		return true;
	}

	protected virtual void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
	{
		TaggedString letterLabel = "LetterLabelTraderCaravanArrival".Translate(parms.faction.Name, traderKind.label).CapitalizeFirst();
		TaggedString letterText = "LetterTraderCaravanArrival".Translate(parms.faction.NameColored, traderKind.label).CapitalizeFirst();
		letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
		PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
		SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
	}

	protected override void ResolveParmsPoints(IncidentParms parms)
	{
		parms.points = TraderCaravanUtility.GenerateGuardPoints();
	}
}
