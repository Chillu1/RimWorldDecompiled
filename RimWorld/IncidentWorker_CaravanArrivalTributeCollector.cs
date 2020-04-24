using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CaravanArrivalTributeCollector : IncidentWorker_TraderCaravanArrival
	{
		public const string TributeCollectorTraderKindCategory = "TributeCollector";

		protected override bool TryResolveParmsGeneral(IncidentParms parms)
		{
			if (!base.TryResolveParmsGeneral(parms))
			{
				return false;
			}
			Map map = (Map)parms.target;
			parms.faction = Faction.Empire;
			parms.traderKind = DefDatabase<TraderKindDef>.AllDefsListForReading.Where((TraderKindDef t) => t.category == "TributeCollector").RandomElementByWeight((TraderKindDef t) => TraderKindCommonality(t, map, parms.faction));
			return true;
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			return FactionCanBeGroupSource(Faction.Empire, (Map)parms.target);
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
}
