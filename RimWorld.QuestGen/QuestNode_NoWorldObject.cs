using RimWorld.Planet;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_NoWorldObject : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	public SlateRef<WorldObject> worldObject;

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
		QuestPart_NoWorldObject questPart_NoWorldObject = new QuestPart_NoWorldObject();
		questPart_NoWorldObject.worldObject = worldObject.GetValue(slate);
		questPart_NoWorldObject.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_NoWorldObject);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_NoWorldObject.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_NoWorldObject);
	}
}
