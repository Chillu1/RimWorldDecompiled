using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_RecordHistoryEvent : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> inSignal;

		public SlateRef<HistoryEventDef> historyDef;

		protected override bool TestRunInt(Slate slate)
		{
			return true;
		}

		protected override void RunInt()
		{
			Slate slate = QuestGen.slate;
			Quest quest = QuestGen.quest;
			HistoryEventDef value = historyDef.GetValue(slate);
			if (value != null)
			{
				QuestPart_RecordHistoryEvent questPart_RecordHistoryEvent = new QuestPart_RecordHistoryEvent();
				questPart_RecordHistoryEvent.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal.GetValue(slate)) ?? slate.Get<string>("inSignal");
				questPart_RecordHistoryEvent.historyEvent = value;
				quest.AddPart(questPart_RecordHistoryEvent);
			}
		}
	}
}
