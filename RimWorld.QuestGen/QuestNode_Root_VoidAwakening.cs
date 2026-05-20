using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using RimWorld.Utility;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_VoidAwakening : QuestNode
{
	private const int StageOneNumStructures = 2;

	private const int StageTwoNumStructures = 3;

	private static readonly FloatRange InterStageDelayHoursRange = new FloatRange(24f, 48f);

	private static readonly IntRange EntityArrivalDelayTicksRange = new IntRange(480, 900);

	private const int GleamingMonolithDelayTicks = 10800;

	public static readonly LargeBuildingSpawnParms StructureSpawnParms = new LargeBuildingSpawnParms
	{
		maxDistanceToColonyBuilding = -1f,
		minDistToEdge = 10,
		attemptSpawnLocationType = SpawnLocationType.Outdoors,
		attemptNotUnderBuildings = true,
		canSpawnOnImpassable = false
	};

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (QuestGen_Get.GetMap() == null)
		{
			return false;
		}
		if (Find.Anomaly.monolith == null)
		{
			return false;
		}
		if (Find.Anomaly.LevelDef != MonolithLevelDefOf.VoidAwakened)
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Building_VoidMonolith monolith = Find.Anomaly.monolith;
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = monolith.Map;
		float var = StorytellerUtility.DefaultThreatPointsNow(map);
		slate.Set("map", map);
		slate.Set("points", var);
		List<CellRect> structureRects = new List<CellRect>();
		List<Thing> tmpStageStructures = new List<Thing>();
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("monolithMap");
		QuestUtility.AddQuestTag(ref map.Parent.questTags, questTagToAdd);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("monolithMap.MapRemoved");
		string questTagToAdd2 = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("monolith");
		string text = QuestGenUtility.HardcodedSignalWithQuestID("monolith.Activated");
		QuestUtility.AddQuestTag(ref monolith.questTags, questTagToAdd2);
		string voidNodeTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("voidNode");
		string inSignal2 = QuestGenUtility.HardcodedSignalWithQuestID("voidNode.NodeClosed");
		string beginDarkeningSignal = QuestGen.GenerateNewSignal("BeginDarkening");
		string darknessEnvelopSignal = QuestGen.GenerateNewSignal("DarknessEnveloped");
		string entityArrivalSignal = QuestGen.GenerateNewSignal("EntityWave");
		StageSignals[] stages = new StageSignals[3];
		for (int i = 0; i < 3; i++)
		{
			string text2 = $"stageStructure.{i}";
			stages[i] = new StageSignals
			{
				structureTag = QuestGenUtility.HardcodedTargetQuestTagWithQuestID(text2),
				beganSignal = QuestGen.GenerateNewSignal($"StageBegan.{i}"),
				structureActivatedSignal = QuestGenUtility.HardcodedSignalWithQuestID(text2 + ".Activated"),
				structureDestroyedSignal = QuestGenUtility.HardcodedSignalWithQuestID(text2 + ".Destroyed"),
				allStructuresActivatedSignal = QuestGenUtility.HardcodedSignalWithQuestID($"AllStructuresActivated.{i}")
			};
		}
		int num = 720;
		quest.AddPart(new QuestPart_Alert_StructuresArriving("[structuresArrivingAlertLabel]", "[structuresArrivingAlertExplanation]", beginDarkeningSignal, stages[0].beganSignal, num));
		quest.Delay(363, delegate
		{
			quest.SignalPass(null, null, beginDarkeningSignal);
		}).debugLabel = "Begin darkness delay";
		quest.GameCondition(GameConditionDefOf.UnnaturalDarkness, -1, inSignal: beginDarkeningSignal, mapParent: map.Parent, permanent: true, forceDisplayAsDuration: true, signalListenMode: QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: false);
		quest.Delay(num, delegate
		{
			quest.SignalPass(null, null, stages[0].beganSignal);
		}).debugLabel = "Stage one delay";
		SpawnStructures(structureRects, stages[0], 2, ThingDefOf.VoidStructure, tmpStageStructures, VoidAwakeningUtility.EncodeWaveType(entityArrivalSignal, VoidAwakeningUtility.WaveType.Twisted, 1f));
		quest.SignalPass(delegate
		{
			quest.Delay(EntityArrivalDelayTicksRange.RandomInRange, delegate
			{
				quest.SignalPass(null, null, VoidAwakeningUtility.EncodeWaveType(entityArrivalSignal, VoidAwakeningUtility.WaveType.Fleshbeast, 1f));
			});
		}, stages[0].beganSignal);
		quest.AddPart(new QuestPart_StructureActivated(2, stages[0].structureActivatedSignal, stages[0].structureDestroyedSignal, stages[0].allStructuresActivatedSignal, 1));
		int num2 = Mathf.RoundToInt(InterStageDelayHoursRange.RandomInRange * 2500f);
		int num3 = num2 - 1800;
		int delayTicks = num3 - 2500;
		quest.AddPart(new QuestPart_Alert_StructuresArriving("[structuresArrivingAlertLabel]", "[structuresArrivingAlertExplanation]", stages[0].allStructuresActivatedSignal, stages[1].beganSignal, num2));
		quest.Delay(delayTicks, delegate
		{
			quest.Letter(LetterDefOf.NeutralEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, label: "DarknessWarningLetterLabel".Translate(), text: "DarknessWarningLetterText".Translate());
		}, stages[0].allStructuresActivatedSignal).debugLabel = "Darkness warning delay";
		quest.Delay(num3, delegate
		{
			quest.SignalPass(null, null, darknessEnvelopSignal);
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, null, filterDeadPawnsFromLookTargets: false, "[darknessStartedLetterText]", null, "[darknessStartedLetterLabel]");
		}, stages[0].allStructuresActivatedSignal).debugLabel = "Darkness delay";
		quest.AddPart(new QuestPart_DarkenMap(darknessEnvelopSignal, map.Parent));
		quest.Delay(num2, delegate
		{
			quest.SignalPass(null, null, stages[1].beganSignal);
		}, stages[0].allStructuresActivatedSignal).debugLabel = "Stage two delay";
		SpawnStructures(structureRects, stages[1], 3, ThingDefOf.VoidStructure, tmpStageStructures, VoidAwakeningUtility.EncodeWaveType(entityArrivalSignal, VoidAwakeningUtility.WaveType.Twisted, 1.25f));
		quest.AddPart(new QuestPart_StructureActivated(3, stages[1].structureActivatedSignal, stages[1].structureDestroyedSignal, stages[1].allStructuresActivatedSignal, 2));
		quest.AddPart(new QuestPart_MonolithTwisting("[monolithTwistingAlertLabel]", "[monolithTwistingAlertExplanation]", stages[1].allStructuresActivatedSignal, stages[2].beganSignal, 10800, monolith, 10500));
		quest.Delay(10500, delegate
		{
			quest.Letter(LetterDefOf.ThreatBig, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(monolith), filterDeadPawnsFromLookTargets: false, "[gleamingMonolithWarningLetterText]", null, "[gleamingMonolithWarningLetterLabel]");
		}, stages[1].allStructuresActivatedSignal);
		quest.Delay(10800, delegate
		{
			quest.SignalPass(null, null, stages[2].beganSignal);
		}, stages[1].allStructuresActivatedSignal).debugLabel = "Stage three delay";
		quest.Delay(11100, delegate
		{
			quest.Letter(LetterDefOf.NegativeEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, Gen.YieldSingle(monolith), filterDeadPawnsFromLookTargets: false, "[gleamingMonolithLetterText]", null, "[gleamingMonolithLetterLabel]");
		}, stages[1].allStructuresActivatedSignal);
		quest.AddPart(new QuestPart_SetMonolithGleaming(monolith, stages[2].beganSignal));
		quest.AddPart(new QuestPart_StructureActivated(1, text, null, stages[2].allStructuresActivatedSignal, 3));
		quest.AddPart(new QuestPart_EntityArrival(entityArrivalSignal, map, monolith, Find.TickManager.TicksGame, structureRects));
		string text3 = QuestGen.GenerateNewSignal("WaveBegan");
		quest.SignalPassAny(null, new List<string>
		{
			stages[0].beganSignal,
			stages[1].beganSignal
		}, text3);
		quest.AddPart(new QuestPart_WanderingEntities(map, text3));
		quest.AddPart(new QuestPart_MetalHell(text, voidNodeTag));
		quest.AddPart(new QuestPart_CleanUpVoidAwakening(inSignal2, map));
		quest.End(QuestEndOutcome.Success, 0, null, inSignal2);
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
	}

	private void SpawnStructures(List<CellRect> structureRects, StageSignals stage, int numStructures, ThingDef structureDef, List<Thing> tmpStageStructures, string structureActivatedSignal)
	{
		tmpStageStructures.Clear();
		Quest quest = QuestGen.quest;
		Map map = QuestGen.slate.Get<Map>("map");
		LargeBuildingSpawnParms parms = StructureSpawnParms.ForThing(structureDef);
		LargeBuildingSpawnParms parms2 = StructureSpawnParms.ForThing(structureDef);
		parms.minDistanceFromUsedRects = 25f;
		List<Thing> list = new List<Thing>();
		for (int i = 0; i < numStructures; i++)
		{
			if (!LargeBuildingCellFinder.TryFindCell(out var cell, map, parms, structureRects, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map)) && !LargeBuildingCellFinder.TryFindCell(out cell, map, parms2, structureRects, (IntVec3 c) => ScattererValidator_AvoidSpecialThings.IsValid(c, map)))
			{
				break;
			}
			IntVec2 size = structureDef.Size;
			structureRects.Add(new CellRect(cell.x - size.x / 2, cell.z - size.z / 2, size.x, size.z));
			Thing structure = ThingMaker.MakeThing(structureDef);
			structure.SetFaction(Faction.OfEntities);
			structure.Position = cell;
			QuestUtility.AddQuestTag(ref structure.questTags, stage.structureTag);
			quest.SpawnThing(map, ThingMaker.MakeThing(ThingDefOf.VoidStructureIncoming), null, cell, stage.beganSignal);
			quest.Delay(EffecterDefOf.VoidStructureIncoming.maintainTicks, delegate
			{
				quest.SpawnThing(map, structure, null, cell, null, lookForSafeSpot: false, tryLandInShipLandingZone: false, null, null, questLookTarget: true, EffecterDefOf.VoidStructureSpawningSkipSequence);
				quest.AddPart(new QuestPart_StructureSpawned(QuestGen.slate.Get<string>("inSignal"), structure));
			}, stage.beganSignal);
			tmpStageStructures.Add(structure);
			list.Add(structure);
			string text = stage.structureTag + "." + i;
			QuestUtility.AddQuestTag(ref structure.questTags, QuestGenUtility.HardcodedTargetQuestTagWithQuestID(text));
			string inSignal = QuestGenUtility.HardcodedSignalWithQuestID(text + ".Activated");
			if (structureActivatedSignal.NullOrEmpty())
			{
				continue;
			}
			quest.SignalPass(delegate
			{
				quest.Delay(EntityArrivalDelayTicksRange.RandomInRange, delegate
				{
					quest.SignalPass(null, null, structureActivatedSignal);
				});
			}, inSignal);
		}
		quest.Letter(LetterDefOf.ThreatBig, stage.beganSignal, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, ((IEnumerable<Thing>)tmpStageStructures).Select((Func<Thing, object>)((Thing s) => new GlobalTargetInfo(s.Position, map))), filterDeadPawnsFromLookTargets: false, "[voidStructuresLetterText]", null, "[voidStructuresLetterLabel]");
		quest.AddPart(new QuestPart_ActivateStructuresAlert(list, stage.beganSignal, stage.allStructuresActivatedSignal));
	}
}
