using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Tradeable : Transferable
	{
		public List<Thing> thingsColony = new List<Thing>();

		public List<Thing> thingsTrader = new List<Thing>();

		private int countToTransfer;

		private float pricePlayerBuy = -1f;

		private float pricePlayerSell = -1f;

		private float priceFactorBuy_TraderPriceType;

		private float priceFactorSell_TraderPriceType;

		private float priceFactorSell_ItemSellPriceFactor;

		private float priceGain_PlayerNegotiator;

		private float priceGain_Settlement;

		public override int CountToTransfer
		{
			get
			{
				return countToTransfer;
			}
			protected set
			{
				countToTransfer = value;
				base.EditBuffer = value.ToStringCached();
			}
		}

		public Thing FirstThingColony
		{
			get
			{
				if (thingsColony.Count == 0)
				{
					return null;
				}
				return thingsColony[0];
			}
		}

		public Thing FirstThingTrader
		{
			get
			{
				if (thingsTrader.Count == 0)
				{
					return null;
				}
				return thingsTrader[0];
			}
		}

		public override string Label => AnyThing.LabelNoCount;

		public virtual float BaseMarketValue => AnyThing.MarketValue;

		public override bool Interactive
		{
			get
			{
				if (IsCurrency)
				{
					if (TradeSession.Active)
					{
						return TradeSession.giftMode;
					}
					return false;
				}
				return true;
			}
		}

		public virtual bool TraderWillTrade => TradeSession.trader.TraderKind.WillTrade(ThingDef);

		public override bool HasAnyThing
		{
			get
			{
				if (FirstThingColony == null)
				{
					return FirstThingTrader != null;
				}
				return true;
			}
		}

		public override Thing AnyThing
		{
			get
			{
				if (FirstThingColony != null)
				{
					return FirstThingColony.GetInnerIfMinified();
				}
				if (FirstThingTrader != null)
				{
					return FirstThingTrader.GetInnerIfMinified();
				}
				Log.Error(string.Concat(GetType(), " lacks AnyThing."));
				return null;
			}
		}

		public override ThingDef ThingDef
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return AnyThing.def;
			}
		}

		public ThingDef StuffDef
		{
			get
			{
				if (!HasAnyThing)
				{
					return null;
				}
				return AnyThing.Stuff;
			}
		}

		public override string TipDescription
		{
			get
			{
				if (!HasAnyThing)
				{
					return "";
				}
				return AnyThing.DescriptionDetailed;
			}
		}

		public TradeAction ActionToDo
		{
			get
			{
				if (CountToTransfer == 0)
				{
					return TradeAction.None;
				}
				if (base.CountToTransferToDestination > 0)
				{
					return TradeAction.PlayerSells;
				}
				return TradeAction.PlayerBuys;
			}
		}

		public virtual bool IsCurrency
		{
			get
			{
				if (Bugged)
				{
					return false;
				}
				return ThingDef == ThingDefOf.Silver;
			}
		}

		public virtual bool IsFavor => false;

		public override TransferablePositiveCountDirection PositiveCountDirection
		{
			get
			{
				if (TradeSession.Active && TradeSession.giftMode)
				{
					return TransferablePositiveCountDirection.Destination;
				}
				return TransferablePositiveCountDirection.Source;
			}
		}

		public float CurTotalCurrencyCostForSource
		{
			get
			{
				if (ActionToDo == TradeAction.None)
				{
					return 0f;
				}
				return (float)base.CountToTransferToSource * GetPriceFor(ActionToDo);
			}
		}

		public float CurTotalCurrencyCostForDestination
		{
			get
			{
				if (ActionToDo == TradeAction.None)
				{
					return 0f;
				}
				return (float)base.CountToTransferToDestination * GetPriceFor(ActionToDo);
			}
		}

		public virtual Window NewInfoDialog => new Dialog_InfoCard(ThingDef);

		private bool Bugged
		{
			get
			{
				if (!HasAnyThing)
				{
					Log.ErrorOnce(ToString() + " is bugged. There will be no more logs about this.", 162112);
					return true;
				}
				return false;
			}
		}

		public virtual int CostToInt(float cost)
		{
			return Mathf.RoundToInt(cost);
		}

		public Tradeable()
		{
		}

		public Tradeable(Thing thingColony, Thing thingTrader)
		{
			thingsColony.Add(thingColony);
			thingsTrader.Add(thingTrader);
		}

		public void AddThing(Thing t, Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				thingsColony.Add(t);
			}
			if (trans == Transactor.Trader)
			{
				thingsTrader.Add(t);
			}
		}

		public PriceType PriceTypeFor(TradeAction action)
		{
			return TradeSession.trader.TraderKind.PriceTypeFor(ThingDef, action);
		}

		private void InitPriceDataIfNeeded()
		{
			if (pricePlayerBuy > 0f)
			{
				return;
			}
			if (IsCurrency)
			{
				pricePlayerBuy = BaseMarketValue;
				pricePlayerSell = BaseMarketValue;
				return;
			}
			priceFactorBuy_TraderPriceType = PriceTypeFor(TradeAction.PlayerBuys).PriceMultiplier();
			priceFactorSell_TraderPriceType = PriceTypeFor(TradeAction.PlayerSells).PriceMultiplier();
			priceGain_PlayerNegotiator = TradeSession.playerNegotiator.GetStatValue(StatDefOf.TradePriceImprovement);
			priceGain_Settlement = TradeSession.trader.TradePriceImprovementOffsetForPlayer;
			priceFactorSell_ItemSellPriceFactor = AnyThing.GetStatValue(StatDefOf.SellPriceFactor);
			pricePlayerBuy = TradeUtility.GetPricePlayerBuy(AnyThing, priceFactorBuy_TraderPriceType, priceGain_PlayerNegotiator, priceGain_Settlement);
			pricePlayerSell = TradeUtility.GetPricePlayerSell(AnyThing, priceFactorSell_TraderPriceType, priceGain_PlayerNegotiator, priceGain_Settlement, TradeSession.TradeCurrency);
			if (pricePlayerSell >= pricePlayerBuy)
			{
				pricePlayerSell = pricePlayerBuy;
			}
		}

		public string GetPriceTooltip(TradeAction action)
		{
			if (!HasAnyThing)
			{
				return "";
			}
			InitPriceDataIfNeeded();
			string text = (action == TradeAction.PlayerBuys) ? "BuyPriceDesc".Translate() : "SellPriceDesc".Translate();
			if (TradeSession.TradeCurrency != 0)
			{
				return text;
			}
			text += "\n\n";
			text += StatDefOf.MarketValue.LabelCap + ": " + BaseMarketValue.ToStringMoney();
			if (action == TradeAction.PlayerBuys)
			{
				text += "\n  x " + 1.4f.ToString("F2") + " (" + "Buying".Translate() + ")";
				if (priceFactorBuy_TraderPriceType != 1f)
				{
					text += "\n  x " + priceFactorBuy_TraderPriceType.ToString("F2") + " (" + "TraderTypePrice".Translate() + ")";
				}
				if (Find.Storyteller.difficulty.tradePriceFactorLoss != 0f)
				{
					text += "\n  x " + (1f + Find.Storyteller.difficulty.tradePriceFactorLoss).ToString("F2") + " (" + "DifficultyLevel".Translate() + ")";
				}
				text += "\n";
				text += "\n" + "YourNegotiatorBonus".Translate() + ": -" + priceGain_PlayerNegotiator.ToStringPercent();
				if (priceGain_Settlement != 0f)
				{
					text += "\n" + "TradeWithFactionBaseBonus".Translate() + ": -" + priceGain_Settlement.ToStringPercent();
				}
			}
			else
			{
				text += "\n  x " + 0.6f.ToString("F2") + " (" + "Selling".Translate() + ")";
				if (priceFactorSell_TraderPriceType != 1f)
				{
					text += "\n  x " + priceFactorSell_TraderPriceType.ToString("F2") + " (" + "TraderTypePrice".Translate() + ")";
				}
				if (priceFactorSell_ItemSellPriceFactor != 1f)
				{
					text += "\n  x " + priceFactorSell_ItemSellPriceFactor.ToString("F2") + " (" + "ItemSellPriceFactor".Translate() + ")";
				}
				if (Find.Storyteller.difficulty.tradePriceFactorLoss != 0f)
				{
					text += "\n  x " + (1f - Find.Storyteller.difficulty.tradePriceFactorLoss).ToString("F2") + " (" + "DifficultyLevel".Translate() + ")";
				}
				text += "\n";
				text += "\n" + "YourNegotiatorBonus".Translate() + ": " + priceGain_PlayerNegotiator.ToStringPercent();
				if (priceGain_Settlement != 0f)
				{
					text += "\n" + "TradeWithFactionBaseBonus".Translate() + ": " + priceGain_Settlement.ToStringPercent();
				}
			}
			text += "\n\n";
			float priceFor = GetPriceFor(action);
			text += "FinalPrice".Translate() + ": " + priceFor.ToStringMoney();
			if ((action == TradeAction.PlayerBuys && priceFor <= 0.5f) || (action == TradeAction.PlayerBuys && priceFor <= 0.01f))
			{
				text += " (" + "minimum".Translate() + ")";
			}
			return text;
		}

		public virtual float GetPriceFor(TradeAction action)
		{
			InitPriceDataIfNeeded();
			if (action == TradeAction.PlayerBuys)
			{
				return pricePlayerBuy;
			}
			return pricePlayerSell;
		}

		public override int GetMinimumToTransfer()
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Destination)
			{
				return -CountHeldBy(Transactor.Trader);
			}
			return -CountHeldBy(Transactor.Colony);
		}

		public override int GetMaximumToTransfer()
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Destination)
			{
				return CountHeldBy(Transactor.Colony);
			}
			return CountHeldBy(Transactor.Trader);
		}

		public override AcceptanceReport UnderflowReport()
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Destination)
			{
				return new AcceptanceReport("TraderHasNoMore".Translate());
			}
			return new AcceptanceReport("ColonyHasNoMore".Translate());
		}

		public override AcceptanceReport OverflowReport()
		{
			if (PositiveCountDirection == TransferablePositiveCountDirection.Destination)
			{
				return new AcceptanceReport("ColonyHasNoMore".Translate());
			}
			return new AcceptanceReport("TraderHasNoMore".Translate());
		}

		private List<Thing> TransactorThings(Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				return thingsColony;
			}
			return thingsTrader;
		}

		public virtual int CountHeldBy(Transactor trans)
		{
			List<Thing> list = TransactorThings(trans);
			int num = 0;
			for (int i = 0; i < list.Count; i++)
			{
				num += list[i].stackCount;
			}
			return num;
		}

		public int CountPostDealFor(Transactor trans)
		{
			if (trans == Transactor.Colony)
			{
				return CountHeldBy(trans) + base.CountToTransferToSource;
			}
			return CountHeldBy(trans) + base.CountToTransferToDestination;
		}

		public virtual void ResolveTrade()
		{
			if (ActionToDo == TradeAction.PlayerSells)
			{
				TransferableUtility.TransferNoSplit(thingsColony, base.CountToTransferToDestination, delegate(Thing thing, int countToTransfer)
				{
					TradeSession.trader.GiveSoldThingToTrader(thing, countToTransfer, TradeSession.playerNegotiator);
				});
			}
			else if (ActionToDo == TradeAction.PlayerBuys)
			{
				TransferableUtility.TransferNoSplit(thingsTrader, base.CountToTransferToSource, delegate(Thing thing, int countToTransfer)
				{
					CheckTeachOpportunity(thing, countToTransfer);
					TradeSession.trader.GiveSoldThingToPlayer(thing, countToTransfer, TradeSession.playerNegotiator);
				});
			}
		}

		private void CheckTeachOpportunity(Thing boughtThing, int boughtCount)
		{
			Building building = boughtThing as Building;
			if (building == null)
			{
				MinifiedThing minifiedThing = boughtThing as MinifiedThing;
				if (minifiedThing != null)
				{
					building = (minifiedThing.InnerThing as Building);
				}
			}
			if (building != null && building.def.building != null && building.def.building.boughtConceptLearnOpportunity != null)
			{
				LessonAutoActivator.TeachOpportunity(building.def.building.boughtConceptLearnOpportunity, OpportunityType.GoodToKnow);
			}
		}

		public override string ToString()
		{
			return string.Concat(GetType(), "(", ThingDef, ", countToTransfer=", CountToTransfer, ")");
		}

		public override int GetHashCode()
		{
			return AnyThing.GetHashCode();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				thingsColony.RemoveAll((Thing x) => x.Destroyed);
				thingsTrader.RemoveAll((Thing x) => x.Destroyed);
			}
			Scribe_Values.Look(ref countToTransfer, "countToTransfer", 0);
			Scribe_Collections.Look(ref thingsColony, "thingsColony", LookMode.Reference);
			Scribe_Collections.Look(ref thingsTrader, "thingsTrader", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				base.EditBuffer = countToTransfer.ToStringCached();
			}
			if (Scribe.mode == LoadSaveMode.PostLoadInit && (thingsColony.RemoveAll((Thing x) => x == null) != 0 || thingsTrader.RemoveAll((Thing x) => x == null) != 0))
			{
				Log.Warning("Some of the things were null after loading.");
			}
		}
	}
}
