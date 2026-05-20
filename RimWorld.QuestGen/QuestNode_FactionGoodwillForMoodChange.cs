using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_FactionGoodwillForMoodChange : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignal;

	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> outSignalSuccess;

	[NoTranslate]
	public SlateRef<string> outSignalFailed;

	public SlateRef<IEnumerable<Pawn>> pawns;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_FactionGoodwillForMoodChange questPart_FactionGoodwillForMoodChange = new QuestPart_FactionGoodwillForMoodChange();
			questPart_FactionGoodwillForMoodChange.pawns.AddRange(pawns.GetValue(slate));
			questPart_FactionGoodwillForMoodChange.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate));
			questPart_FactionGoodwillForMoodChange.outSignalSuccess = QuestGenUtility.HardcodedSignalWithQuestID(outSignalSuccess.GetValue(slate));
			questPart_FactionGoodwillForMoodChange.outSignalFailed = QuestGenUtility.HardcodedSignalWithQuestID(outSignalFailed.GetValue(slate));
			questPart_FactionGoodwillForMoodChange.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
			QuestGen.quest.AddPart(questPart_FactionGoodwillForMoodChange);
		}
	}
}
