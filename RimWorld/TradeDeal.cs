using System.Collections.Generic;
using RimWorld.Planet;
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
				if (!TradeUtility.PlayerSellableNow(item, TradeSession.trader))
				{
					continue;
				}
				if (!TradeSession.playerNegotiator.IsWorldPawn() && !InSellablePosition(item, out var reason))
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
			if (!t.Spawned && (!ModsConfig.BiotechActive || t.def != ThingDefOf.Genepack || !(t.ParentHolder is CompGenepackContainer)) && (!(t is Book) || !(t.ParentHolder is Building_Bookcase)) && !(t.ParentHolder is Building_OutfitStand))
			{
				reason = null;
				return false;
			}
			IntVec3 positionHeld = t.PositionHeld;
			Room room = t.SpawnedParentOrMe.GetRoom();
			if (positionHeld.Fogged(t.MapHeld))
			{
				reason = null;
				return false;
			}
			if (room != null)
			{
				int num = GenRadial.NumCellsInRadius(6.9f);
				for (int i = 0; i < num; i++)
				{
					IntVec3 intVec = positionHeld + GenRadial.RadialPattern[i];
					if (!intVec.InBounds(t.MapHeld) || intVec.GetRoom(t.MapHeld) != room)
					{
						continue;
					}
					List<Thing> thingList = intVec.GetThingList(t.MapHeld);
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
				actuallyTraded = (float)goodwillChange > 0f;
				return true;
			}
			if (CurrencyTradeable == null || CurrencyTradeable.CountPostDealFor(Transactor.Colony) < 0)
			{
				Find.WindowStack.WindowOfType<Dialog_Trade>().FlashSilver();
				Messages.Message("MessageColonyCannotAfford".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				actuallyTraded = false;
				return false;
			}
			if (ModsConfig.IdeologyActive)
			{
				if (tradeables.Any((Tradeable x) => x.ActionToDo != TradeAction.None && x.ThingDef != null && x.ThingDef.IsNaturalOrgan))
				{
					HistoryEvent historyEvent = new HistoryEvent(HistoryEventDefOf.TradedOrgan, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer));
					if (!historyEvent.Notify_PawnAboutToDo())
					{
						actuallyTraded = false;
						return false;
					}
					Find.HistoryEventsManager.RecordEvent(historyEvent);
				}
				if (tradeables.Any((Tradeable x) => x.ActionToDo == TradeAction.PlayerSells && x.ThingDef != null && x.ThingDef.IsNaturalOrgan))
				{
					HistoryEvent historyEvent2 = new HistoryEvent(HistoryEventDefOf.SoldOrgan, TradeSession.playerNegotiator.Named(HistoryEventArgsNames.Doer));
					if (!historyEvent2.Notify_PawnAboutToDo())
					{
						actuallyTraded = false;
						return false;
					}
					Find.HistoryEventsManager.RecordEvent(historyEvent2);
				}
			}
			UpdateCurrencyCount();
			LimitCurrencyCountToFunds();
			actuallyTraded = false;
			float num = 0f;
			foreach (Tradeable tradeable in tradeables)
			{
				if (tradeable.ActionToDo != TradeAction.None)
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
			if (TradeSession.trader is Pawn pawn)
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
