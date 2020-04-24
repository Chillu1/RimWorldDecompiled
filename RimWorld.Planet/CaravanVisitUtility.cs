using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	[StaticConstructorOnStartup]
	public static class CaravanVisitUtility
	{
		private static readonly Texture2D TradeCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/Trade");

		public static Settlement SettlementVisitedNow(Caravan caravan)
		{
			if (!caravan.Spawned || caravan.pather.Moving)
			{
				return null;
			}
			List<Settlement> settlementBases = Find.WorldObjects.SettlementBases;
			for (int i = 0; i < settlementBases.Count; i++)
			{
				Settlement settlement = settlementBases[i];
				if (settlement.Tile == caravan.Tile && settlement.Faction != caravan.Faction && settlement.Visitable)
				{
					return settlement;
				}
			}
			return null;
		}

		public static Command TradeCommand(Caravan caravan, Faction faction = null, TraderKindDef trader = null)
		{
			Pawn bestNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan, faction, trader);
			Command_Action command_Action = new Command_Action();
			command_Action.defaultLabel = "CommandTrade".Translate();
			command_Action.defaultDesc = "CommandTradeDesc".Translate();
			command_Action.icon = TradeCommandTex;
			command_Action.action = delegate
			{
				Settlement settlement = SettlementVisitedNow(caravan);
				if (settlement != null && settlement.CanTradeNow)
				{
					Find.WindowStack.Add(new Dialog_Trade(bestNegotiator, settlement));
					PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(settlement.Goods.OfType<Pawn>(), "LetterRelatedPawnsTradingWithSettlement".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent);
				}
			};
			if (bestNegotiator == null)
			{
				if (trader != null && trader.permitRequiredForTrading != null && !caravan.PawnsListForReading.Any((Pawn p) => p.royalty != null && p.royalty.HasPermit(trader.permitRequiredForTrading, faction)))
				{
					command_Action.Disable("CommandTradeFailNeedPermit".Translate(trader.permitRequiredForTrading.LabelCap));
				}
				else
				{
					command_Action.Disable("CommandTradeFailNoNegotiator".Translate());
				}
			}
			if (bestNegotiator != null && bestNegotiator.skills.GetSkill(SkillDefOf.Social).TotallyDisabled)
			{
				command_Action.Disable("CommandTradeFailSocialDisabled".Translate());
			}
			return command_Action;
		}
	}
}
