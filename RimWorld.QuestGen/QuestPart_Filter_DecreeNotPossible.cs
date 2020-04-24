using System.Linq;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestPart_Filter_DecreeNotPossible : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			if (!args.TryGetArg("SUBJECT", out Pawn arg))
			{
				return false;
			}
			if (arg.royalty != null)
			{
				return !arg.royalty.PossibleDecreeQuests.Contains(quest.root);
			}
			return true;
		}
	}
}
