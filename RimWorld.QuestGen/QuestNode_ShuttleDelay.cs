using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_ShuttleDelay : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalComplete;

	public SlateRef<int> delayTicks;

	public SlateRef<IEnumerable<Pawn>> lodgers;

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
		QuestPart_ShuttleDelay questPart_ShuttleDelay = new QuestPart_ShuttleDelay();
		questPart_ShuttleDelay.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_ShuttleDelay.delayTicks = delayTicks.GetValue(slate);
		if (lodgers.GetValue(slate) != null)
		{
			questPart_ShuttleDelay.lodgers.AddRange(lodgers.GetValue(slate));
		}
		questPart_ShuttleDelay.expiryInfoPart = "ShuttleArrivesIn".Translate();
		questPart_ShuttleDelay.expiryInfoPartTip = "ShuttleArrivesOn".Translate();
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_ShuttleDelay);
		}
		if (!outSignalComplete.GetValue(slate).NullOrEmpty())
		{
			questPart_ShuttleDelay.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
		}
		QuestGen.quest.AddPart(questPart_ShuttleDelay);
	}
}
