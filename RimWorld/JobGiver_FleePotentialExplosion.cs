using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleePotentialExplosion : ThinkNode_JobGiver
{
	public const float FleeDist = 9f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if ((int)pawn.RaceProps.intelligence < 2)
		{
			return null;
		}
		if (pawn.mindState.knownExploder == null)
		{
			return null;
		}
		if (!pawn.mindState.knownExploder.Spawned)
		{
			pawn.mindState.knownExploder = null;
			return null;
		}
		if (pawn.Downed && !pawn.health.CanCrawl)
		{
			return null;
		}
		if (PawnUtility.PlayerForcedJobNowOrSoon(pawn))
		{
			return null;
		}
		Thing knownExploder = pawn.mindState.knownExploder;
		if ((float)(pawn.Position - knownExploder.Position).LengthHorizontalSquared > 81f)
		{
			return null;
		}
		if (!RCellFinder.TryFindDirectFleeDestination(knownExploder.Position, 9f, pawn, out var result))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Goto, result);
		job.locomotionUrgency = LocomotionUrgency.Sprint;
		return job;
	}
}
