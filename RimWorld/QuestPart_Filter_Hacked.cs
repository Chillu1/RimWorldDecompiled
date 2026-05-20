using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_Hacked : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			if (args.TryGetArg("SUBJECT", out Thing arg))
			{
				CompHackable compHackable = arg.TryGetComp<CompHackable>();
				if (compHackable != null)
				{
					return compHackable.IsHacked;
				}
			}
			return false;
		}
	}
}
