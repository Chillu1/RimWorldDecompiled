using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class TradeDeal
	{
		private List<Tradeable> tradeables = new List<Tradeable>();

		public List<string> cannotSellReasons = new List<string>();

		public int TradeableCount => tradeables.Count;

		public Tradeable CurrencyTradeable
		{
			get
			{
				for (int i = 0; i < tradeables.Count; i++)
				{
					if ((TradeSession.TradeCurrency == TradeCurrency.Favor) ? tradeables[i].IsFavor : (tradeables[i].ThingDef == ThingDefOf.Silver))
					{
						return tradeables[i];
					}
				}
				return null;
			}
		}

		public List<Tradeable> AllTradeables => tradeables;

		public TradeDeal()
		{
			Reset();
		}

		public void Reset()
		{
			tradeables.Clear();
			cannotSellReasons.Clear();
			AddAllTradeables();
		}

		private void AddAllTradeables()
		{
			foreach (Thing item in TradeSession.trader.ColonyThingsWillingToBuy(TradeSession.playerNegotiator))
			{
				if (TradeUtility.PlayerSellableNow(item, TradeSession.trader))
				{
					if (!TradeSession.playerNegotiator.IsWorldPawn() && !InSellablePosition(item, out string reason))
					{
						if (reason != null && !cannotSellReasons.Contains(reason))
						{
							cannotSellReasons.Add(reason);
						}
					}
					else
					{
						AddToTradeables(item, Transactor.Colony);
					}
				}
			}
			if (!TradeSession.giftMode)
			{
				foreach (Thing good in TradeSession.trader.Goods)
				{
					AddToTradeables(good, Transactor.Trader);
				}
			}
			if (!TradeSession.giftMode && tradeables.Find((Tradeable x) => x.IsCurrency) == null)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
				thing.stackCount = 0;
				AddToTradeables(thing, Transactor.Trader);
			}
			if (TradeSession.TradeCurrency == TradeCurrency.Favor)
			{
				tradeables.Add(new Tradeable_RoyalFavor());
			}
		}

		private bool InSellablePosition(Thing t, out string reason)
		{
			if (!t.Spawned)
			{
				reason = null;
				return false;
			}
			if (t.Position.Fogged(t.Map))
			{
				reason = null;
				return false;
			}
			Room room = t.GetRoom();
			if (room != null)
			{
				int num = GenRadial.NumCellsInRadius(6.9f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
					if (!intVec.InBounds(t.Map) || intVec.GetRoom(t.Map) != room)
					{
						continue;
					}
					List<Thing> thingList = intVec.GetThingList(t.Map);
					for (int j = 0; j < thingList.Count; j++)
					{
						if (thingList[j].PreventPlayerSellingThingsNearby(out reason))
						{
							return false;
						}
					}
				}
			}
			reason = null;
			return true;
		}

		private void AddToTradeables(Thing t, Transactor trans)
		{
			Tradeable tradeable = TransferableUtility.TradeableMatching(t, tradeables);
			if (tradeable == null)
			{
				tradeable = ((!(t is Pawn)) ? new Tradeable() : new Tradeable_Pawn());
				tradeables.Add(tradeable);
			}
			tradeable.AddThing(t, trans);
		}

		public void UpdateCurrencyCount()
		{
			if (CurrencyTradeable == null || TradeSession.giftMode)
			{
				return;
			}
			float num = 0f;
			for (int i = 0; i < tradeables.Count; i++)
			{
				Tradeable tradeable = tradeables[i];
				if (!tradeable.IsCurrency)
				{
					num += tradeable.CurTotalCurrencyCostForSource;
				}
			}
			CurrencyTradeable.ForceToSource(-CurrencyTradeable.CostToInt(num));
		}

		public bool TryExecute(out bool actuallyTraded)
		{
			if (TradeSession.giftMode)
			{
				UpdateCurrencyCount();
				LimitCurrencyCountToFunds();
				int goodwillChange = FactionGiftUtility.GetGoodwillChange(tradeables, TradeSession.trader.Faction);
				FactionGiftUtility.GiveGift(tradeables, TradeSession.trader.Faction, TradeSession.playerNegotiator);
				actuallyTraded = ((float)goodwillChange > 0f);
				return true;
			}
			if (CurrencyTradeable == null || CurrencyTradeable.CountPostDealFor(Transactor.Colony) < 0)
			{
				Find.WindowStack.WindowOfType<Dialog_Trade>().FlashSilver();
				Messages.Message("MessageColonyCannotAfford".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				actuallyTraded = false;
				return false;
			}
			UpdateCurrencyCount();
			LimitCurrencyCountToFunds();
			actuallyTraded = false;
			float num = 0f;
			foreach (Tradeable tradeable in tradeables)
			{
				if (tradeable.ActionToDo != 0)
				{
					actuallyTraded = true;
				}
				if (tradeable.ActionToDo == TradeAction.PlayerSells)
				{
					num += tradeable.CurTotalCurrencyCostForDestination;
				}
				tradeable.ResolveTrade();
			}
			Reset();
			if (TradeSession.trader.Faction != null)
			{
				TradeSession.trader.Faction.Notify_PlayerTraded(num, TradeSession.playerNegotiator);
			}
			Pawn pawn = TradeSession.trader as Pawn;
			if (pawn != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.TradedWith, TradeSession.playerNegotiator, pawn);
			}
			if (actuallyTraded)
			{
				TradeSession.playerNegotiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.Inspired_Trade);
			}
			return true;
		}

		public bool DoesTraderHaveEnoughSilver()
		{
			if (TradeSession.giftMode)
			{
				return true;
			}
			if (CurrencyTradeable == null)
			{
				return false;
			}
			return CurrencyTradeable.CountPostDealFor(Transactor.Trader) >= 0;
		}

		private void LimitCurrencyCountToFunds()
		{
			if (CurrencyTradeable != null)
			{
				if (CurrencyTradeable.CountToTransferToSource > CurrencyTradeable.CountHeldBy(Transactor.Trader))
				{
					CurrencyTradeable.ForceToSource(CurrencyTradeable.CountHeldBy(Transactor.Trader));
				}
				if (CurrencyTradeable.CountToTransferToDestination > CurrencyTradeable.CountHeldBy(Transactor.Colony))
				{
					CurrencyTradeable.ForceToDestination(CurrencyTradeable.CountHeldBy(Transactor.Colony));
				}
			}
		}
	}
}
