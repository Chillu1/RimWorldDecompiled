using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_PlantsHarvested : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	public SlateRef<ThingDef> plant;

	public SlateRef<int> count;

	public QuestNode node;

	protected override bool TestRunInt(Slate slate)
	{
		if (node != null)
		{
			return node.TestRun(slate);
		}
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		QuestPart_PlantsHarvested questPart_PlantsHarvested = new QuestPart_PlantsHarvested();
		questPart_PlantsHarvested.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_PlantsHarvested.plant = plant.GetValue(slate);
		questPart_PlantsHarvested.count = count.GetValue(slate);
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_PlantsHarvested);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_PlantsHarvested.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_PlantsHarvested);
	}
}
