using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.QuestGen;
using Verse;

namespace RimWorld.Planet;

public static class TransportersArrivalActionUtility
{
	public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, Action<PlanetTile, TransportersArrivalAction> launchAction, PlanetTile destinationTile, Action<Action> uiConfirmationCallback = null) where T : TransportersArrivalAction
	{
		FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
		if (!floatMenuAcceptanceReport.Accepted && floatMenuAcceptanceReport.FailReason.NullOrEmpty() && floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
		{
			yield break;
		}
		if (!floatMenuAcceptanceReport.Accepted && !floatMenuAcceptanceReport.FailReason.NullOrEmpty())
		{
			label = label + " (" + floatMenuAcceptanceReport.FailReason + ")";
		}
		yield return new FloatMenuOption(label, delegate
		{
			FloatMenuAcceptanceReport floatMenuAcceptanceReport2 = acceptanceReportGetter();
			if (floatMenuAcceptanceReport2.Accepted)
			{
				if (uiConfirmationCallback == null)
				{
					launchAction(destinationTile, arrivalActionGetter());
				}
				else
				{
					uiConfirmationCallback(delegate
					{
						launchAction(destinationTile, arrivalActionGetter());
					});
				}
			}
			else if (!floatMenuAcceptanceReport2.FailMessage.NullOrEmpty())
			{
				Messages.Message(floatMenuAcceptanceReport2.FailMessage, new GlobalTargetInfo(destinationTile), MessageTypeDefOf.RejectInput, historical: false);
			}
		});
	}

	public static bool AnyNonDownedColonist(IEnumerable<IThingHolder> pods)
	{
		if (CaravanShuttleUtility.IsCaravanShuttle(pods.First() as CompTransporter))
		{
			return true;
		}
		foreach (IThingHolder pod in pods)
		{
			ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
			for (int i = 0; i < directlyHeldThings.Count; i++)
			{
				if (directlyHeldThings[i] is Pawn { IsColonist: not false, Downed: false })
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool AnyPotentialCaravanOwner(IEnumerable<IThingHolder> pods, Faction faction)
	{
		foreach (IThingHolder pod in pods)
		{
			if (pod is CompTransporter transporter && CaravanShuttleUtility.IsCaravanShuttle(transporter))
			{
				return true;
			}
			ThingOwner directlyHeldThings = pod.GetDirectlyHeldThings();
			for (int i = 0; i < directlyHeldThings.Count; i++)
			{
				if (directlyHeldThings[i] is Pawn pawn && CaravanUtility.IsOwner(pawn, faction))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Thing GetLookTarget(List<ActiveTransporterInfo> pods)
	{
		for (int i = 0; i < pods.Count; i++)
		{
			ThingOwner directlyHeldThings = pods[i].GetDirectlyHeldThings();
			for (int j = 0; j < directlyHeldThings.Count; j++)
			{
				if (directlyHeldThings[j] is Pawn { IsColonist: not false } pawn)
				{
					return pawn;
				}
			}
		}
		for (int k = 0; k < pods.Count; k++)
		{
			Thing thing = pods[k].GetDirectlyHeldThings().FirstOrDefault();
			if (thing != null)
			{
				return thing;
			}
		}
		return null;
	}

	public static void DropTravellingDropPods(List<ActiveTransporterInfo> transporters, IntVec3 near, Map map)
	{
		RemovePawnsFromWorldPawns(transporters);
		for (int i = 0; i < transporters.Count; i++)
		{
			DropCellFinder.TryFindDropSpotNear(near, map, out var result, allowFogged: false, canRoofPunch: true);
			DropPodUtility.MakeDropPodAt(result, map, transporters[i]);
		}
	}

	public static Thing DropShuttle(ActiveTransporterInfo transporter, Map map, IntVec3 near, Rot4? rotation = null, Faction faction = null)
	{
		RemovePawnsFromWorldPawns(Gen.YieldSingle(transporter));
		Thing thing = transporter.RemoveShuttle();
		if (thing == null)
		{
			thing = QuestGen_Shuttle.GenerateShuttle(faction, null, null, acceptColonists: false, onlyAcceptColonists: false, onlyAcceptHealthy: false, 0, dropEverythingIfUnsatisfied: false, leaveImmediatelyWhenSatisfied: false, dropEverythingOnArrival: true);
		}
		TransportShipDef shipDef = thing.TryGetComp<CompShuttle>()?.Props.shipDef ?? TransportShipDefOf.Ship_Shuttle;
		Rot4 valueOrDefault = rotation.GetValueOrDefault();
		if (!rotation.HasValue)
		{
			valueOrDefault = shipDef.shipThing.defaultPlacingRot;
			rotation = valueOrDefault;
		}
		thing.Rotation = rotation.Value;
		IntVec3 result;
		if ((bool)RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(near, map, shipDef.shipThing, rotation.Value))
		{
			result = near;
		}
		else if (!CellFinder.TryFindRandomCellNear(near, map, 20, (IntVec3 c) => RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(c, map, shipDef.shipThing, rotation.Value), out result) && !CellFinder.TryFindRandomCell(map, (IntVec3 c) => RoyalTitlePermitWorker_CallShuttle.ShuttleCanLandHere(c, map, shipDef.shipThing, rotation.Value), out result))
		{
			Log.Warning("Could not find a suitable cell for shuttle landing.");
			result = near;
		}
		CompTransporter compTransporter = thing.TryGetComp<CompTransporter>();
		compTransporter.innerContainer.TryAddRangeOrTransfer(transporter.innerContainer);
		TransportShip transportShip = compTransporter.Shuttle.shipParent;
		if (transportShip == null)
		{
			transportShip = TransportShipMaker.MakeTransportShip(shipDef, null, thing);
		}
		if (!result.IsValid)
		{
			result = DropCellFinder.GetBestShuttleLandingSpot(map, Faction.OfPlayer);
		}
		transportShip.ArriveAt(result, map.Parent);
		TransportShipDef shipDef2 = thing.TryGetComp<CompShuttle>().Props.shipDef;
		if (shipDef2 == null || !shipDef2.playerShuttle)
		{
			transportShip.AddJobs(ShipJobDefOf.Unload, ShipJobDefOf.FlyAway);
		}
		else
		{
			ShipJob_Unload shipJob_Unload = (ShipJob_Unload)ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload);
			shipJob_Unload.dropMode = TransportShipDropMode.PawnsOnly;
			transportShip.AddJob(shipJob_Unload);
		}
		return thing;
	}

	public static void RemovePawnsFromWorldPawns(IEnumerable<ActiveTransporterInfo> transporters)
	{
		foreach (ActiveTransporterInfo transporter in transporters)
		{
			ThingOwner innerContainer = transporter.innerContainer;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (innerContainer[i] is Pawn p && p.IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(p);
				}
			}
		}
	}
}
