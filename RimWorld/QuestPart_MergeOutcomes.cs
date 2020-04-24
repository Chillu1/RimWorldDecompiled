using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class QuestPart_MergeOutcomes : QuestPart
	{
		public List<string> inSignals = new List<string>();

		public string outSignal;

		private List<QuestEndOutcome?> signalsReceived = new List<QuestEndOutcome?>();

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			int num = inSignals.IndexOf(signal.tag);
			if (num >= 0)
			{
				while (signalsReceived.Count <= num)
				{
					signalsReceived.Add(null);
				}
				signalsReceived[num] = GetOutcome(signal.args);
				CheckEnd();
			}
		}

		private QuestEndOutcome GetOutcome(SignalArgs args)
		{
			if (args.TryGetArg("OUTCOME", out QuestEndOutcome arg))
			{
				return arg;
			}
			return QuestEndOutcome.Unknown;
		}

		private void CheckEnd()
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = inSignals.Count == signalsReceived.Count;
			for (int i = 0; i < signalsReceived.Count; i++)
			{
				if (!signalsReceived[i].HasValue)
				{
					flag3 = false;
				}
				else if (signalsReceived[i].Value == QuestEndOutcome.Success)
				{
					flag = true;
				}
				else if (signalsReceived[i].Value == QuestEndOutcome.Fail)
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				Find.SignalManager.SendSignal(new Signal(outSignal, QuestEndOutcome.Fail.Named("OUTCOME")));
			}
			else if (flag3)
			{
				if (flag)
				{
					Find.SignalManager.SendSignal(new Signal(outSignal, QuestEndOutcome.Success.Named("OUTCOME")));
				}
				else
				{
					Find.SignalManager.SendSignal(new Signal(outSignal, QuestEndOutcome.Unknown.Named("OUTCOME")));
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
