using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class Settlement_TraderTracker : IThingHolder, IExposable
	{
		public Settlement settlement;

		private ThingOwner<Thing> stock;

		private int lastStockGenerationTicks = -1;

		private bool everGeneratedStock;

		private const float DefaultTradePriceImprovement = 0.02f;

		private List<Pawn> tmpSavedPawns = new List<Pawn>();

		protected virtual int RegenerateStockEveryDays => 30;

		public IThingHolder ParentHolder => settlement;

		public List<Thing> StockListForReading
		{
			get
			{
				if (stock == null)
				{
					RegenerateStock();
				}
				return stock.InnerListForReading;
			}
		}

		public TraderKindDef TraderKind
		{
			get
			{
				List<TraderKindDef> baseTraderKinds = settlement.Faction.def.baseTraderKinds;
				if (baseTraderKinds.NullOrEmpty())
				{
					return null;
				}
				int index = Mathf.Abs(settlement.HashOffset()) % baseTraderKinds.Count;
				return baseTraderKinds[index];
			}
		}

		public int RandomPriceFactorSeed => Gen.HashCombineInt(settlement.ID, 1933327354);

		public bool EverVisited => everGeneratedStock;

		public bool RestockedSinceLastVisit
		{
			get
			{
				if (everGeneratedStock)
				{
					return stock == null;
				}
				return false;
			}
		}

		public int NextRestockTick
		{
			get
			{
				if (stock == null || !everGeneratedStock)
				{
					return -1;
				}
				return ((lastStockGenerationTicks != -1) ? lastStockGenerationTicks : 0) + RegenerateStockEveryDays * 60000;
			}
		}

		public virtual string TraderName
		{
			get
			{
				if (settlement.Faction == null)
				{
					return settlement.LabelCap;
				}
				return "SettlementTrader".Translate(settlement.LabelCap, settlement.Faction.Name);
			}
		}

		public virtual bool CanTradeNow
		{
			get
			{
				if (TraderKind != null)
				{
					if (stock != null)
					{
						return stock.InnerListForReading.Any((Thing x) => TraderKind.WillTrade(x.def));
					}
					return true;
				}
				return false;
			}
		}

		public virtual float TradePriceImprovementOffsetForPlayer => 0.02f;

		public Settlement_TraderTracker(Settlement settlement)
		{
			this.settlement = settlement;
		}

		public virtual void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				tmpSavedPawns.Clear();
				if (stock != null)
				{
					for (int num = stock.Count - 1; num >= 0; num--)
					{
						Pawn pawn = stock[num] as Pawn;
						if (pawn != null)
						{
							stock.Remove(pawn);
							tmpSavedPawns.Add(pawn);
						}
					}
				}
			}
			Scribe_Collections.Look(ref tmpSavedPawns, "tmpSavedPawns", LookMode.Reference);
			Scribe_Deep.Look(ref stock, "stock");
			Scribe_Values.Look(ref lastStockGenerationTicks, "lastStockGenerationTicks", 0);
			Scribe_Values.Look(ref everGeneratedStock, "wasStockGeneratedYet", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit || Scribe.mode == LoadSaveMode.Saving)
			{
				for (int i = 0; i < tmpSavedPawns.Count; i++)
				{
					stock.TryAdd(tmpSavedPawns[i], canMergeWithExistingStacks: false);
				}
				tmpSavedPawns.Clear();
			}
		}

		public virtual IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			Caravan caravan = playerNegotiator.GetCaravan();
			foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
			{
				yield return item;
			}
			List<Pawn> pawns = caravan.PawnsListForReading;
			for (int i = 0; i < pawns.Count; i++)
			{
				if (!caravan.IsOwner(pawns[i]))
				{
					yield return pawns[i];
				}
			}
		}

		public virtual void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			if (stock == null)
			{
				RegenerateStock();
			}
			Caravan caravan = playerNegotiator.GetCaravan();
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, settlement);
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
				if (!pawn.RaceProps.Humanlike && !stock.TryAdd(pawn, canMergeWithExistingStacks: false))
				{
					pawn.Destroy();
				}
			}
			else if (!stock.TryAdd(thing, canMergeWithExistingStacks: false))
			{
				thing.Destroy();
			}
		}

		public virtual void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Caravan caravan = playerNegotiator.GetCaravan();
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, settlement);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				caravan.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
				return;
			}
			Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null);
			if (pawn2 == null)
			{
				Log.Error("Could not find any pawn to give sold thing to.");
				thing.Destroy();
			}
			else if (!pawn2.inventory.innerContainer.TryAdd(thing))
			{
				Log.Error("Could not add sold thing to inventory.");
				thing.Destroy();
			}
		}

		public virtual void TraderTrackerTick()
		{
			if (stock == null)
			{
				return;
			}
			if (Find.TickManager.TicksGame - lastStockGenerationTicks > RegenerateStockEveryDays * 60000)
			{
				TryDestroyStock();
				return;
			}
			for (int num = stock.Count - 1; num >= 0; num--)
			{
				Pawn pawn = stock[num] as Pawn;
				if (pawn != null && pawn.Destroyed)
				{
					stock.Remove(pawn);
				}
			}
			for (int num2 = stock.Count - 1; num2 >= 0; num2--)
			{
				Pawn pawn2 = stock[num2] as Pawn;
				if (pawn2 != null && !pawn2.IsWorldPawn())
				{
					Log.Error("Faction base has non-world-pawns in its stock. Removing...");
					stock.Remove(pawn2);
				}
			}
		}

		public void TryDestroyStock()
		{
			if (stock == null)
			{
				return;
			}
			for (int num = stock.Count - 1; num >= 0; num--)
			{
				Thing thing = stock[num];
				stock.Remove(thing);
				if (!(thing is Pawn) && !thing.Destroyed)
				{
					thing.Destroy();
				}
			}
			stock = null;
		}

		public bool ContainsPawn(Pawn p)
		{
			if (stock != null)
			{
				return stock.Contains(p);
			}
			return false;
		}

		protected virtual void RegenerateStock()
		{
			TryDestroyStock();
			stock = new ThingOwner<Thing>(this);
			everGeneratedStock = true;
			if (settlement.Faction == null || !settlement.Faction.IsPlayer)
			{
				ThingSetMakerParams parms = default(ThingSetMakerParams);
				parms.traderDef = TraderKind;
				parms.tile = settlement.Tile;
				parms.makingFaction = settlement.Faction;
				stock.TryAddRangeOrTransfer(ThingSetMakerDefOf.TraderStock.root.Generate(parms));
			}
			for (int i = 0; i < stock.Count; i++)
			{
				Pawn pawn = stock[i] as Pawn;
				if (pawn != null)
				{
					Find.WorldPawns.PassToWorld(pawn);
				}
			}
			lastStockGenerationTicks = Find.TickManager.TicksGame;
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return stock;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}
	}
}
