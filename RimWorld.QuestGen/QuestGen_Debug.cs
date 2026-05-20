namespace RimWorld.QuestGen;

public static class QuestGen_Debug
{
	public static void Log(this Quest quest, string message, string inSignal = null, QuestPart.SignalListenMode signalListenMode = QuestPart.SignalListenMode.OngoingOnly)
	{
		QuestPart_Log questPart_Log = new QuestPart_Log();
		questPart_Log.inSignal = QuestGenUtility.HardcodedSignalWithQuestID(inSignal) ?? QuestGen.slate.Get<string>("inSignal");
		questPart_Log.signalListenMode = signalListenMode;
		questPart_Log.message = message;
		quest.AddPart(questPart_Log);
	}
}
