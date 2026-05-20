using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public static class QuestGen_TransportShip
{
	public static QuestPart_SetupTransportShip GenerateTransportShip(this Quest quest, TransportShipDef def, IEnumerable<Thing> contents, Thing shipThing, string inSignal = null)
	{
		QuestPart_SetupTransportShip questPart_SetupTransportShip = new QuestPart_SetupTransportShip
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			transportShip = TransportShipMaker.MakeTransportShip(def, null, shipThing),
			items = contents?.Where((Thing c) => !(c is Pawn)).ToList(),
			pawns = contents?.OfType<Pawn>().ToList()
		};
		quest.AddPart(questPart_SetupTransportShip);
		return questPart_SetupTransportShip;
	}

	public static QuestPart_SendTransportShipAwayOnCleanup SendTransportShipAwayOnCleanup(this Quest quest, TransportShip transportShip, bool unloadContents = false, TransportShipDropMode unsatisfiedDropMode = TransportShipDropMode.NonRequired)
	{
		QuestPart_SendTransportShipAwayOnCleanup questPart_SendTransportShipAwayOnCleanup = new QuestPart_SendTransportShipAwayOnCleanup
		{
			transportShip = transportShip,
			unloadContents = unloadContents,
			unsatisfiedDropMode = unsatisfiedDropMode
		};
		quest.AddPart(questPart_SendTransportShipAwayOnCleanup);
		return questPart_SendTransportShipAwayOnCleanup;
	}

	public static QuestPart_AddShipJob AddShipJob(this Quest quest, TransportShip transportShip, ShipJobDef def, ShipJobStartMode startMode = ShipJobStartMode.Queue, string inSignal = null)
	{
		QuestPart_AddShipJob questPart_AddShipJob = new QuestPart_AddShipJob
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			shipJobStartMode = startMode,
			transportShip = transportShip,
			shipJobDef = def
		};
		quest.AddPart(questPart_AddShipJob);
		return questPart_AddShipJob;
	}

	public static QuestPart_AddShipJob_Unload AddShipJob_Unload(this Quest quest, TransportShip transportShip, ShipJobStartMode startMode = ShipJobStartMode.Queue, bool unforbidAll = true, string inSignal = null)
	{
		QuestPart_AddShipJob_Unload questPart_AddShipJob_Unload = new QuestPart_AddShipJob_Unload
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			shipJobStartMode = startMode,
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.Unload,
			unforbidAll = unforbidAll
		};
		quest.AddPart(questPart_AddShipJob_Unload);
		return questPart_AddShipJob_Unload;
	}

	public static QuestPart_AddShipJob_Arrive AddShipJob_Arrive(this Quest quest, TransportShip transportShip, MapParent mapParent, Pawn mapOfPawn = null, IntVec3? cell = null, ShipJobStartMode startMode = ShipJobStartMode.Queue, Faction factionForArrival = null, string inSignal = null)
	{
		if (mapParent is PocketMapParent pocketMapParent)
		{
			mapParent = pocketMapParent.sourceMap.Parent;
		}
		QuestPart_AddShipJob_Arrive questPart_AddShipJob_Arrive = new QuestPart_AddShipJob_Arrive
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			shipJobStartMode = startMode,
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.Arrive,
			cell = (cell ?? IntVec3.Invalid),
			mapParent = mapParent,
			mapOfPawn = mapOfPawn,
			factionForArrival = factionForArrival
		};
		quest.AddPart(questPart_AddShipJob_Arrive);
		return questPart_AddShipJob_Arrive;
	}

	public static QuestPart_AddShipJob_WaitTime AddShipJob_WaitTime(this Quest quest, TransportShip transportShip, int duration, bool leaveImmediatelyWhenSatisfied, List<Thing> sendAwayIfAllDespawned = null, string inSignal = null)
	{
		QuestPart_AddShipJob_WaitTime questPart_AddShipJob_WaitTime = new QuestPart_AddShipJob_WaitTime
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.WaitTime,
			leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied,
			sendAwayIfAllDespawned = sendAwayIfAllDespawned,
			duration = duration
		};
		quest.AddPart(questPart_AddShipJob_WaitTime);
		return questPart_AddShipJob_WaitTime;
	}

	public static QuestPart_AddShipJob_WaitForever AddShipJob_WaitForever(this Quest quest, TransportShip transportShip, bool leaveImmediatelyWhenSatisfied, bool showGizmos, List<Thing> sendAwayIfAllDespawned = null, string inSignal = null)
	{
		QuestPart_AddShipJob_WaitForever questPart_AddShipJob_WaitForever = new QuestPart_AddShipJob_WaitForever
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.WaitForever,
			leaveImmediatelyWhenSatisfied = leaveImmediatelyWhenSatisfied,
			sendAwayIfAllDespawned = sendAwayIfAllDespawned
		};
		quest.AddPart(questPart_AddShipJob_WaitForever);
		return questPart_AddShipJob_WaitForever;
	}

	public static QuestPart_AddShipJob_WaitSendable AddShipJob_WaitSendable(this Quest quest, TransportShip transportShip, MapParent destination, bool leaveImmeiatelyWhenSatisfied = false, bool targetPlayerSettlement = false, string inSignal = null)
	{
		QuestPart_AddShipJob_WaitSendable questPart_AddShipJob_WaitSendable = new QuestPart_AddShipJob_WaitSendable
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.WaitSendable,
			destination = destination,
			leaveImmediatelyWhenSatisfied = leaveImmeiatelyWhenSatisfied,
			targetPlayerSettlement = targetPlayerSettlement
		};
		quest.AddPart(questPart_AddShipJob_WaitSendable);
		return questPart_AddShipJob_WaitSendable;
	}

	public static QuestPart_AddShipJob_FlyAway AddShipJob_FlyAway(this Quest quest, TransportShip transportShip, PlanetTile? destinationTile = null, TransportersArrivalAction arrivalAction = null, TransportShipDropMode dropMode = TransportShipDropMode.NonRequired, string inSignal = null)
	{
		QuestPart_AddShipJob_FlyAway questPart_AddShipJob_FlyAway = new QuestPart_AddShipJob_FlyAway
		{
			inSignal = (inSignal ?? QuestGen.slate.Get<string>("inSignal")),
			transportShip = transportShip,
			shipJobDef = ShipJobDefOf.FlyAway,
			destinationTile = (destinationTile ?? PlanetTile.Invalid),
			arrivalAction = arrivalAction,
			dropMode = dropMode
		};
		quest.AddPart(questPart_AddShipJob_FlyAway);
		return questPart_AddShipJob_FlyAway;
	}
}
