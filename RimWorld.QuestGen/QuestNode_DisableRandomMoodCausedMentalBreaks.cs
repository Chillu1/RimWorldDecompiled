using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_DisableRandomMoodCausedMentalBreaks : QuestNode
{
	[NoTranslate]
	public SlateRef<string> inSignalEnable;

	[NoTranslate]
	public SlateRef<string> inSignalDisable;

	public SlateRef<IEnumerable<Pawn>> pawns;

	protected override bool TestRunInt(Slate slate)
	{
		return true;
	}

	protected override void RunInt()
	{
		Slate slate = QuestGen.slate;
		IEnumerable<Pawn> value = pawns.GetValue(slate);
		if (!value.EnumerableNullOrEmpty())
		{
			QuestPart_DisableRandomMoodCausedMentalBreaks questPart_DisableRandomMoodCausedMentalBreaks = new QuestPart_DisableRandomMoodCausedMentalBreaks();
			questPart_DisableRandomMoodCausedMentalBreaks.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
			questPart_DisableRandomMoodCausedMentalBreaks.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
			questPart_DisableRandomMoodCausedMentalBreaks.pawns.AddRange(value);
			QuestGen.quest.AddPart(questPart_DisableRandomMoodCausedMentalBreaks);
		}
	}
}
