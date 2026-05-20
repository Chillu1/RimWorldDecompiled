using Verse;

namespace RimWorld;

public class QuestPart_QuestEnd : QuestPart
{
	public string inSignal;

	public QuestEndOutcome? outcome;

	public bool sendLetter;

	public bool playSound;

	public override void Notify_QuestSignalReceived(Signal signal)
	{
		base.Notify_QuestSignalReceived(signal);
		if (!(signal.tag != inSignal))
		{
			QuestEndOutcome arg;
			if (outcome.HasValue)
			{
				arg = outcome.Value;
			}
			else if (!signal.args.TryGetArg("OUTCOME", out arg))
			{
				arg = QuestEndOutcome.Unknown;
			}
			quest.End(arg, sendLetter, playSound);
		}
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref inSignal, "inSignal");
		Scribe_Values.Look(ref outcome, "outcome");
		Scribe_Values.Look(ref sendLetter, "sendLetter", defaultValue: false);
		Scribe_Values.Look(ref playSound, "playSound", defaultValue: false);
	}

	public override void AssignDebugData()
	{
		base.AssignDebugData();
		inSignal = "DebugSignal" + Rand.Int;
	}
}
