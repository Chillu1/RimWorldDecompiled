using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Root_VoidMonolith : QuestNode
{
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
		if (QuestGen_Get.GetMap() == null)
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
		Building_VoidMonolith monolith = slate.Get<Building_VoidMonolith>("monolith");
		QuestUtility.AddQuestTag(questTagToAdd: QuestGenUtility.HardcodedTargetQuestTagWithQuestID("monolithMap"), questTags: ref map.Parent.questTags);
		string inSignal = QuestGenUtility.HardcodedSignalWithQuestID("monolithMap.Destroyed");
		quest.AddPart(new QuestPart_MonolithPart(monolith));
		quest.End(QuestEndOutcome.Fail, 0, null, inSignal, QuestPart.SignalListenMode.OngoingOnly, sendStandardLetter: true);
		QuestPart_Hyperlinks part = new QuestPart_Hyperlinks();
		quest.AddPart(part);
	}
}
