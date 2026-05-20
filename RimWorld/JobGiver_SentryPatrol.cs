using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SentryPatrol : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		CompSentryDrone comp = pawn.GetComp<CompSentryDrone>();
		if (comp == null)
		{
			return null;
		}
		if (comp.Mode != CompSentryDrone.SentryDroneMode.Patrol)
		{
			return null;
		}
		if (pawn.CurJobDef == JobDefOf.GotoPatrolDest && pawn.CurJob.jobGiver == this)
		{
			LocalTargetInfo destination = pawn.pather.Destination;
			if (destination.IsValid && destination.Cell.DistanceTo(pawn.Position) >= 1f)
			{
				return pawn.CurJob;
			}
		}
		IntVec3 nextPatrolDest = comp.GetNextPatrolDest();
		if (!nextPatrolDest.IsValid)
		{
			return null;
		}
		if (!pawn.CanReach(nextPatrolDest, PathEndMode.OnCell, Danger.Deadly))
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.GotoPatrolDest, nextPatrolDest);
	}
}
