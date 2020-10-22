using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_ReachOutside : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Room room = pawn.GetRoom();
			if (room.PsychologicallyOutdoors && room.TouchesMapEdge)
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
}
