using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_SignalFactionNonPlayer : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public QuestNode node;

		private const string OuterNodeCompletedSignal = "OuterNodeCompleted";

		protected override bool TestRunInt(Slate slate)
		{
			if (node != null)
			{
				return node.TestRun(slate);
			}
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			QuestPart_Filter_FactionNonPlayer questPart_Filter_FactionNonPlayer = new QuestPart_Filter_FactionNonPlayer();
			questPart_Filter_FactionNonPlayer.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? QuestGen.slate.Get<string>("inSignal");
			if (node != null)
			{
				questPart_Filter_FactionNonPlayer.outSignal = QuestGen.GenerateNewSignal("OuterNodeCompleted");
				QuestGenUtility.RunInnerNode(node, questPart_Filter_FactionNonPlayer.outSignal);
			}
			QuestGen.quest.AddPart(questPart_Filter_FactionNonPlayer);
		}
	}
}
