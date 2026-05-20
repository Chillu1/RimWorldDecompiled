using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ThoughtWorker_ThroneRoomRequirementsNotMet : ThoughtWorker_RoomRequirementsNotMet
{
	protected override IEnumerable<string> UnmetRequirements(Pawn p)
	{
		return p.royalty.GetUnmetThroneroomRequirements(includeOnGracePeriod: false);
	}

	public override string PostProcessDescription(Pawn p, string description)
	{
		return description.Formatted(UnmetRequirements(p).ToLineList("- "), p.royalty.HighestTitleWithThroneRoomRequirements().Named("TITLE"));
	}
}
