using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

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
				for (int i = 0; i < pawn.inventory.innerContainer.Count; i++)
				{
					Thing thing = pawn.inventory.innerContainer[i];
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
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn p = lord.ownedPawns[i];
				switch (p.GetTraderCaravanRole())
				{
				case TraderCaravanRole.Carrier:
				{
					for (int j = 0; j < p.inventory.innerContainer.Count; j++)
					{
						yield return p.inventory.innerContainer[j];
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
		if (ModsConfig.BiotechActive)
		{
			List<Building> list = pawn.Map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.GeneBank);
			foreach (Building item2 in list)
			{
				if (!ReachableForTrade(item2))
				{
					continue;
				}
				CompGenepackContainer compGenepackContainer = item2.TryGetComp<CompGenepackContainer>();
				if (compGenepackContainer == null)
				{
					continue;
				}
				List<Genepack> containedGenepacks = compGenepackContainer.ContainedGenepacks;
				foreach (Genepack item3 in containedGenepacks)
				{
					yield return item3;
				}
			}
		}
		IEnumerable<IHaulSource> enumerable2 = pawn.Map.listerBuildings.AllColonistBuildingsOfType<IHaulSource>();
		foreach (IHaulSource item4 in enumerable2)
		{
			Building thing = (Building)item4;
			if (!ReachableForTrade(thing))
			{
				continue;
			}
			foreach (Thing item5 in (IEnumerable<Thing>)item4.GetDirectlyHeldThings())
			{
				yield return item5;
			}
		}
		if (pawn.GetLord() == null)
		{
			yield break;
		}
		foreach (Pawn item6 in from x in TradeUtility.AllSellableColonyPawns(pawn.Map)
			where !x.Downed && ReachableForTrade(x)
			select x)
		{
			yield return item6;
		}
	}

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		if (Goods.Contains(toGive))
		{
			Log.Error("Tried to add " + toGive?.ToString() + " to stock (pawn's trader tracker), but it's already here.");
			return;
		}
		if (toGive is Pawn pawn)
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
		if (toGive is Pawn pawn)
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
		string obj = thing?.ToString();
		IntVec3 intVec = positionHeld;
		Log.Error("Could not place bought thing " + obj + " at " + intVec.ToString());
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
			Log.Error("Tried to sell pawn " + newPawn?.ToString() + " to " + pawn?.ToString() + ", but " + pawn?.ToString() + " has no lord. Traders without lord can't buy pawns.");
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
		Thing thing2 = thing;
		if (HaulAIUtility.IsInHaulableInventory(thing))
		{
			thing2 = thing.SpawnedParentOrMe;
		}
		if (pawn.Map != thing2.MapHeld)
		{
			return false;
		}
		return pawn.Map.reachability.CanReach(pawn.Position, thing2, PathEndMode.Touch, TraverseMode.PassDoors, Danger.Some);
	}
}
