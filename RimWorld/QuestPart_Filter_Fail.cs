namespace RimWorld
{
	public class QuestPart_Filter_Fail : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			if (args.TryGetArg("OUTCOME", out QuestEndOutcome arg))
			{
				return arg == QuestEndOutcome.Fail;
			}
			return false;
		}
	}
}
