using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_SanguophageShip : QuestNode
{
	private const string QuestTag = "SanguophageShip";

	private const int TicksToShuttleArrival = 180;

	private const int TicksToReinforcements = 10000;

	private const int TicksToBeginAssault = 5000;

	private static readonly SimpleCurve PointsToThrallCountCurve = new SimpleCurve
	{
		new CurvePoint(0f, 2f),
		new CurvePoint(2000f, 5f)
	};

	private static readonly SimpleCurve PointsToReinforcementsCountCurve = new SimpleCurve
	{
		new CurvePoint(2000f, 0f),
		new CurvePoint(2500f, 2f),
		new CurvePoint(5000f, 4f),
		new CurvePoint(8000f, 6f)
	};

	protected override void RunInt()
	{
		if (!ModLister.CheckBiotech("Sanguophage ship"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		float x = slate.Get("points", 0f);
		int endTicks = 5240;
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("SanguophageShip");
		string attackedSignal = QuestGenUtility.HardcodedSignalWithQuestID("shuttlePawns.TookDamageFromPlayer");
		string defendTimeoutSignal = QuestGen.GenerateNewSignal("DefendTimeout");
		string beginAssaultSignal = QuestGen.GenerateNewSignal("BeginAssault");
		string assaultBeganSignal = QuestGen.GenerateNewSignal("AssaultBegan");
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
		slate.Set("map", map);
		List<FactionRelation> list = new List<FactionRelation>();
		foreach (Faction item3 in Find.FactionManager.AllFactionsListForReading)
		{
			list.Add(new FactionRelation(item3, FactionRelationKind.Hostile));
		}
		Faction faction = FactionGenerator.NewGeneratedFactionWithRelations(FactionDefOf.Sanguophages, list, hidden: true);
		faction.temporary = true;
		Find.FactionManager.Add(faction);
		quest.ReserveFaction(faction);
		List<Pawn> shuttlePawns = new List<Pawn>();
		Pawn sanguophage = quest.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Sanguophage, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true));
		sanguophage.health.forceDowned = true;
		shuttlePawns.Add(sanguophage);
		slate.Set("sanguophage", sanguophage);
		int num = Mathf.RoundToInt(PointsToThrallCountCurve.Evaluate(x));
		PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.SanguophageThrall, faction, PawnGenerationContext.NonPlayer, null, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: true, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true);
		for (int i = 0; i < num; i++)
		{
			Pawn item = quest.GeneratePawn(request);
			shuttlePawns.Add(item);
		}
		slate.Set("thrallCount", num);
		slate.Set("shuttlePawns", shuttlePawns);
		Thing thing = ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed_Exitable);
		quest.SetFaction(Gen.YieldSingle(thing), faction);
		TryFindShuttleCrashPosition(map, thing.def.size, out var shuttleCrashPosition);
		TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_ShuttleCrashing, shuttlePawns, thing).transportShip;
		quest.AddShipJob_WaitTime(transportShip, 60, leaveImmediatelyWhenSatisfied: false).showGizmos = false;
		quest.AddShipJob(transportShip, ShipJobDefOf.Unload);
		QuestUtility.AddQuestTag(ref transportShip.questTags, questTagToAdd);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal);
		int reinforcementsCount = Mathf.RoundToInt(PointsToReinforcementsCountCurve.Evaluate(x));
		if (reinforcementsCount > 0)
		{
			endTicks = 10060;
		}
		List<Pawn> reinforcements = null;
		if (reinforcementsCount > 0)
		{
			reinforcements = new List<Pawn>();
			for (int j = 0; j < reinforcementsCount; j++)
			{
				Pawn item2 = quest.GeneratePawn(request);
				reinforcements.Add(item2);
			}
			quest.BiocodeWeapons(reinforcements);
		}
		quest.Delay(180, delegate
		{
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, shuttlePawns, filterDeadPawnsFromLookTargets: false, "[sanguophageShuttleCrashedLetterText]", null, "[sanguophageShuttleCrashedLetterLabel]");
			quest.AddShipJob_Arrive(transportShip, map.Parent, sanguophage, shuttleCrashPosition, ShipJobStartMode.Force_DelayCurrent, faction);
			quest.DefendPoint(map.Parent, sanguophage, shuttleCrashPosition, shuttlePawns, faction, null, null, 5f);
			quest.Delay(5000, delegate
			{
				quest.SignalPass(null, null, attackedSignal);
			}).debugLabel = "Assault delay";
			quest.AnySignal(new string[2] { attackedSignal, defendTimeoutSignal }, null, Gen.YieldSingle(beginAssaultSignal));
			quest.SignalPassActivable(delegate
			{
				quest.AnyPawnInCombatShape(shuttlePawns, delegate
				{
					QuestPart_AssaultColony questPart_AssaultColony = quest.AssaultColony(faction, map.Parent, shuttlePawns);
					questPart_AssaultColony.canKidnap = false;
					questPart_AssaultColony.canSteal = false;
					questPart_AssaultColony.canTimeoutOrFlee = false;
					quest.Letter(LetterDefOf.ThreatSmall, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, shuttlePawns, filterDeadPawnsFromLookTargets: false, "[assaultBeginLetterText]", null, "[assaultBeginLetterLabel]");
				}, null, null, assaultBeganSignal);
			}, null, beginAssaultSignal, null, null, assaultBeganSignal);
			if (reinforcementsCount > 0)
			{
				quest.Delay(10000, delegate
				{
					quest.Letter(LetterDefOf.ThreatBig, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, reinforcements, filterDeadPawnsFromLookTargets: false, "[reinforcementsArrivedLetterText]", null, "[reinforcementsArrivedLetterLabel]");
					DropCellFinder.TryFindRaidDropCenterClose(out var spot, map);
					quest.DropPods(map.Parent, reinforcements, null, null, null, null, false, useTradeDropSpot: false, joinPlayer: false, makePrisoners: false, null, null, QuestPart.SignalListenMode.OngoingOnly, spot);
					QuestPart_AssaultColony questPart_AssaultColony = quest.AssaultColony(faction, map.Parent, reinforcements);
					questPart_AssaultColony.canSteal = false;
					questPart_AssaultColony.canTimeoutOrFlee = false;
				}).debugLabel = "Reinforcements delay";
			}
			quest.Delay(endTicks, delegate
			{
				QuestGen_End.End(quest, QuestEndOutcome.Success);
			}).debugLabel = "End delay";
		}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, null, tickHistorically: false, QuestPart.SignalListenMode.OngoingOnly, waitUntilPlayerHasHomeMap: true).debugLabel = "Arrival delay";
	}

	protected override bool TestRunInt(Slate slate)
	{
		Map map = QuestGen_Get.GetMap();
		if (map == null)
		{
			return false;
		}
		if (!TryFindShuttleCrashPosition(map, ThingDefOf.ShuttleCrashed.size, out var _))
		{
			return false;
		}
		return true;
	}

	private bool TryFindShuttleCrashPosition(Map map, IntVec2 size, out IntVec3 spot)
	{
		if (DropCellFinder.FindSafeLandingSpot(out spot, null, map, 35, 15, 25, size, ThingDefOf.ShuttleCrashed.interactionCellOffset))
		{
			return true;
		}
		return false;
	}
}
