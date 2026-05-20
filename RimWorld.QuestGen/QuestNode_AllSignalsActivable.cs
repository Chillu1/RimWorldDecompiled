using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_AllSignalsActivable : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	[NoTranslate]
	public SlateRef<IEnumerable<string>> inSignals;

	[NoTranslate]
	public SlateRef<IEnumerable<string>> outSignals;

	public QuestNode node;

	public SlateRef<QuestPart.SignalListenMode?> signalListenMode;

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
		if (((outSignals.GetValue(slate) != null) ? outSignals.GetValue(slate).Count() : 0) + ((node != null) ? 1 : 0) == 0)
		{
			return;
		}
		QuestPart_PassAllActivable questPart_PassAllActivable = new QuestPart_PassAllActivable();
		QuestGen.quest.AddPart(questPart_PassAllActivable);
		questPart_PassAllActivable.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
		questPart_PassAllActivable.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		foreach (string item in inSignals.GetValue(slate))
		{
			questPart_PassAllActivable.inSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
		}
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_PassAllActivable.OutSignalCompleted);
		}
		IEnumerable<string> value = outSignals.GetValue(slate);
		if (value != null)
		{
			foreach (string item2 in value)
			{
				questPart_PassAllActivable.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(item2));
			}
		}
		questPart_PassAllActivable.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
	}
}
