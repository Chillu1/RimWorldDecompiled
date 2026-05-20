using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public abstract class QuestNode_Root_ArchonexusVictory_Cycle : QuestNode
{
	public const int ColonistsAllowed = 5;

	public const int AnimalsAllowed = 5;

	public const float RequiredWealth = 350000f;

	public const int LetterReminderInterval = 3600000;

	protected Map map;

	protected abstract int ArchonexusCycle { get; }

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		this.map = QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true);
		string text = QuestGen.GenerateNewSignal("PlayerWealthSatisfied");
		string text2 = QuestGen.GenerateNewSignal("SendLetterReminder");
		QuestGen.GenerateNewSignal("ActivateLetterReminderSignal");
		QuestPart_RequirementsToAcceptPlayerWealth questPart_RequirementsToAcceptPlayerWealth = new QuestPart_RequirementsToAcceptPlayerWealth();
		questPart_RequirementsToAcceptPlayerWealth.requiredPlayerWealth = 350000f;
		quest.AddPart(questPart_RequirementsToAcceptPlayerWealth);
		QuestPart_PlayerWealth questPart_PlayerWealth = new QuestPart_PlayerWealth();
		questPart_PlayerWealth.inSignalEnable = quest.AddedSignal;
		questPart_PlayerWealth.playerWealth = 350000f;
		questPart_PlayerWealth.outSignalsCompleted.Add(text);
		questPart_PlayerWealth.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
		quest.AddPart(questPart_PlayerWealth);
		QuestPart_PassOutInterval questPart_PassOutInterval = new QuestPart_PassOutInterval();
		questPart_PassOutInterval.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
		questPart_PassOutInterval.inSignalEnable = text;
		questPart_PassOutInterval.ticksInterval = new IntRange(3600000, 3600000);
		questPart_PassOutInterval.outSignals.Add(text2);
		quest.AddPart(questPart_PassOutInterval);
		QuestPart_Filter_PlayerWealth questPart_Filter_PlayerWealth = new QuestPart_Filter_PlayerWealth();
		questPart_Filter_PlayerWealth.minPlayerWealth = 350000f;
		questPart_Filter_PlayerWealth.inSignal = text2;
		questPart_Filter_PlayerWealth.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
		questPart_Filter_PlayerWealth.signalListenMode = QuestPart.SignalListenMode.NotYetAcceptedOnly;
		quest.AddPart(questPart_Filter_PlayerWealth);
		quest.CanAcceptQuest(delegate
		{
			QuestNode_ResolveQuestName.Resolve();
			string text3 = slate.Get<string>("resolvedQuestName");
			quest.Letter(LetterDefOf.PositiveEvent, null, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.NotYetAcceptedOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelArchonexusWealthReached".Translate(text3), text: "LetterTextArchonexusWealthReached".Translate(text3));
		}, null, questPart_Filter_PlayerWealth.outSignal, null, null, QuestPart.SignalListenMode.NotYetAcceptedOnly);
		Reward_ArchonexusMap reward_ArchonexusMap = new Reward_ArchonexusMap();
		reward_ArchonexusMap.currentPart = ArchonexusCycle;
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)reward_ArchonexusMap }
		};
		questPart_Choice.choices.Add(item);
		List<MapParent> list = new List<MapParent>();
		List<Map> maps = Find.Maps;
		for (int num = 0; num < maps.Count; num++)
		{
			Map map = maps[num];
			if (map.IsPlayerHome)
			{
				list.Add(map.Parent);
			}
		}
		slate.Set("playerSettlements", list);
		slate.Set("playerSettlementsCount", list.Count);
		slate.Set("colonistsAllowed", 5);
		slate.Set("animalsAllowed", 5);
		slate.Set("requiredWealth", 350000f);
		slate.Set("map", this.map);
		slate.Set("mapParent", this.map.Parent);
	}

	protected void PickNewColony(Faction takeOverFaction, WorldObjectDef worldObjectDef, SoundDef colonyStartSounDef, int maxRelics = 1)
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		string text = QuestGen.GenerateNewSignal("NewColonyCreated");
		string text2 = QuestGen.GenerateNewSignal("NewColonyCancelled");
		quest.AddPart(new QuestPart_NewColony
		{
			inSignal = slate.Get<string>("inSignal"),
			otherFaction = takeOverFaction,
			outSignalCompleted = text,
			outSignalCancelled = text2,
			worldObjectDef = worldObjectDef,
			maxRelics = maxRelics
		});
		quest.SetQuestNotYetAccepted(text2, revertOngoingQuestAfterLoad: true);
		quest.End(QuestEndOutcome.Success, 0, null, text);
	}

	protected void TryAddStudyRequirement(Quest quest, Slate slate, ThingDef buildingToStudyDef)
	{
		Thing thing = map.listerThings.ThingsOfDef(buildingToStudyDef).FirstOrDefault();
		if (thing != null)
		{
			slate.Set("archonexusMajorStructure", thing);
			slate.Set("studyRequirement", var: true);
			quest.Letter(LetterDefOf.PositiveEvent, QuestGenUtility.HardcodedSignalWithQuestID("archonexusMajorStructure.Researched"), null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.NotYetAcceptedOnly, null, filterDeadPawnsFromLookTargets: false, label: "LetterLabelArchonexusStructureResearched".Translate(thing), text: "LetterTextArchonexusStructureResearched".Translate(thing));
			QuestPart_RequirementsToAcceptThingStudied_ArchotechStructures questPart_RequirementsToAcceptThingStudied_ArchotechStructures = new QuestPart_RequirementsToAcceptThingStudied_ArchotechStructures();
			questPart_RequirementsToAcceptThingStudied_ArchotechStructures.thing = thing;
			quest.AddPart(questPart_RequirementsToAcceptThingStudied_ArchotechStructures);
		}
		else
		{
			slate.Set("studyRequirement", var: false);
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		return QuestGen_Get.GetMap(mustBeInfestable: false, null, canBeSpace: true) != null;
	}
}
