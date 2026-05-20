using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AllSignals : QuestNode
{
	[NoTranslate]
	public SlateRef<IEnumerable<string>> inSignals;

	[NoTranslate]
	public SlateRef<IEnumerable<string>> outSignals;

	public QuestNode node;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

	private const string OuterNodeCompletedSignal = "OuterNodeCompleted";

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
		switch (((outSignals.GetValue(slate) != null) ? outSignals.GetValue(slate).Count() : 0) + ((node != null) ? 1 : 0))
		{
		case 0:
			return;
		case 1:
		{
			QuestPart_PassAll questPart_PassAll = new QuestPart_PassAll();
			foreach (string item in inSignals.GetValue(slate))
			{
				questPart_PassAll.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
			}
			if (node != null)
			{
				questPart_PassAll.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_PassAll.outSignal);
			}
			else
			{
				questPart_PassAll.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.GetValue(slate).First());
			}
			questPart_PassAll.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
			QuestGen.quest.AddPart(questPart_PassAll);
			return;
		}
		}
		QuestPart_PassAllOutMany questPart_PassAllOutMany = new QuestPart_PassAllOutMany();
		foreach (string item2 in inSignals.GetValue(slate))
		{
			questPart_PassAllOutMany.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item2));
		}
		if (node != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassAllOutMany.outSignals.Add(text);
			QuestGenUtility.RunInnerNode(node, text);
		}
		foreach (string item3 in outSignals.GetValue(slate))
		{
			questPart_PassAllOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item3));
		}
		questPart_PassAllOutMany.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		QuestGen.quest.AddPart(questPart_PassAllOutMany);
	}
}
