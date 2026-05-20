using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_BetrayMTB : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignalEnable;

		[NoTranslate]
		public SlateRef<string> inSignalDisable;

		[NoTranslate]
		public SlateRef<string> outSignal;

		public SlateRef<IEnumerable<Pawn>> pawns;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			if (!pawns.GetValue(slate).EnumerableNullOrEmpty())
			{
				QuestPart_BetrayMTB questPart_BetrayMTB = new QuestPart_BetrayMTB();
				questPart_BetrayMTB.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
				questPart_BetrayMTB.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
				questPart_BetrayMTB.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignal.GetValue(slate)));
				questPart_BetrayMTB.pawns.AddRange(pawns.GetValue(slate));
				QuestGen.quest.AddPart(questPart_BetrayMTB);
			}
		}
	}
}
