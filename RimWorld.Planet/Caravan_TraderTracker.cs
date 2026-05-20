using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet;

public class Caravan_TraderTracker : IExposable
{
	private Caravan caravan;

	private List<Pawn> soldPrisoners = new List<Pawn>();

	public TraderKindDef TraderKind
	{
		get
		{
			List<Pawn> pawnsListForReading = caravan.PawnsListForReading;
			for (int i = 0; i < pawnsListForReading.Count; i++)
			{
				Pawn pawn = pawnsListForReading[i];
				if (caravan.IsOwner(pawn) && pawn.TraderKind != null)
				{
					return pawn.TraderKind;
				}
			}
			return null;
		}
	}

	public IEnumerable<Thing> Goods
	{
		get
		{
			List<Thing> inv = CaravanInventoryUtility.AllInventoryItems(caravan);
			for (int i = 0; i < inv.Count; i++)
			{
				yield return inv[i];
			}
			List<Pawn> pawns = caravan.PawnsListForReading;
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (!caravan.IsOwner(pawn) && (!pawn.RaceProps.packAnimal || pawn.inventory == null || pawn.inventory.innerContainer.Count <= 0) && !soldPrisoners.Contains(pawn))
				{
					yield return pawn;
				}
			}
		}
	}

	public int RandomPriceFactorSeed => Gen.HashCombineInt(caravan.ID, 1048142365);

	public string TraderName => caravan.LabelCap;

	public bool CanTradeNow
	{
		get
		{
			if (TraderKind != null && !caravan.AllOwnersDowned && caravan.Faction != Faction.OfPlayer)
			{
				return Goods.Any((Thing x) => TraderKind.WillTrade(x.def));
			}
			return false;
		}
	}

	public Caravan_TraderTracker(Caravan caravan)
	{
		this.caravan = caravan;
	}

	public void ExposeData()
	{
		Scribe_Collections.Look(ref soldPrisoners, "soldPrisoners", LookMode.Reference);
		if (Scribe.mode == LoadSaveMode.PostLoadInit)
		{
			soldPrisoners.RemoveAll((Pawn x) => x == null);
		}
	}

	public IEnumerable<Thing> ColonyThingsWillingToBuy(Pawn playerNegotiator)
	{
		Caravan playerCaravan = playerNegotiator.GetCaravan();
		foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(playerCaravan))
		{
			yield return item;
		}
		List<Pawn> pawns = playerCaravan.PawnsListForReading;
		for (int i = 0; i < pawns.Count; i++)
		{
			if (!playerCaravan.IsOwner(pawns[i]))
			{
				yield return pawns[i];
			}
		}
	}

	public void GiveSoldThingToTrader(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		if (Goods.Contains(toGive))
		{
			Log.Error("Tried to add " + toGive?.ToString() + " to stock (pawn's trader tracker), but it's already here.");
			return;
		}
		Caravan caravan = playerNegotiator.GetCaravan();
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerSells, playerNegotiator, this.caravan);
		if (thing is Pawn pawn)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, caravan.PawnsListForReading);
			this.caravan.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: false);
			if (pawn.IsWorldPawn() && !this.caravan.Spawned)
			{
				Find.WorldPawns.RemovePawn(pawn);
			}
			if (pawn.RaceProps.Humanlike)
			{
				soldPrisoners.Add(pawn);
			}
		}
		else
		{
			Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, this.caravan.PawnsListForReading, null);
			if (pawn2 == null)
			{
				Log.Error("Could not find pawn to move sold thing to (sold by player). thing=" + thing);
				thing.Destroy();
			}
			else if (!pawn2.inventory.innerContainer.TryAdd(thing))
			{
				Log.Error("Could not add item to inventory.");
				thing.Destroy();
			}
		}
	}

	public void GiveSoldThingToPlayer(Thing toGive, int countToGive, Pawn playerNegotiator)
	{
		Caravan caravan = playerNegotiator.GetCaravan();
		Thing thing = toGive.SplitOff(countToGive);
		thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, this.caravan);
		if (thing is Pawn pawn)
		{
			CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(pawn, this.caravan.PawnsListForReading);
			caravan.AddPawn(pawn, addCarriedPawnToWorldPawnsIfAny: true);
			if (!pawn.IsWorldPawn() && caravan.Spawned)
			{
				Find.WorldPawns.PassToWorld(pawn);
			}
			soldPrisoners.Remove(pawn);
			return;
		}
		Pawn pawn2 = CaravanInventoryUtility.FindPawnToMoveInventoryTo(thing, caravan.PawnsListForReading, null);
		if (pawn2 == null)
		{
			Log.Error("Could not find pawn to move bought thing to (bought by player). thing=" + thing);
			thing.Destroy();
		}
		else if (!pawn2.inventory.innerContainer.TryAdd(thing))
		{
			Log.Error("Could not add item to inventory.");
			thing.Destroy();
		}
	}
}
