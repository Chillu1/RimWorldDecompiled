using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ThingsProduced : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	public SlateRef<ThingDef> def;

	public SlateRef<ThingDef> stuff;

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
		QuestPart_ThingsProduced questPart_ThingsProduced = new QuestPart_ThingsProduced();
		questPart_ThingsProduced.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_ThingsProduced.def = def.GetValue(slate);
		questPart_ThingsProduced.stuff = stuff.GetValue(slate);
		questPart_ThingsProduced.count = count.GetValue(slate);
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_ThingsProduced);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_ThingsProduced.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_ThingsProduced);
	}
}
