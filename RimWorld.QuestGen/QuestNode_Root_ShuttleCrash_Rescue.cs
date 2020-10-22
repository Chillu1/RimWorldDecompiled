using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.QuestGen
{
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

		private static QuestGen_Pawns.GetPawnParms CivilianPawnParams
		{
			get
			{
				QuestGen_Pawns.GetPawnParms result = default(QuestGen_Pawns.GetPawnParms);
				result.mustBeOfFaction = Faction.Empire;
				result.canGeneratePawn = true;
				result.mustBeWorldPawn = true;
				result.mustBeOfKind = PawnKindDefOf.Empire_Common_Lodger;
				return result;
			}
		}

		protected override void RunInt()
		{
			if (!ModLister.RoyaltyInstalled)
			{
				Log.ErrorOnce("Shuttle crash rescue is a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.", 8811221);
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
			int max = Mathf.FloorToInt(MaxCiviliansByPointsCurve.Evaluate(questPoints));
			int num = Rand.Range(1, max);
			int max2 = Mathf.FloorToInt(MaxSoldiersByPointsCurve.Evaluate(questPoints));
			int num2 = Rand.Range(1, max2);
			TryFindEnemyFaction(out var enemyFaction);
			Thing crashedShuttle = ThingMaker.MakeThing(ThingDefOf.ShuttleCrashed);
			TryFindShuttleCrashPosition(map, Faction.Empire, crashedShuttle.def.size, out var shuttleCrashPosition);
			List<Pawn> civilians = new List<Pawn>();
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < num - 1; i++)
			{
				Pawn pawn = quest.GetPawn(CivilianPawnParams);
				civilians.Add(pawn);
				list.Add(pawn);
			}
			Quest quest2 = quest;
			QuestGen_Pawns.GetPawnParms parms = new QuestGen_Pawns.GetPawnParms
			{
				mustBeOfFaction = Faction.Empire,
				canGeneratePawn = true,
				mustBeWorldPawn = true,
				seniorityRange = new FloatRange(100f, MaxAskerSeniorityByPointsCurve.Evaluate(questPoints)),
				mustHaveRoyalTitleInCurrentFaction = true
			};
			Pawn asker = quest2.GetPawn(parms);
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
				Quest quest3 = quest;
				parms = new QuestGen_Pawns.GetPawnParms
				{
					mustBeOfFaction = Faction.Empire,
					canGeneratePawn = true,
					mustBeOfKind = mustBeOfKind,
					mustBeWorldPawn = true
				};
				Pawn pawn2 = quest3.GetPawn(parms);
				soldiers.Add(pawn2);
			}
			List<Pawn> allPassengers = new List<Pawn>();
			allPassengers.AddRange(soldiers);
			allPassengers.AddRange(civilians);
			quest.BiocodeWeapons(allPassengers);
			Thing rescueShuttle = QuestGen_Shuttle.GenerateShuttle(Faction.Empire, allPassengers, null, acceptColonists: false, onlyAcceptColonists: false, onlyAcceptHealthy: false, -1, dropEverythingIfUnsatisfied: false, leaveImmediatelyWhenSatisfied: true, dropEverythingOnArrival: false, stayAfterDroppedEverythingOnArrival: false, null, null, -1, null, permitShuttle: false, hideControls: true, allPassengers.Cast<Thing>().ToList());
			quest.Delay(120, delegate
			{
				quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "LetterLabelShuttleCrashed".Translate(), text: "LetterTextShuttleCrashed".Translate(), lookTargets: Gen.YieldSingle(crashedShuttle));
				quest.SpawnSkyfaller(map, ThingDefOf.ShuttleCrashing, Gen.YieldSingle(crashedShuttle), Faction.OfPlayer, shuttleCrashPosition);
				quest.DropPods(map.Parent, allPassengers, null, null, null, null, dropSpot: shuttleCrashPosition, sendStandardLetter: false);
				quest.DefendPoint(map.Parent, shuttleCrashPosition, soldiers, Faction.Empire, null, null, 12f, isCaravanSendable: false, addFleeToil: false);
				IntVec3 position = shuttleCrashPosition + IntVec3.South;
				quest.WaitForEscort(map.Parent, civilians, Faction.Empire, position, null, addFleeToil: false);
				string inSignal4 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.Spawned");
				quest.ExitOnShuttle(map.Parent, allPassengers, Faction.Empire, rescueShuttle, inSignal4, addFleeToil: false);
				quest.SendShuttleAwayOnCleanup(rescueShuttle);
				quest.ShuttleDelay(20000, civilians, delegate
				{
					quest.Letter(LetterDefOf.NeutralEvent, null, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(rescueShuttle), filterDeadPawnsFromLookTargets: false, "[rescueShuttleArrivedLetterText]", null, "[rescueShuttleArrivedLetterLabel]");
					quest.SpawnSkyfaller(map, ThingDefOf.ShuttleIncoming, Gen.YieldSingle(rescueShuttle), Faction.Empire, null, null, lookForSafeSpot: false, tryLandInShipLandingZone: false, crashedShuttle);
					quest.ShuttleLeaveDelay(rescueShuttle, 30000, null, null, null, delegate
					{
						quest.SendShuttleAway(rescueShuttle);
						quest.End(QuestEndOutcome.Fail, 0, null, null, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
					});
				}, null, null, alert: true);
				IntVec3 walkIntSpot = default(IntVec3);
				TryFindRaidWalkInPosition(map, shuttleCrashPosition, out walkIntSpot);
				float soldiersTotalCombatPower = 0f;
				for (int k = 0; k < soldiers.Count; k++)
				{
					soldiersTotalCombatPower += soldiers[k].kindDef.combatPower;
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
					for (int l = 0; l < list2.Count; l++)
					{
						Find.WorldPawns.PassToWorld(list2[l]);
						QuestGen.AddToGeneratedPawns(list2[l]);
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
			string[] inSignalsShuttleSent = new string[2]
			{
				text,
				text2
			};
			string text3 = QuestGenUtility.HardcodedSignalWithQuestID("rescueShuttle.Destroyed");
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("asker.Destroyed");
			quest.GoodwillChangeShuttleSentThings(Faction.Empire, list, -5, null, inSignalsShuttleSent, text3, "GoodwillChangeReason_CiviliansLost".Translate(), canSendMessage: true, canSendHostilityLetter: false, QuestPart.SignalListenMode.Always);
			quest.GoodwillChangeShuttleSentThings(Faction.Empire, Gen.YieldSingle(asker), -10, null, inSignalsShuttleSent, text3, "GoodwillChangeReason_CommanderLost".Translate(), canSendMessage: true, canSendHostilityLetter: false, QuestPart.SignalListenMode.Always);
			quest.Leave(allPassengers, "", sendStandardLetter: false);
			quest.Letter(LetterDefOf.PositiveEvent, text, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[questCompletedSuccessLetterText]", null, "[questCompletedSuccessLetterLabel]");
			string questSuccess = QuestGen.GenerateNewSignal("QuestSuccess");
			quest.SignalPass(delegate
			{
				RewardsGeneratorParams rewardsGeneratorParams = default(RewardsGeneratorParams);
				rewardsGeneratorParams.rewardValue = questPoints;
				rewardsGeneratorParams.allowGoodwill = true;
				rewardsGeneratorParams.allowRoyalFavor = true;
				RewardsGeneratorParams parms2 = rewardsGeneratorParams;
				quest.GiveRewards(parms2, null, null, null, null, null, null, null, null, addCampLootReward: false, asker, addShuttleLootReward: true);
				QuestGen_End.End(quest, QuestEndOutcome.Success);
			}, questSuccess);
			quest.SignalPass(null, text, questSuccess);
			quest.AnyOnTransporter(allPassengers, rescueShuttle, delegate
			{
				quest.AnyOnTransporter(Gen.YieldSingle(asker), rescueShuttle, delegate
				{
					quest.Letter(LetterDefOf.PositiveEvent, null, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[questCompletedCiviliansLostSuccessLetterText]", null, "[questCompletedCiviliansLostSuccessLetterLabel]");
					quest.SignalPass(null, null, questSuccess);
				}, delegate
				{
					quest.Letter(LetterDefOf.NegativeEvent, null, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[askerLostLetterText]", null, "[askerLostLetterLabel]");
					QuestGen_End.End(quest, QuestEndOutcome.Fail);
				});
			}, delegate
			{
				quest.Letter(LetterDefOf.NegativeEvent, null, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[allLostLetterText]", null, "[allLostLetterLabel]");
				QuestGen_End.End(quest, QuestEndOutcome.Fail);
			}, text2);
			quest.Letter(LetterDefOf.NegativeEvent, inSignal, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(asker), filterDeadPawnsFromLookTargets: false, "[askerDiedLetterText]", null, "[askerDiedLetterLabel]");
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal);
			quest.Letter(LetterDefOf.NegativeEvent, text3, null, Faction.Empire, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(rescueShuttle), filterDeadPawnsFromLookTargets: false, "[shuttleDestroyedLetterText]", null, "[shuttleDestroyedLetterLabel]");
			quest.End(QuestEndOutcome.Fail, 0, null, text3);
			quest.End(QuestEndOutcome.Fail, 0, null, QuestGenUtility.HardcodedSignalWithQuestID("asker.LeftMap"), QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("askerFaction.BecameHostileToPlayer");
			quest.End(QuestEndOutcome.Fail, 0, null, inSignal2, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
			quest.End(QuestEndOutcome.InvalidPreAcceptance, 0, null, inSignal2, QuestPart.SignalListenMode.NotYetAcceptedOnly);
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
			return Find.FactionManager.AllFactionsVisible.Where((Faction f) => f.HostileTo(Faction.Empire) && f.HostileTo(Faction.OfPlayer)).TryRandomElement(out enemyFaction);
		}

		private bool TryFindShuttleCrashPosition(Map map, Faction faction, IntVec2 size, out IntVec3 spot)
		{
			if (DropCellFinder.FindSafeLandingSpot(out spot, faction, map, 35, 15, 25, size))
			{
				return true;
			}
			return false;
		}

		private bool TryFindRaidWalkInPosition(Map map, IntVec3 shuttleCrashSpot, out IntVec3 spawnSpot)
		{
			Predicate<IntVec3> predicate = (IntVec3 p) => !map.roofGrid.Roofed(p) && p.Walkable(map) && map.reachability.CanReach(p, shuttleCrashSpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some);
			if (RCellFinder.TryFindEdgeCellFromTargetAvoidingColony(shuttleCrashSpot, map, predicate, out spawnSpot))
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
			if (!Find.Storyteller.difficultyValues.allowViolentQuests)
			{
				return false;
			}
			if (!QuestGen_Pawns.GetPawnTest(CivilianPawnParams, out var _))
			{
				return false;
			}
			if (Faction.Empire.PlayerRelationKind == FactionRelationKind.Hostile)
			{
				return false;
			}
			if (!TryFindEnemyFaction(out var _))
			{
				return false;
			}
			Map map = QuestGen_Get.GetMap();
			if (!TryFindShuttleCrashPosition(map, Faction.Empire, ThingDefOf.ShuttleCrashed.size, out var spot))
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
}
