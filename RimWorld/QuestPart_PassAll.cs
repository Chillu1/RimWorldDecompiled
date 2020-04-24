using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassAll : QuestPart
	{
		public List<string> inSignals = new List<string>();

		public string outSignal;

		private List<bool> signalsReceived = new List<bool>();

		private bool AllSignalsReceived => PassAllQuestPartUtility.AllReceived(inSignals, signalsReceived);

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			if (AllSignalsReceived)
			{
				return;
			}
			int num = inSignals.IndexOf(signal.tag);
			if (num >= 0)
			{
				while (signalsReceived.Count <= num)
				{
					signalsReceived.Add(item: false);
				}
				signalsReceived[num] = true;
				if (AllSignalsReceived)
				{
					Find.SignalManager.SendSignal(new Signal(outSignal));
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
			Scribe_Values.Look(ref outSignal, "outSignal");
			Scribe_Collections.Look(ref signalsReceived, "signalsReceived", LookMode.Value);
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
