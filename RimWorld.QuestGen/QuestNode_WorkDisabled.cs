using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_WorkDisabled : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<WorkTags> disabledWorkTags;

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		if (pawns.GetValue(slate) != null)
		{
			QuestPart_WorkDisabled questPart_WorkDisabled = new QuestPart_WorkDisabled();
			questPart_WorkDisabled.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			questPart_WorkDisabled.pawns.AddRange(pawns.GetValue(slate));
			questPart_WorkDisabled.disabledWorkTags = disabledWorkTags.GetValue(slate);
			QuestGen.quest.AddPart(questPart_WorkDisabled);
		}
	}

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}
}
