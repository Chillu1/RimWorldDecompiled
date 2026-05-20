using Verse;

namespace RimWorld;

public abstract class QuestPart_Filter : QuestPart
{
	public string inSignal;

	public string outSignal;

	public string outSignalElse;

	protected abstract bool Pass(SignalArgs args);

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag == inSignal))
		{
			return;
		}
		if (Pass(signal.args))
		{
			if (!outSignal.NullOrEmpty())
			{
				Find.SignalManager.SendSignal(new Signal(outSignal, signal.args));
			}
		}
		else if (!outSignalElse.NullOrEmpty())
		{
			Find.SignalManager.SendSignal(new Signal(outSignalElse, signal.args));
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref outSignal, "outSignal");
		Scribe_Values.Look(ref outSignalElse, "outSignalElse");
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
		outSignal = "DebugSignal" + Rand.Int;
	}
}
