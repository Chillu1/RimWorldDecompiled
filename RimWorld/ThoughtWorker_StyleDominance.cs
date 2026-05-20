using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThoughtWorker_StyleDominance : ThoughtWorker_Precept
	{
		public override string PostProcessLabel(Pawn p, string label)
		{
			return label.Formatted(p.Ideo.adjective.Named("ADJECTIVE")).CapitalizeFirst();
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			return description.Formatted(p.Ideo.adjective.Named("ADJECTIVE"), p.Ideo.thingStyleCategories.Select((ThingStyleCategoryWithPriority s) => s.category.LabelCap.Resolve()).ToCommaList(useAnd: true).Named("STYLES")).CapitalizeFirst();
		}

		protected override ThoughtState ShouldHaveThought(Pawn p)
		{
			int styleDominanceThoughtIndex = p.styleObserver.StyleDominanceThoughtIndex;
			if (styleDominanceThoughtIndex >= 0)
			{
				return ThoughtState.ActiveAtStage(styleDominanceThoughtIndex);
			}
			return false;
		}
	}
}
