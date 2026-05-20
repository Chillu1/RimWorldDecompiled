using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AnySignal : QuestNode
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
			QuestPart_PassAny questPart_PassAny = new QuestPart_PassAny();
			foreach (string item in inSignals.GetValue(slate))
			{
				questPart_PassAny.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
			}
			if (node != null)
			{
				questPart_PassAny.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_PassAny.outSignal);
			}
			else
			{
				questPart_PassAny.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.GetValue(slate).First());
			}
			questPart_PassAny.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
			QuestGen.quest.AddPart(questPart_PassAny);
			return;
		}
		}
		QuestPart_PassAnyOutMany questPart_PassAnyOutMany = new QuestPart_PassAnyOutMany();
		foreach (string item2 in inSignals.GetValue(slate))
		{
			questPart_PassAnyOutMany.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item2));
		}
		if (node != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassAnyOutMany.outSignals.Add(text);
			QuestGenUtility.RunInnerNode(node, text);
		}
		foreach (string item3 in outSignals.GetValue(slate))
		{
			questPart_PassAnyOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item3));
		}
		questPart_PassAnyOutMany.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		QuestGen.quest.AddPart(questPart_PassAnyOutMany);
	}
}
