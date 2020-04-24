using System.Collections.Generic;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_AnyPawnAlive : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<List<Pawn>> pawns;

		public QuestNode node;

		private const string OuterNodeCompletedSignal = "OuterNodeCompleted";

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Filter_AnyPawnAlive questPart_Filter_AnyPawnAlive = new QuestPart_Filter_AnyPawnAlive
			{
				inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal")),
				pawns = pawns.GetValue(slate)
			};
			if (node != null)
			{
				questPart_Filter_AnyPawnAlive.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_Filter_AnyPawnAlive.outSignal);
			}
			QuestGen.quest.AddPart(questPart_Filter_AnyPawnAlive);
		}
	}
}
