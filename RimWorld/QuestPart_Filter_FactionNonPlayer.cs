using Verse;

namespace RimWorld
{
	public class QuestPart_Filter_FactionNonPlayer : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			if (args.TryGetArg("FACTION", out Faction arg))
			{
				return arg != Faction.OfPlayer;
			}
			if (args.TryGetArg("SUBJECT", out Thing arg2))
			{
				return arg2.Faction != Faction.OfPlayer;
			}
			return false;
		}
	}
}
