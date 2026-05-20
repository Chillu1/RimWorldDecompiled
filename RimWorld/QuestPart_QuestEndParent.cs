namespace RimWorld;

public class QuestPart_QuestEndParent : QuestPart_QuestEnd
{
	public override void Notify_QuestSignalReceived(Signal signal)
	{
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
			quest.parent.End(arg, sendLetter, playSound);
		}
	}
}
