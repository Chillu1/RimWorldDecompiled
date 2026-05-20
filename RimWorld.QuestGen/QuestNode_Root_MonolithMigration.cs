using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_MonolithMigration : QuestNode
{
	public static readonly IntRange SpawnDelayRangeTicks = new IntRange(2500, 5000);

	protected override bool TestRunInt(Slate slate)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!Find.Anomaly.GenerateMonolith)
		{
			return false;
		}
		Map map = QuestGen_Get.GetMap();
		if (map == null)
		{
			return false;
		}
		if (map.listerThings.AnyThingWithDef(ThingDefOf.VoidMonolith))
		{
			return false;
		}
		return true;
	}

	protected override void RunInt()
	{
		Quest quest = QuestGen.quest;
		Slate slate = QuestGen.slate;
		Map map = QuestGen_Get.GetMap();
		quest.AcceptanceRequirementNotSpace(map.Parent);
		QuestPart_Choice questPart_Choice = quest.RewardChoice();
		QuestPart_Choice.Choice item = new QuestPart_Choice.Choice
		{
			rewards = { (Reward)new Reward_Unknown() }
		};
		questPart_Choice.choices.Add(item);
		slate.Set("askerIsNull", var: true);
		string questTagToAdd = QuestGenUtility.HardcodedTargetQuestTagWithQuestID("monolithMap");
		QuestUtility.AddQuestTag(ref map.Parent.questTags, questTagToAdd);
		string startSpawnMonolithSignal = QuestGen.GenerateNewSignal("SpawnMonolith");
		quest.Delay(SpawnDelayRangeTicks.RandomInRange, delegate
		{
			quest.SignalPass(null, null, startSpawnMonolithSignal);
		});
		quest.AddPart(new QuestPart_SpawnMonolith(startSpawnMonolithSignal));
		quest.Delay(EffecterDefOf.VoidStructureIncoming.maintainTicks, delegate
		{
			QuestGen_End.End(quest, QuestEndOutcome.Success);
		}, startSpawnMonolithSignal);
	}
}
