using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace RimWorld.Planet;

[StaticConstructorOnStartup]
public static class FactionGiftUtility
{
	private static readonly Texture2D OfferGiftsCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/OfferGifts");

	public static Command OfferGiftsCommand(Caravan caravan, Settlement settlement)
	{
		return new Command_Action
		{
			defaultLabel = "CommandOfferGifts".Translate(),
			defaultDesc = "CommandOfferGiftsDesc".Translate(),
			icon = OfferGiftsCommandTex,
			action = delegate
			{
				Pawn playerNegotiator = BestCaravanPawnUtility.FindBestNegotiator(caravan);
				Find.WindowStack.Add(new Dialog_Trade(playerNegotiator, settlement, giftsOnly: true));
			}
		};
	}

	public static void GiveGift(List<Tradeable> tradeables, Faction giveTo, GlobalTargetInfo lookTarget)
	{
		int goodwillChange = GetGoodwillChange(tradeables, giveTo);
		for (int i = 0; i < tradeables.Count; i++)
		{
			if (tradeables[i].ActionToDo == TradeAction.PlayerSells)
			{
				tradeables[i].ResolveTrade();
			}
		}
		if (giveTo.PlayerGoodwill == 100)
		{
			SendGiftNotAppreciatedMessage(giveTo, lookTarget);
		}
		Faction.OfPlayer.TryAffectGoodwillWith(giveTo, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.GaveGift);
	}

	public static void GiveGift(List<ActiveTransporterInfo> pods, Settlement giveTo)
	{
		int goodwillChange = GetGoodwillChange(pods.Cast<IThingHolder>(), giveTo);
		for (int i = 0; i < pods.Count; i++)
		{
			ThingOwner innerContainer = pods[i].innerContainer;
			for (int num = innerContainer.Count - 1; num >= 0; num--)
			{
				GiveGiftInternal(innerContainer[num], innerContainer[num].stackCount, giveTo.Faction);
				if (num < innerContainer.Count)
				{
					innerContainer.RemoveAt(num);
				}
			}
		}
		if (giveTo.Faction.PlayerGoodwill == 100)
		{
			SendGiftNotAppreciatedMessage(giveTo.Faction, giveTo);
		}
		Faction.OfPlayer.TryAffectGoodwillWith(giveTo.Faction, goodwillChange, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.GaveGift);
	}

	private static void GiveGiftInternal(Thing thing, int count, Faction giveTo)
	{
		Thing thing2 = thing.SplitOff(count);
		if (thing2 is Pawn pawn)
		{
			pawn.SetFaction(giveTo);
		}
		thing2.DestroyOrPassToWorld();
	}

	public static bool CheckCanCarryGift(List<Tradeable> tradeables, ITrader trader)
	{
		if (!(trader is Pawn pawn))
		{
			return true;
		}
		float num = 0f;
		float num2 = 0f;
		Lord lord = pawn.GetLord();
		if (lord != null)
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn2 = lord.ownedPawns[i];
				TraderCaravanRole traderCaravanRole = pawn2.GetTraderCaravanRole();
				if ((pawn2.RaceProps.Humanlike && traderCaravanRole != TraderCaravanRole.Guard) || traderCaravanRole == TraderCaravanRole.Carrier)
				{
					num += MassUtility.Capacity(pawn2);
					num2 += MassUtility.GearAndInventoryMass(pawn2);
				}
			}
		}
		else
		{
			num = MassUtility.Capacity(pawn);
			num2 = MassUtility.GearAndInventoryMass(pawn);
		}
		float num3 = 0f;
		for (int j = 0; j < tradeables.Count; j++)
		{
			if (tradeables[j].ActionToDo == TradeAction.PlayerSells)
			{
				int num4 = Mathf.Min(tradeables[j].CountToTransferToDestination, tradeables[j].CountHeldBy(Transactor.Colony));
				if (num4 > 0)
				{
					num3 += tradeables[j].AnyThing.GetStatValue(StatDefOf.Mass) * (float)num4;
				}
			}
		}
		if (num2 + num3 <= num)
		{
			return true;
		}
		float num5 = num - num2;
		if (num5 <= 0f)
		{
			Messages.Message("MessageCantGiveGiftBecauseCantCarryEncumbered".Translate(), MessageTypeDefOf.RejectInput, historical: false);
		}
		else
		{
			Messages.Message("MessageCantGiveGiftBecauseCantCarry".Translate(num3.ToStringMass(), num5.ToStringMass()), MessageTypeDefOf.RejectInput, historical: false);
		}
		return false;
	}

	public static int GetGoodwillChange(IEnumerable<IThingHolder> pods, Settlement giveTo)
	{
		float num = 0f;
		foreach (IThingHolder pod in pods)
		{
			ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
			for (int i = 0; i < directlyHeldThings.Count; i++)
			{
				float singlePrice;
				if (directlyHeldThings[i].def == ThingDefOf.Silver)
				{
					singlePrice = directlyHeldThings[i].MarketValue;
				}
				else
				{
					float priceFactorSell_TraderPriceType = ((giveTo.TraderKind != null) ? giveTo.TraderKind.PriceTypeFor(directlyHeldThings[i].def, TradeAction.PlayerSells).PriceMultiplier() : 1f);
					singlePrice = TradeUtility.GetPricePlayerSell(directlyHeldThings[i], priceFactorSell_TraderPriceType, 1f, 0f, giveTo.TradePriceImprovementOffsetForPlayer, 0f, 0f);
				}
				num += GetBaseGoodwillChange(directlyHeldThings[i], directlyHeldThings[i].stackCount, singlePrice, giveTo.Faction);
			}
		}
		return PostProcessedGoodwillChange(num, giveTo.Faction);
	}

	public static int GetGoodwillChange(List<Tradeable> tradeables, Faction theirFaction)
	{
		float num = 0f;
		for (int i = 0; i < tradeables.Count; i++)
		{
			if (tradeables[i].ActionToDo == TradeAction.PlayerSells)
			{
				int count = Mathf.Min(tradeables[i].CountToTransferToDestination, tradeables[i].CountHeldBy(Transactor.Colony));
				num += GetBaseGoodwillChange(tradeables[i].AnyThing, count, tradeables[i].GetPriceFor(TradeAction.PlayerSells), theirFaction);
			}
		}
		return PostProcessedGoodwillChange(num, theirFaction);
	}

	private static float GetBaseGoodwillChange(Thing anyThing, int count, float singlePrice, Faction theirFaction)
	{
		if (count <= 0)
		{
			return 0f;
		}
		float num = singlePrice * (float)count;
		if (anyThing is Pawn { IsPrisoner: not false } pawn && pawn.Faction == theirFaction)
		{
			num *= 2f;
		}
		return num / 40f;
	}

	private static int PostProcessedGoodwillChange(float goodwillChange, Faction theirFaction)
	{
		float num = theirFaction.PlayerGoodwill;
		float num2 = 0f;
		SimpleCurve giftGoodwillFactorRelationsCurve = DiplomacyTuning.GiftGoodwillFactorRelationsCurve;
		while (goodwillChange >= 0.25f)
		{
			num2 += 0.25f * giftGoodwillFactorRelationsCurve.Evaluate(Mathf.Min(num + num2, 100f));
			goodwillChange -= 0.25f;
			if (num2 >= 200f)
			{
				break;
			}
		}
		if (num2 < 200f)
		{
			num2 += goodwillChange * giftGoodwillFactorRelationsCurve.Evaluate(Mathf.Min(num + num2, 100f));
		}
		return (int)Mathf.Min(num2, 200f);
	}

	private static void SendGiftNotAppreciatedMessage(Faction giveTo, GlobalTargetInfo lookTarget)
	{
		Messages.Message("MessageGiftGivenButNotAppreciated".Translate(giveTo.Name), lookTarget, MessageTypeDefOf.NegativeEvent);
	}
}
