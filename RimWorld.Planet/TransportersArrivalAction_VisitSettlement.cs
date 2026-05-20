using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class TransportersArrivalAction_VisitSettlement : TransportersArrivalAction_FormCaravan
{
	protected Settlement settlement;

	public TransportersArrivalAction_VisitSettlement()
	{
	}

	public TransportersArrivalAction_VisitSettlement(Settlement settlement, string arrivalMessageKey)
		: base(arrivalMessageKey)
	{
		this.settlement = settlement;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref settlement, "settlement");
	}

	public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(pods, destinationTile);
		if (!floatMenuAcceptanceReport)
		{
			return floatMenuAcceptanceReport;
		}
		if (settlement != null && settlement.Tile != destinationTile)
		{
			return false;
		}
		return CanVisit(pods, settlement);
	}

	public static FloatMenuAcceptanceReport CanVisit(IEnumerable<IThingHolder> pods, Settlement settlement)
	{
		if (settlement == null || !settlement.Spawned || !settlement.Visitable)
		{
			return false;
		}
		if (!TransportersArrivalActionUtility.AnyPotentialCaravanOwner(pods, Faction.OfPlayer))
		{
			return false;
		}
		return true;
	}

	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Action<PlanetTile, TransportersArrivalAction> launchAction, IEnumerable<IThingHolder> pods, Settlement settlement, bool isShuttle)
	{
		return TransportersArrivalActionUtility.GetFloatMenuOptions(() => CanVisit(pods, settlement), () => new TransportersArrivalAction_VisitSettlement(settlement, isShuttle ? "MessageShuttleArrived" : "MessageTransportPodsArrived"), "VisitSettlement".Translate(settlement.Label), launchAction, settlement.Tile);
	}
}
