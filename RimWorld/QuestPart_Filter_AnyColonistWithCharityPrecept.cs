using System.Linq;

namespace RimWorld
{
	public class QuestPart_Filter_AnyColonistWithCharityPrecept : QuestPart_Filter
	{
		protected override bool Pass(SignalArgs args)
		{
			return IdeoUtility.AllColonistsWithCharityPrecept().Any();
		}
	}
}
