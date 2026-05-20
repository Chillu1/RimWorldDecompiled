using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public class IncidentWorker_CaravanArrivalTributeCollector : IncidentWorker_TraderCaravanArrival
{
	public const string TributeCollectorTraderKindCategory = "TributeCollector";

	protected override bool TryResolveParmsGeneral(IncidentParms parms)
	{
		if (!base.TryResolveParmsGeneral(parms))
		{
			return false;
		}
		if (Faction.OfEmpire == null)
		{
			return false;
		}
		Map map = (Map)parms.target;
		parms.faction = Faction.OfEmpire;
		parms.traderKind = DefDatabase<TraderKindDef>.AllDefsListForReading.Where((TraderKindDef t) => t.category == "TributeCollector").RandomElementByWeight((TraderKindDef t) => TraderKindCommonality(t, map, parms.faction));
		return true;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (!base.CanFireNowSub(parms) || Faction.OfEmpire == null)
		{
			return false;
		}
		return FactionCanBeGroupSource(Faction.OfEmpire, parms);
	}

	protected override float TraderKindCommonality(TraderKindDef traderKind, Map map, Faction faction)
	{
		return traderKind.CalculatedCommonality;
	}

	protected override void SendLetter(IncidentParms parms, List<Pawn> pawns, TraderKindDef traderKind)
	{
		TaggedString letterLabel = "LetterLabelTributeCollectorArrival".Translate().CapitalizeFirst();
		TaggedString letterText = "LetterTributeCollectorArrival".Translate(parms.faction.Named("FACTION")).CapitalizeFirst();
		letterText += "\n\n" + "LetterCaravanArrivalCommonWarning".Translate();
		PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(pawns, ref letterLabel, ref letterText, "LetterRelatedPawnsNeutralGroup".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
		SendStandardLetter(letterLabel, letterText, LetterDefOf.PositiveEvent, parms, pawns[0]);
	}
}
