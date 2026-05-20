namespace RimWorld.QuestGen
{
	public static class QuestGen_HistoryEvents
	{
		public static QuestPart_RecordHistoryEvent RecordHistoryEvent(this Quest quest, HistoryEventDef def, string inSignal = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
		{
			QuestPart_RecordHistoryEvent questPart_RecordHistoryEvent = new QuestPart_RecordHistoryEvent();
			questPart_RecordHistoryEvent.inSignal = inSignal ?? QuestGen.slate.Get<string>("inSignal");
			questPart_RecordHistoryEvent.historyEvent = def;
			questPart_RecordHistoryEvent.signalListenMode = signalListenMode;
			quest.AddPart(questPart_RecordHistoryEvent);
			return questPart_RecordHistoryEvent;
		}
	}
}
