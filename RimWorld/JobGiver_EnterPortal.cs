using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_EnterPortal : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.mindState.duty.focus.Thing is MapPortal mapPortal) || mapPortal.Map != pawn.Map || !pawn.CanReach(mapPortal, PathEndMode.Touch, Danger.Deadly))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.EnterPortal, mapPortal);
		job.locomotionUrgency = PawnUtility.ResolveLocomotion(pawn, LocomotionUrgency.Jog);
		return job;
	}
}
