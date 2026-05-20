using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GuardianShipDelay : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignalEnable;

		[NoTranslate]
		public SlateRef<string> inSignalDisable;

		[NoTranslate]
		public SlateRef<string> outSignalComplete;

		public SlateRef<Pawn> pawn;

		public SlateRef<int> delayTicks;

		public QuestNode node;

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
			QuestPart_GuardianShipDelay questPart_GuardianShipDelay = new QuestPart_GuardianShipDelay();
			questPart_GuardianShipDelay.pawn = pawn.GetValue(slate);
			questPart_GuardianShipDelay.delayTicks = delayTicks.GetValue(slate);
			questPart_GuardianShipDelay.inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalEnable.GetValue(slate)) ?? slate.Get<string>("inSignal");
			questPart_GuardianShipDelay.inSignalDisable = QuestGenUtility.HardcodedSignalWithQuestID(inSignalDisable.GetValue(slate));
			questPart_GuardianShipDelay.reactivatable = true;
			if (node != null)
			{
				QuestGenUtility.RunInnerNode(node, questPart_GuardianShipDelay);
			}
			if (!outSignalComplete.GetValue(slate).NullOrEmpty())
			{
				questPart_GuardianShipDelay.outSignalsCompleted.Add(QuestGenUtility.HardcodedSignalWithQuestID(outSignalComplete.GetValue(slate)));
			}
			QuestGen.quest.AddPart(questPart_GuardianShipDelay);
		}
	}
}
