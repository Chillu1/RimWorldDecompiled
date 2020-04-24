using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassAny : QuestPart
	{
		public List<string> inSignals = new List<string>();

		public string outSignal;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (inSignals.Contains(signal.tag))
			{
				Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
			Scribe_Values.Look(ref outSignal, "outSignal");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignals.Clear();
			for (int i = 0; i < 3; i++)
			{
				inSignals.Add("DebugSignal" + Rand.Int);
			}
			outSignal = "DebugSignal" + Rand.Int;
		}
	}
}
