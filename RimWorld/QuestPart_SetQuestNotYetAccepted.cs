using Verse;

namespace RimWorld
{
	public class QuestPart_SetQuestNotYetAccepted : QuestPart
	{
		public string inSignal;

		public bool revertOngoingQuestAfterLoad;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				quest.SetNotYetAccepted();
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref revertOngoingQuestAfterLoad, "revertOngoingQuestAfterLoad", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && revertOngoingQuestAfterLoad && quest.State == QuestState.Ongoing)
			{
				quest.SetNotYetAccepted();
			}
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
		}
	}
}
