using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ShuttleLeaveDelay : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	[NoTranslate]
	public SlateRef<IEnumerable<string>> inSignalsDisable;

	public SlateRef<int> delayTicks;

	public SlateRef<Thing> shuttle;

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
		QuestPart_ShuttleLeaveDelay questPart_ShuttleLeaveDelay = new QuestPart_ShuttleLeaveDelay();
		questPart_ShuttleLeaveDelay.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_ShuttleLeaveDelay.delayTicks = delayTicks.GetValue(slate);
		questPart_ShuttleLeaveDelay.shuttle = shuttle.GetValue(slate);
		questPart_ShuttleLeaveDelay.expiryInfoPart = "ShuttleDepartsIn".Translate();
		questPart_ShuttleLeaveDelay.expiryInfoPartTip = "ShuttleDepartsOn".Translate();
		if (inSignalsDisable.GetValue(slate) != null)
		{
			foreach (string item in inSignalsDisable.GetValue(slate))
			{
				questPart_ShuttleLeaveDelay.inSignalsDisable.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
			}
		}
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_ShuttleLeaveDelay);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_ShuttleLeaveDelay.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_ShuttleLeaveDelay);
	}
}
