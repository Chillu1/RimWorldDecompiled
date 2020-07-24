using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_TraderTracker : IExposable
	{
		private Pawn pawn;

		public TraderKindDef traderKind;

		private List<Pawn> soldPrisoners = new List<Pawn>();

		public IEnumerable<Thing> Goods
		{
			get
			{
				Lord lord = pawn.GetLord();
				if (lord == null || !(lord.LordJob is LordJob_TradeWithColony))
				{
					for (int j = 0; j < pawn.inventory.innerContainer.Count; j++)
					{
						Thing thing = pawn.inventory.innerContainer[j];
						if (!pawn.inventory.NotForSale(thing))
						{
							yield return thing;
						}
					}
				}
				if (lord == null)
				{
					yield break;
				}
				for (int j = 0; j < lord.ownedPawns.Count; j++)
				{
					Pawn p = lord.ownedPawns[j];
					switch (p.GetTraderCaravanRole())
					{
					case TraderCaravanRole.Carrier:
					{
						for (int k = 0; k < p.inventory.innerContainer.Count; k++)
						{
							yield return p.inventory.innerContainer[k];
						}
						break;
					}
					case TraderCaravanRole.Chattel:
						if (!soldPrisoners.Contains(p))
						{
							yield return p;
						}
						break;
					}
				}
			}
		}

		public int RandomPriceFactorSeed => Gen.HashCombineInt(pawn.thingIDNumber, 1149275593);

		public string TraderName => pawn.LabelShort;

		public bool CanTradeNow
		{
			get
			{
				if (!pawn.Dead && pawn.Spawned && pawn.mindState.wantsToTradeWithColony && pawn.CanCasuallyInteractNow() && !pawn.Downed && !pawn.IsPrisoner && pawn.Faction != Faction.OfPlayer && (pawn.Faction == null || !pawn.Faction.HostileTo(Faction.OfPlayer)))
				{
					if (!Goods.Any((Thing x) => traderKind.WillTrade(x.def)))
					{
						return traderKind.tradeCurrency == TradeCurrency.Favor;
					}
					return true;
				}
				return false;
			}
		}

		public Pawn_TraderTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref traderKind, "traderKind");
			Scribe_Collections.Look(ref soldPrisoners, "soldPrisoners", LookMode.Reference);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				soldPrisoners.RemoveAll((Pawn x) => x == null);
			}
		}

		public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
		{
			IEnumerable<Thing> enumerable = pawn.Map.listerThings.AllThings.Where((Thing x) => x.def.category == ThingCategory.Item && TradeUtility.PlayerSellableNow(x, pawn) && !x.Position.Fogged(x.Map) && (pawn.Map.areaManager.Home[x.Position] || x.IsInAnyStorage()) && ReachableForTrade(x));
			foreach (Thing item in enumerable)
			{
				yield return item;
			}
			if (pawn.GetLord() == null)
			{
				yield break;
			}
			foreach (Pawn item2 in from x in TradeUtility.AllSellableColonyPawns(pawn.Map)
				where !x.Downed && ReachableForTrade(x)
				select x)
			{
				yield return item2;
			}
		}

		public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			if (Goods.Contains(toGive))
			{
				Log.Error(string.Concat("Tried to add ", toGive, " to stock (pawn's trader tracker), but it's already here."));
				return;
			}
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				pawn.PreTraded(TradeAction.PlayerSells, playerNegotiator, this.pawn);
				AddPawnToStock(pawn);
				return;
			}
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this.pawn);
			Thing thing2 = TradeUtility.ThingFromStockToMergeWith(this.pawn, thing);
			if (thing2 != null)
			{
				if (!thing2.TryAbsorbStack(thing, respectStackLimit: false))
				{
					thing.Destroy();
				}
			}
			else
			{
				AddThingToRandomInventory(thing);
			}
		}

		public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
		{
			Pawn pawn = toGive as Pawn;
			if (pawn != null)
			{
				pawn.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this.pawn);
				pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Undefined);
				if (soldPrisoners.Contains(pawn))
				{
					soldPrisoners.Remove(pawn);
				}
				return;
			}
			IntVec3 positionHeld = toGive.PositionHeld;
			Map mapHeld = toGive.MapHeld;
			Thing thing = toGive.SplitOff(countToGive);
			thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this.pawn);
			if (GenPlace.TryPlaceThing(thing, positionHeld, mapHeld, ThingPlaceMode.Near))
			{
				this.pawn.GetLord()?.extraForbiddenThings.Add(thing);
				return;
			}
			Log.Error(string.Concat("Could not place bought thing ", thing, " at ", positionHeld));
			thing.Destroy();
		}

		private void AddPawnToStock(Pawn newPawn)
		{
			if (!newPawn.Spawned)
			{
				GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
			}
			if (newPawn.Faction != pawn.Faction)
			{
				newPawn.SetFaction(pawn.Faction);
			}
			if (newPawn.RaceProps.Humanlike)
			{
				newPawn.kindDef = PawnKindDefOf.Slave;
			}
			Lord lord = pawn.GetLord();
			if (lord == null)
			{
				newPawn.Destroy();
				Log.Error(string.Concat("Tried to sell pawn ", newPawn, " to ", pawn, ", but ", pawn, " has no lord. Traders without lord can't buy pawns."));
			}
			else
			{
				if (newPawn.RaceProps.Humanlike)
				{
					soldPrisoners.Add(newPawn);
				}
				lord.AddPawn(newPawn);
			}
		}

		private void AddThingToRandomInventory(Thing thing)
		{
			Lord lord = pawn.GetLord();
			IEnumerable<Pawn> source = Enumerable.Empty<Pawn>();
			if (lord != null)
			{
				source = lord.ownedPawns.Where((Pawn x) => x.GetTraderCaravanRole() == TraderCaravanRole.Carrier);
			}
			if (source.Any())
			{
				if (!source.RandomElement().inventory.innerContainer.TryAdd(thing))
				{
					thing.Destroy();
				}
			}
			else if (!pawn.inventory.innerContainer.TryAdd(thing))
			{
				thing.Destroy();
			}
		}

		private bool ReachableForTrade(Thing thing)
		{
			if (pawn.Map != thing.Map)
			{
				return false;
			}
			return pawn.Map.reachability.CanReach(pawn.Position, thing, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
		}
	}
}
