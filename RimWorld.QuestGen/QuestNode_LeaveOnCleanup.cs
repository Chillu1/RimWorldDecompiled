using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen;

public class QuestNode_LeaveOnCleanup : QuestNode
{
	public SlateRef<IEnumerable<Pawn>> pawns;

	public SlateRef<bool?> sendStandardLetter;

	[NoTranslate]
	public SlateRef<string> inSignalRemovePawn;

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
			QuestPart_Leave questPart_Leave = new QuestPart_Leave();
			questPart_Leave.pawns.AddRange(value);
			questPart_Leave.sendStandardLetter = sendStandardLetter.GetValue(slate) ?? questPart_Leave.sendStandardLetter;
			questPart_Leave.leaveOnCleanup = true;
			questPart_Leave.inSignalRemovePawn = inSignalRemovePawn.GetValue(slate);
			QuestGen.quest.AddPart(questPart_Leave);
		}
	}
}
