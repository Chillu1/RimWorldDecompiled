using Verse;

namespace RimWorld.QuestGen
{
	public abstract class QuestNode_Filter : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public QuestNode node;

		public QuestNode elseNode;

		private const string FilterPassSignal = "FilterPass";

		private const string FilterNoPassSignal = "FilterNoPass";

		protected override bool TestRunInt(Slate slate)
		{
			if (node != null && !node.TestRun(slate))
			{
				return false;
			}
			if (elseNode != null && !elseNode.TestRun(slate))
			{
				return false;
			}
			return true;
		}

		protected abstract QuestPart_Filter MakeFilterQuestPart();

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Filter questPart_Filter = MakeFilterQuestPart();
			questPart_Filter.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
			if (node != null)
			{
				questPart_Filter.outSignal = QuestGen.GenerateNewSignal("FilterPass");
				QuestGenUtility.RunInnerNode(node, questPart_Filter.outSignal);
			}
			if (elseNode != null)
			{
				questPart_Filter.outSignalElse = QuestGen.GenerateNewSignal("FilterNoPass");
				QuestGenUtility.RunInnerNode(elseNode, questPart_Filter.outSignalElse);
			}
			QuestGen.quest.AddPart(questPart_Filter);
		}
	}
}
