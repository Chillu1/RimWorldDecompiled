using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_PassAllActivable : QuestPartActivable
	{
		public List<string> inSignals = new List<string>();

		private List<bool> signalsReceived = new List<bool>();

		private bool AllSignalsReceived
		{
			get
			{
				if (inSignals.Count != signalsReceived.Count)
				{
					return false;
				}
				for (int i = 0; i < signalsReceived.Count; i++)
				{
					if (!signalsReceived[i])
					{
						return false;
					}
				}
				return true;
			}
		}

		protected override void Enable(SignalArgs receivedArgs)
		{
			signalsReceived.Clear();
			base.Enable(receivedArgs);
		}

		protected override void ProcessQuestSignal(Signal signal)
		{
			base.ProcessQuestSignal(signal);
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
					Complete();
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Collections.Look(ref inSignals, "inSignals", LookMode.Value);
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
		}
	}
}
