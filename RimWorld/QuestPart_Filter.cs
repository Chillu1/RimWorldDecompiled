using Verse;

namespace RimWorld
{
	public abstract class QuestPart_Filter : QuestPart
	{
		public string inSignal;

		public string outSignal;

		protected abstract bool Pass(SignalArgs args);

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal && Pass(signal.args))
			{
				Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignal, "outSignal");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			outSignal = "DebugSignal" + Rand.Int;
		}
	}
}
