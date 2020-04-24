using Verse;

namespace RimWorld
{
	public class QuestPart_Log : QuestPart
	{
		public string inSignal;

		public string message;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				Log.Message(signal.args.GetFormattedText(message));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref message, "message");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			message = "Dev: Test";
		}
	}
}
