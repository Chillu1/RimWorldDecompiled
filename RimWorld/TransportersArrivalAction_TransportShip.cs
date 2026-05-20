using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public class TransportersArrivalAction_TransportShip : TransportersArrivalAction
{
	public MapParent mapParent;

	public TransportShip transportShip;

	public IntVec3 cell = IntVec3.Invalid;

	public override bool GeneratesMap => true;

	public TransportersArrivalAction_TransportShip()
	{
	}

	public TransportersArrivalAction_TransportShip(MapParent mapParent, TransportShip transportShip)
	{
		this.mapParent = mapParent;
		this.transportShip = transportShip;
	}

	public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
	{
		return !mapParent.HasMap;
	}

	public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
	{
		if (transportShip == null || transportShip.Disposed)
		{
			Log.Error("Trying to arrive in a null or disposed transport ship.");
			return;
		}
		if (mapParent.Destroyed)
		{
			new TransportersArrivalAction_FormCaravan().Arrived(transporters, tile);
			return;
		}
		bool flag = !mapParent.HasMap;
		Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile, null);
		if (!cell.IsValid)
		{
			cell = DropCellFinder.GetBestShuttleLandingSpot(orGenerateMap, Faction.OfPlayer);
		}
		LookTargets lookTargets = new LookTargets(cell, orGenerateMap);
		if (!cell.IsValid)
		{
			Log.Error("Could not find cell for transport ship arrival.");
			return;
		}
		if (orGenerateMap.Parent is Settlement settlement && settlement.Faction != Faction.OfPlayer)
		{
			TaggedString letterLabel = "LetterLabelCaravanEnteredEnemyBase".Translate();
			TaggedString letterText = "LetterShuttleLandedInEnemyBase".Translate(settlement.Label).CapitalizeFirst();
			SettlementUtility.AffectRelationsOnAttacked(settlement, ref letterText);
			if (flag)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(orGenerateMap.mapPawns.AllPawns, ref letterLabel, ref letterText, "LetterRelatedPawnsInMapWherePlayerLanded".Translate(Faction.OfPlayer.def.pawnsPlural), informEvenIfSeenBefore: true);
			}
			Find.LetterStack.ReceiveLetter(letterLabel, letterText, LetterDefOf.NeutralEvent, lookTargets, settlement.Faction);
		}
		transporters[0].innerContainer.Remove(transportShip.shipThing);
		transportShip.TransporterComp.innerContainer.TryAddRangeOrTransfer(transporters[0].innerContainer, canMergeWithExistingStacks: true, destroyLeftover: true);
		transportShip.ArriveAt(cell, mapParent);
		TransportShipDef shipDef = transportShip.shipThing.TryGetComp<CompShuttle>().Props.shipDef;
		if (shipDef != null && shipDef.playerShuttle)
		{
			ShipJob_Unload shipJob_Unload = (ShipJob_Unload)ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload);
			shipJob_Unload.dropMode = TransportShipDropMode.PawnsOnly;
			transportShip.AddJob(shipJob_Unload);
		}
		Messages.Message("MessageShuttleArrived".Translate(), lookTargets, MessageTypeDefOf.TaskCompletion);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_References.Look(ref transportShip, "transportShip");
		Scribe_References.Look(ref mapParent, "mapParent");
		Scribe_Values.Look(ref cell, "cell");
	}
}
