using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SelfShutdown : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (RCellFinder.TryFindNearbyMechSelfShutdownSpot(pawn.Position, pawn, pawn.Map, out var result, allowForbidden: true))
			{
				Job job = JobMaker.MakeJob(JobDefOf.SelfShutdown, result);
				job.forceSleep = true;
				return job;
			}
			return null;
		}
	}
}
