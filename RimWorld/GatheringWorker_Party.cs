using Verse;
using Verse.AI.Group;

namespace RimWorld;

public class GatheringWorker_Party : GatheringWorker
{
	protected override LordJob CreateLordJob(IntVec3 spot, Pawn organizer)
	{
		return new LordJob_Joinable_Party(spot, organizer, def);
	}

	protected override bool TryFindGatherSpot(Pawn organizer, out IntVec3 spot)
	{
		return RCellFinder.TryFindGatheringSpot(organizer, def, ignoreRequiredColonistCount: false, out spot);
	}
}
