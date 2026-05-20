using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class QuestPart_PassAllSequence : QuestPart
{
	public List<string> inSignals = new List<string>();

	public string outSignal;

	private int ptr = -1;

	private bool AllSignalsReceived => ptr >= inSignals.Count - 1;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		if (!AllSignalsReceived && inSignals.IndexOf(signal.tag) == ptr + 1)
		{
			ptr++;
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
		Scribe_Values.Look(ref ptr, "ptr", 0);
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
