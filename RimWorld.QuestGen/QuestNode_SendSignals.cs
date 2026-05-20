using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_SendSignals : QuestNode
{
	[NoTranslate]
	public SlateRef<IEnumerable<string>> outSignals;

	[NoTranslate]
	public SlateRef<string> outSignalsFormat;

	public SlateRef<int> outSignalsFormattedCount;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		IEnumerable<string> enumerable = Enumerable.Empty<string>();
		if (outSignals.GetValue(slate) != null)
		{
			enumerable = enumerable.Concat(outSignals.GetValue(slate));
		}
		if (outSignalsFormattedCount.GetValue(slate) > 0)
		{
			for (int i = 0; i < outSignalsFormattedCount.GetValue(slate); i++)
			{
				enumerable = enumerable.Concat(Gen.YieldSingle(outSignalsFormat.GetValue(slate).Formatted(i.Named("INDEX")).ToString()));
			}
		}
		if (enumerable.EnumerableNullOrEmpty())
		{
			return;
		}
		if (enumerable.Count() == 1)
		{
			QuestPart_Pass questPart_Pass = new QuestPart_Pass();
			questPart_Pass.inSignal = QuestGen.slate.Get<string>("inSignal");
			questPart_Pass.outSignal = QuestGenUtility.HardcodedSignalWithQuestID(enumerable.First());
			QuestGen.quest.AddPart(questPart_Pass);
			return;
		}
		QuestPart_PassOutMany questPart_PassOutMany = new QuestPart_PassOutMany();
		questPart_PassOutMany.inSignal = QuestGen.slate.Get<string>("inSignal");
		foreach (string item in enumerable)
		{
			questPart_PassOutMany.outSignals.Add(QuestGenUtility.HardcodedSignalWithQuestID(item));
		}
		QuestGen.quest.AddPart(questPart_PassOutMany);
	}
}
