using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class CaravanShuttleUtility
{
	private static int cachedCaravanItemsHash = -1;

	private static float cachedCaravanShuttleMass = -1f;

	private static float GetCaravanShuttleMass(Caravan caravan)
	{
		int num = caravan.GetHashCode();
		foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
		{
			num = Gen.HashCombineInt(num, item.GetHashCode());
		}
		foreach (Pawn pawn in caravan.pawns)
		{
			num = Gen.HashCombineInt(num, pawn.GetHashCode());
		}
		if (num == cachedCaravanItemsHash)
		{
			return cachedCaravanShuttleMass;
		}
		cachedCaravanItemsHash = num;
		cachedCaravanShuttleMass = CollectionsMassCalculator.MassUsage(caravan.pawns, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMass: true);
		cachedCaravanShuttleMass -= caravan.Shuttle.GetStatValue(StatDefOf.Mass);
		return cachedCaravanShuttleMass;
	}

	public static float FuelInCaravan(Caravan caravan)
	{
		float num = 0f;
		foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
		{
			if (caravan.Shuttle.RefuelableComp.Props.fuelFilter.Allows(item))
			{
				num += (float)item.stackCount;
			}
		}
		return num;
	}

	public static void ConsumeFuelFromCaravanInventory(Caravan caravan, int fuelAmount)
	{
		int num = fuelAmount;
		foreach (Thing item in CaravanInventoryUtility.AllInventoryItems(caravan))
		{
			if (caravan.Shuttle.RefuelableComp.Props.fuelFilter.Allows(item))
			{
				Thing thing = item.SplitOff(Mathf.Min(num, item.stackCount));
				num -= thing.stackCount;
			}
			if (num <= 0)
			{
				break;
			}
		}
	}

	public static AcceptanceReport CanLaunchCaravanShuttle(Caravan caravan)
	{
		float caravanShuttleMass = GetCaravanShuttleMass(caravan);
		if (caravanShuttleMass > caravan.Shuttle.TransporterComp.MassCapacity)
		{
			return "CommandLaunchGroupFailOverMassCapacity".Translate() + ": " + "MassUsageString".Translate(caravanShuttleMass.ToString("F0"), caravan.Shuttle.TransporterComp.MassCapacity.ToString("F0"));
		}
		float fuelLevel = caravan.Shuttle.FuelLevel;
		return caravan.Shuttle.LaunchableComp.CanLaunch(fuelLevel);
	}

	public static bool IsCaravanShuttle(CompTransporter transporter)
	{
		if (transporter?.Shuttle == null)
		{
			return false;
		}
		return transporter.parent.IsInCaravan();
	}

	public static void LaunchShuttle(Caravan caravan, PlanetTile destinationTile, TransportersArrivalAction arrivalAction)
	{
		Find.WorldTargeter.StopTargeting();
		Building_PassengerShuttle shuttle = caravan.Shuttle;
		float fuelLevel = shuttle.FuelLevel;
		if (!shuttle.LaunchableComp.CanLaunch(fuelLevel))
		{
			return;
		}
		int num = Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, destinationTile, passImpassable: true, int.MaxValue, canTraverseLayers: true);
		if (num <= shuttle.LaunchableComp.MaxLaunchDistanceAtFuelLevel(fuelLevel, destinationTile.Layer))
		{
			float amount = Mathf.Max(shuttle.LaunchableComp.FuelNeededToLaunchAtDist(num, destinationTile.Layer), 1f);
			shuttle.RefuelableComp.ConsumeFuel(amount);
			CaravanInventoryUtility.GetOwnerOf(caravan, shuttle).inventory.innerContainer.Remove(shuttle);
			ActiveTransporterInfo activeTransporterInfo = new ActiveTransporterInfo();
			activeTransporterInfo.sentTransporterDef = shuttle.TransporterComp.parent.def;
			LoadCaravanItemsIntoContainer(caravan.pawns, activeTransporterInfo.innerContainer);
			activeTransporterInfo.innerContainer.TryAddRangeOrTransfer(caravan.pawns);
			activeTransporterInfo.SetShuttle(shuttle);
			TravellingTransporters travellingTransporters = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(shuttle.LaunchableComp.Props.worldObjectDef);
			travellingTransporters.SetFaction(Faction.OfPlayer);
			travellingTransporters.destinationTile = destinationTile;
			travellingTransporters.arrivalAction = arrivalAction;
			PlanetTile planetTile = caravan.Tile;
			if (planetTile.Layer != destinationTile.Layer)
			{
				planetTile = destinationTile.Layer.GetClosestTile_NewTemp(planetTile);
			}
			travellingTransporters.Tile = planetTile;
			travellingTransporters.AddTransporter(activeTransporterInfo, justLeftTheMap: true);
			Find.WorldObjects.Add(travellingTransporters);
			shuttle.LaunchableComp.lastLaunchTick = Find.TickManager.TicksGame;
			if (shuttle is INotifyLaunchableLaunch notifyLaunchableLaunch)
			{
				notifyLaunchableLaunch.Notify_LaunchableLaunched(shuttle.LaunchableComp);
			}
			caravan.Destroy();
			CameraJumper.TryJump(shuttle);
		}
	}

	public static void LoadCaravanItemsIntoContainer(IEnumerable<Pawn> caravanPawns, ThingOwner container)
	{
		foreach (Pawn caravanPawn in caravanPawns)
		{
			int num = 1000;
			while (caravanPawn.inventory?.FirstUnloadableThing.Thing != null && num-- > 0)
			{
				ThingCount firstUnloadableThing = caravanPawn.inventory.FirstUnloadableThing;
				caravanPawn.inventory.innerContainer.TryTransferToContainer(firstUnloadableThing.Thing, container, firstUnloadableThing.Count);
			}
		}
	}
}
