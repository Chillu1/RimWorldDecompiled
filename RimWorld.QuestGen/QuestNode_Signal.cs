using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_Signal : QuestNode
{
	[NoTranslate]
	[TranslationHandle(Priority = 100)]
	public SlateRef<string> inSignal;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

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
			QuestPart_Pass questPart_Pass = new QuestPart_Pass();
			questPart_Pass.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
			questPart_Pass.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
			if (node != null)
			{
				questPart_Pass.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_Pass.outSignal);
			}
			else
			{
				questPart_Pass.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(outSignals.GetValue(slate).First());
			}
			questPart_Pass.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
			QuestGen.quest.AddPart(questPart_Pass);
			return;
		}
		}
		QuestPart_PassOutMany questPart_PassOutMany = new QuestPart_PassOutMany();
		questPart_PassOutMany.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
		questPart_PassOutMany.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		if (node != null)
		{
			string text = QuestGen.GenerateNewSignal("OuterNodeCompleted");
			questPart_PassOutMany.outSignals.Add(text);
			QuestGenUtility.RunInnerNode(node, text);
		}
		foreach (string item in outSignals.GetValue(slate))
		{
			questPart_PassOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
		}
		questPart_PassOutMany.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
		QuestGen.quest.AddPart(questPart_PassOutMany);
	}
}
