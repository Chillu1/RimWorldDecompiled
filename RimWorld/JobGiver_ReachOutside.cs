using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ReachOutside : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		District district = pawn.GetDistrict();
		if (district.Room.PsychologicallyOutdoors && district.TouchesMapEdge)
		{
			return null;
		}
		if (!pawn.CanReachMapEdge())
		{
			return null;
		}
		if (!RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out var result))
		{
			return null;
		}
		if (result == pawn.Position)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.Goto, result);
	}
}
