using Verse;

namespace RimWorld
{
	public class QuestPart_RecordHistoryEvent : QuestPart
	{
		public string inSignal;

		public HistoryEventDef historyEvent;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				Find.HistoryEventsManager.RecordEvent(new HistoryEvent(historyEvent));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Defs.Look(ref historyEvent, "historyEvent");
		}
	}
}
