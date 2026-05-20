using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SignalActivable : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	[NoTranslate]
	[TranslationHandle(Priority = 100)]
	public SlateRef<string> inSignal;

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
		QuestPart_PassActivable questPart_PassActivable = new QuestPart_PassActivable();
		QuestGen.quest.AddPart(questPart_PassActivable);
		questPart_PassActivable.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
		questPart_PassActivable.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
		questPart_PassActivable.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
		if (node != null)
		{
			QuestGenUtility.RunInnerNode(node, questPart_PassActivable.OutSignalCompleted);
		}
		IEnumerable<string> value = outSignals.GetValue(slate);
		if (value != null)
		{
			foreach (string item in value)
			{
				questPart_PassActivable.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
			}
		}
		questPart_PassActivable.signalListenMode = signalListenMode.GetValue(slate).GetValueOrDefault();
	}
}
