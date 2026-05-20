using RimWorld;

namespace Verse.AI
{
	public class JobGiver_IdleWhileDespawned : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			return JobMaker.MakeJob(JobDefOf.IdleWhileDespawned);
		}
	}
}
