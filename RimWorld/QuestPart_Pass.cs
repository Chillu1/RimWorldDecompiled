using Verse;

namespace RimWorld
{
	public class QuestPart_Pass : QuestPart
	{
		public string inSignal;

		public string outSignal;

		public QuestEndOutcome? outSignalOutcomeArg;

		public override void Notify_QuestSignalReceived(Signal signal)
		{
			base.Notify_QuestSignalReceived(signal);
			if (signal.tag == inSignal)
			{
				SignalArgs args = new SignalArgs(signal.args);
				if (outSignalOutcomeArg.HasValue)
				{
					args.Add(outSignalOutcomeArg.Value.Named("OUTCOME"));
				}
				Find.SignalManager.SendSignal(new Signal(outSignal, args));
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref inSignal, "inSignal");
			Scribe_Values.Look(ref outSignal, "outSignal");
			Scribe_Values.Look(ref outSignalOutcomeArg, "outSignalOutcomeArg");
		}

		public override void AssignDebugData()
		{
			base.AssignDebugData();
			inSignal = "DebugSignal" + Rand.Int;
			outSignal = "DebugSignal" + Rand.Int;
		}
	}
}
