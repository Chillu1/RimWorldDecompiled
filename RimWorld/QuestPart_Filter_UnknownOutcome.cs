namespace RimWorld
{
	public class QuestPart_Filter_UnknownOutcome : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			if (args.TryGetArg("OUTCOME", out QuestEndOutcome arg))
			{
				return arg == QuestEndOutcome.Unknown;
			}
			return true;
		}
	}
}
