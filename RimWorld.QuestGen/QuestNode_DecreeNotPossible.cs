using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_DecreeNotPossible : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public QuestNode node;

		private const string OuterNodeCompletedSignal = "OuterNodeCompleted";

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Filter_DecreeNotPossible questPart_Filter_DecreeNotPossible = new QuestPart_Filter_DecreeNotPossible();
			questPart_Filter_DecreeNotPossible.inSignal = (QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal"));
			if (node != null)
			{
				questPart_Filter_DecreeNotPossible.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_Filter_DecreeNotPossible.outSignal);
			}
			QuestGen.quest.AddPart(questPart_Filter_DecreeNotPossible);
		}
	}
}
