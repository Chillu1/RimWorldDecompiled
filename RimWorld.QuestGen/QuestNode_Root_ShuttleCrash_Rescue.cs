using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.QuestGen;

public class QuestNode_Root_ShuttleCrash_Rescue : QuestNode
{
	private const int WanderRadius_Soldiers = 12;

	private const int QuestStartDelay = 120;

	private const int RescueShuttle_Delay = 20000;

	private const int RescueShuttle_LeaveDelay = 30000;

	private const int RaidDelay = 10000;

	private const float MinRaidDistance_Colony = 15f;

	private const float MinRaidDistance_ShuttleCrash = 15f;

	private const int FactionGoodwillChange_AskerLost = -10;

	private const int FactionGoodwillChange_CivilianLost = -5;

	private const float ThreatPointsFactor = 0.37f;

	private const float MinAskerSeniority = 100f;

	private static readonly SimpleCurve MaxCiviliansByPointsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 2f),
		new CurvePoint(500f, 4f)
	};

	private static readonly SimpleCurve MaxSoldiersByPointsCurve = new SimpleCurve
	{
		new CurvePoint(0f, 1f),
		new CurvePoint(100f, 3f),
		new CurvePoint(500f, 6f)
	};

	private static readonly SimpleCurve MaxAskerSeniorityByPointsCurve = new SimpleCurve
	{
		new CurvePoint(300f, 100f),
		new CurvePoint(1500f, 850f)
	};

	private static QuestGen_Pawns.GetPawnParms CivilianPawnParams => new QuestGen_Pawns.GetPawnParms
	{
		mustBeOfFaction = Faction.OfEmpire,
		canGeneratePawn = true,
		mustBeWorldPawn = true,
		mustBeOfKind = PawnKindDefOf.Empire_Common_Lodger
	};

	protected override void RunInt()
	{
		if (!ModLister.CheckRoyalty("Shuttle crash rescue"))
		{
			return;
		}
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		float questPoints = slate.Get("points", 0f);
		slate.Set("map", map);
		slate.Set("rescueDelay", 20000);
		slate.Set("leaveDelay", 30000);
		slate.Set("rescueShuttleAfterRaidDelay", 10000);
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("civilian");
		int maxExclusive = Mathf.FloorToInt(MaxCiviliansByPointsCurve.Evaluate(questPoints));
		int num = Rand.Range(1, maxExclusive);
		int maxExclusive2 = Mathf.FloorToInt(MaxSoldiersByPointsCurve.Evaluate(questPoints));
		int num2 = Rand.Range(1, maxExclusive2);
		TryFindEnemyFaction(out var enemyFaction);
		Thing crashedShuttle = ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed);
		TryFindShuttleCrashPosition(map, Faction.OfEmpire, crashedShuttle.def.size, crashedShuttle.def.interactionCellOffset, out var shuttleCrashPosition);
		List<Pawn> civilians = new List<Pawn>();
		List<Pawn> list = new List<Pawn>();
		for (int i = 0; i < num - 1; i++)
		{
			Pawn pawn = quest.GetPawn(CivilianPawnParams);
			QuestUtility.AddQuestTag(ref pawn.questTags, questTagToAdd);
			civilians.Add(pawn);
			list.Add(pawn);
		}
		Pawn asker = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
		{
			mustBeOfFaction = Faction.OfEmpire,
			canGeneratePawn = true,
			mustBeWorldPawn = true,
			seniorityRange = new FloatRange(100f, MaxAskerSeniorityByPointsCurve.Evaluate(questPoints)),
			mustHaveRoyalTitleInCurrentFaction = true
		});
		civilians.Add(asker);
		PawnKindDef mustBeOfKind = new PawnKindDef[3]
		{
			PawnKindDefOf.Empire_Fighter_Trooper,
			PawnKindDefOf.Empire_Fighter_Janissary,
			PawnKindDefOf.Empire_Fighter_Cataphract
		}.RandomElement();
		List<Pawn> soldiers = new List<Pawn>();
		for (int j = 0; j < num2; j++)
		{
			Pawn pawn2 = quest.GetPawn(new QuestGen_Pawns.GetPawnParms
			{
				mustBeOfFaction = Faction.OfEmpire,
				canGeneratePawn = true,
				mustBeOfKind = mustBeOfKind,
				mustBeWorldPawn = true,
				mustBeCapableOfViolence = true
			});
			soldiers.Add(pawn2);
		}
		List<Pawn> allPassengers = new List<Pawn>();
		allPassengers.AddRange(soldiers);
		allPassengers.AddRange(civilians);
		quest.BiocodeWeapons(allPassengers);
		Thing rescueShuttle = QuestGen_Shuttle.GenerateShuttle(Faction.OfEmpire, allPassengers);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("soldiers.Rescued");
		quest.RemoveFromRequiredPawnsOnRescue(rescueShuttle, soldiers, inSignal);
		quest.Delay(120, delegate
		{
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "LetterLabelShuttleCrashed".Translate(), text: "LetterTextShuttleCrashed".Translate(), lookTargets: Gen.YieldSingle(crashedShuttle));
			quest.SpawnSkyfaller(map, ThingDefOf.ShuttleCrashing, Gen.YieldSingle(crashedShuttle), Faction.OfPlayer, shuttleCrashPosition);
			quest.DropPods(map.Parent, allPassengers, null, null, null, null, dropSpot: shuttleCrashPosition, sendStandardLetter: false);
			quest.DefendPoint(map.Parent, asker, shuttleCrashPosition, soldiers, Faction.OfEmpire, null, null, 12f, isCaravanSendable: false, addFleeToil: false);
			IntVec3 position = shuttleCrashPosition + IntVec3.South;
			quest.WaitForEscort(map.Parent, civilians, Faction.OfEmpire, position, null, addFleeToil: false);
			string inSignal7 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.Spawned");
			quest.ExitOnShuttle(map.Parent, allPassengers, Faction.OfEmpire, rescueShuttle, inSignal7, addFleeToil: false);
			quest.ShuttleDelay(20000, civilians, delegate
			{
				quest.Letter(LetterDefOf.NeutralEvent, null, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(rescueShuttle), filterDeadPawnsFromLookTargets: false, "[rescueShuttleArrivedLetterText]", null, "[rescueShuttleArrivedLetterLabel]");
				TransportShip transportShip = quest.GenerateTransportShip(TransportShipDefOf.Ship_Shuttle, null, rescueShuttle).transportShip;
				quest.SendTransportShipAwayOnCleanup(transportShip);
				DropCellFinder.TryFindDropSpotNear(shuttleCrashPosition, map, out var result, allowFogged: false, canRoofPunch: false, allowIndoors: false, ThingDefOf.Shuttle.Size + new IntVec2(2, 2), mustBeReachableFromCenter: false);
				quest.AddShipJob_Arrive(transportShip, map.Parent, null, result, ShipJobStartMode.Instant, Faction.OfEmpire);
				quest.AddShipJob_WaitTime(transportShip, 30000, leaveImmediatelyWhenSatisfied: true, allPassengers.Cast<Thing>().ToList());
				quest.ShuttleLeaveDelay(rescueShuttle, 30000);
				quest.AddShipJob_FlyAway(transportShip, null, null, TransportShipDropMode.None);
			}, null, null, alert: true);
			TryFindRaidWalkInPosition(map, shuttleCrashPosition, out var walkIntSpot);
			float soldiersTotalCombatPower = 0f;
			for (int num3 = 0; num3 < soldiers.Count; num3++)
			{
				soldiersTotalCombatPower += soldiers[num3].kindDef.combatPower;
			}
			quest.Delay(10000, delegate
			{
				List<Pawn> list2 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
				{
					faction = enemyFaction,
					groupKind = PawnGroupKindDefOf.Combat,
					points = (questPoints + soldiersTotalCombatPower) * 0.37f,
					tile = map.Tile
				}).ToList();
				for (int k = 0; k < list2.Count; k++)
				{
					Find.WorldPawns.PassToWorld(list2[k]);
					QuestGen.AddToGeneratedPawns(list2[k]);
				}
				QuestPart_PawnsArrive questPart_PawnsArrive = new QuestPart_PawnsArrive();
				questPart_PawnsArrive.pawns.AddRange(list2);
				questPart_PawnsArrive.arrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
				questPart_PawnsArrive.joinPlayer = false;
				questPart_PawnsArrive.mapParent = map.Parent;
				questPart_PawnsArrive.spawnNear = walkIntSpot;
				questPart_PawnsArrive.inSignal = QuestGen.slate.Get<string>("inSignal");
				questPart_PawnsArrive.sendStandardLetter = false;
				quest.AddPart(questPart_PawnsArrive);
				quest.AssaultThings(map.Parent, list2, enemyFaction, allPassengers, null, null, excludeFromLookTargets: true);
				quest.Letter(LetterDefOf.ThreatBig, null, null, enemyFaction, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, list2, filterDeadPawnsFromLookTargets: false, "[raidArrivedLetterText]", null, "[raidArrivedLetterLabel]");
			}, null, null, null, reactivatable: false, null, null, isQuestTimeout: false, null, null, "RaidDelay");
		});
		string text = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.SentSatisfied");
		string text2 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.SentUnsatisfied");
		string[] inSignalsShuttleSent = new string[2] { text, text2 };
		string text3 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.Destroyed");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.LeftBehind");
		string inSignal3 = QuestGenUtility.HardcodedSignalWithQuestID("asker.Destroyed");
		string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("civilian.Destroyed");
		string inSignal5 = QuestGenUtility.HardcodedSignalWithQuestID("map.MapRemoved");
		quest.GoodwillChangeShuttleSentThings(Faction.OfEmpire, list, -5, null, inSignalsShuttleSent, text3, HistoryEventDefOf.ShuttleGuardsMissedShuttle, canSendMessage: true, canSendHostilityLetter: false, QuestPart.SignalListenMode.Always);
		quest.GoodwillChangeShuttleSentThings(Faction.OfEmpire, Gen.YieldSingle(asker), -10, null, inSignalsShuttleSent, text3, HistoryEventDefOf.ShuttleCommanderMissedShuttle, canSendMessage: true, canSendHostilityLetter: false, QuestPart.SignalListenMode.Always);
		quest.Leave(allPassengers, "", sendStandardLetter: false);
		quest.Letter(LetterDefOf.PositiveEvent, text, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[questCompletedSuccessLetterText]", null, "[questCompletedSuccessLetterLabel]");
		string questSuccess = QuestGen.GenerateNewSignal("QuestSuccess");
		quest.SignalPass(delegate
		{
			RewardsGeneratorParams parms = new RewardsGeneratorParams
			{
				rewardValue = questPoints,
				allowGoodwill = true,
				allowRoyalFavor = true,
				allowDevelopmentPoints = true
			};
			Quest quest2 = quest;
			Pawn asker2 = asker;
			quest2.GiveRewards(parms, null, null, null, null, null, null, null, null, addCampLootReward: false, asker2, addShuttleLootReward: true);
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		}, questSuccess);
		quest.SignalPass(null, text, questSuccess);
		quest.AnyOnTransporter(allPassengers, rescueShuttle, delegate
		{
			quest.AnyOnTransporter(Gen.YieldSingle(asker), rescueShuttle, delegate
			{
				quest.Letter(LetterDefOf.PositiveEvent, null, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[questCompletedCiviliansLostSuccessLetterText]", null, "[questCompletedCiviliansLostSuccessLetterLabel]");
				quest.SignalPass(null, null, questSuccess);
			}, delegate
			{
				quest.Letter(LetterDefOf.NegativeEvent, null, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[askerLostLetterText]", null, "[askerLostLetterLabel]");
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			});
		}, delegate
		{
			quest.Letter(LetterDefOf.NegativeEvent, null, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[allLostLetterText]", null, "[allLostLetterLabel]");
			QuestGen_End.End(quest, QuestEndOutcome.Fail);
		}, text2);
		quest.Letter(LetterDefOf.NegativeEvent, inSignal3, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(asker), filterDeadPawnsFromLookTargets: false, "[askerDiedLetterText]", null, "[askerDiedLetterLabel]");
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal3);
		quest.Letter(LetterDefOf.NegativeEvent, inSignal4, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, civilians, filterDeadPawnsFromLookTargets: false, "[civilianDiedLetterText]", null, "[civilianDiedLetterLabel]");
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal4);
		quest.Letter(LetterDefOf.NegativeEvent, text3, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(rescueShuttle), filterDeadPawnsFromLookTargets: false, "[shuttleDestroyedLetterText]", null, "[shuttleDestroyedLetterLabel]");
		quest.End(QuestEndOutcome.Fail, 0, null, text3);
		quest.Letter(LetterDefOf.NegativeEvent, inSignal2, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(rescueShuttle), filterDeadPawnsFromLookTargets: false, "[shuttleLeftBehindLetterText]", null, "[shuttleLeftBehindLetterLabel]");
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal2);
		quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("asker.LeftMap"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		string inSignal6 = QuestGenUtility.HardcodedSignalWithQuestID("askerFaction.BecameHostileToPlayer");
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal6, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		quest.End(QuestEndOutcome.InvalidPreAcceptance, 0, null, inSignal6, QuestPart.SignalListenMode.NotYetAcceptedOnly);
		quest.Letter(LetterDefOf.NegativeEvent, inSignal5, null, Faction.OfEmpire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(asker), filterDeadPawnsFromLookTargets: false, "[mapRemovedLetterText]", null, "[mapRemovedLetterLabel]");
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal5);
		slate.Set("asker", asker);
		slate.Set("askerFaction", asker.Faction);
		slate.Set("enemyFaction", enemyFaction);
		slate.Set("soldiers", soldiers);
		slate.Set("civilians", civilians);
		slate.Set("civilianCountMinusOne", civilians.Count - 1);
		slate.Set("rescueShuttle", rescueShuttle);
	}

	private bool TryFindEnemyFaction(out Faction enemyFaction)
	{
		return Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.HostileTo(Faction.OfEmpire) && f.HostileTo(Faction.OfPlayer)).TryRandomElement(out enemyFaction);
	}

	private bool TryFindShuttleCrashPosition(Map map, Faction faction, IntVec2 size, IntVec3? interactionCell, out IntVec3 spot)
	{
		if (DropCellFinder.FindSafeLandingSpot(out spot, faction, map, 35, 15, 25, size, interactionCell))
		{
			return true;
		}
		return false;
	}

	private bool TryFindRaidWalkInPosition(Map map, IntVec3 shuttleCrashSpot, out IntVec3 spawnSpot)
	{
		Predicate<IntVec3> predicate = (IntVec3 p) => (map.TileInfo.AllowRoofedEdgeWalkIn || !map.roofGrid.Roofed(p)) && p.Walkable(map) && map.reachability.CanReach(p, shuttleCrashSpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some);
		if (RCellFinder.TryFindEdgeCellFromPositionAvoidingColony(shuttleCrashSpot, map, predicate, out spawnSpot))
		{
			return true;
		}
		if (CellFinder.TryFindRandomEdgeCellWith(predicate, map, CellFinder.EdgeRoadChance_Hostile, out spawnSpot))
		{
			return true;
		}
		spawnSpot = IntVec3.Invalid;
		return false;
	}

	protected override bool TestRunInt(Slate slate)
	{
		if (!Find.Storyteller.difficulty.allowViolentQuests)
		{
			return false;
		}
		if (Faction.OfEmpire == null)
		{
			return false;
		}
		if (!QuestGen_Pawns.GetPawnTest(CivilianPawnParams, out var _))
		{
			return false;
		}
		if (Faction.OfEmpire.PlayerRelationKind == FactionRelationKind.Hostile)
		{
			return false;
		}
		if (!TryFindEnemyFaction(out var _))
		{
			return false;
		}
		Map map = QuestGen_Get.GetMap();
		if (map == null)
		{
			return false;
		}
		if (!TryFindShuttleCrashPosition(map, Faction.OfEmpire, ThingDefOf.ShuttleCrashed.size, ThingDefOf.ShuttleCrashed.interactionCellOffset, out var spot))
		{
			return false;
		}
		if (!TryFindRaidWalkInPosition(map, spot, out var _))
		{
			return false;
		}
		return true;
	}
}
