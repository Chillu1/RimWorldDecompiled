using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_Leave : QuestNode
	{
		public SlateRef<string> inSignal;

		public SlateRef<IEnumerable<Pawn>> pawns;

		public SlateRef<bool?> sendStandardLetter;

		public SlateRef<bool?> leaveOnCleanup;

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
				questPart_Leave.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
				questPart_Leave.pawns.AddRange(value);
				questPart_Leave.sendStandardLetter = (sendStandardLetter.GetValue(slate) ?? questPart_Leave.sendStandardLetter);
				questPart_Leave.leaveOnCleanup = (leaveOnCleanup.GetValue(slate) ?? questPart_Leave.leaveOnCleanup);
				QuestGen.quest.AddPart(questPart_Leave);
			}
		}
	}
}
