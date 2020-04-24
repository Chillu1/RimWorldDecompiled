using Verse;

namespace RimWorld
{
	public class ThoughtWorker_NoPersonalBedroom : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.royalty == null || p.MapHeld == null || !p.MapHeld.IsPlayerHome || p.royalty.HighestTitleWithBedroomRequirements() == null)
			{
				return false;
			}
			return !p.royalty.HasPersonalBedroom();
		}

		public override string PostProcessDescription(Pawn p, string description)
		{
			return description.Formatted(p.royalty.HighestTitleWithBedroomRequirements().def.GetLabelCapFor(p).Named("TITLE"));
		}
	}
}
