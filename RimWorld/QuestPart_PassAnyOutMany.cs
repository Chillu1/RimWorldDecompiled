using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassAnyOutMany : QuestPart
	{
		public List<string> inSignals = new List<string>();

		public List<string> outSignals = new List<string>();

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (inSignals.Contains(signal.tag))
			{
				for (int i = 0; i < outSignals.Count; i++)
				{
					Find.SignalManager.SendSignal(new Signal(outSignals[i], signal.args));
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
			Scribe_Collections.Look(ref outSignals, "outSignals", LookMode.Value);
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignals.Clear();
			outSignals.Clear();
			for (int i = 0; i < 3; i++)
			{
				inSignals.Add("DebugSignal" + Rand.Int);
				outSignals.Add("DebugSignal" + Rand.Int);
			}
		}
	}
}
