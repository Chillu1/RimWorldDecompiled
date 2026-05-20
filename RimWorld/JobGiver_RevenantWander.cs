using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_RevenantWander : ThinkNode_JobGiver
{
	public static float WanderDist = 30f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		CellFinder.TryFindRandomReachableNearbyCell(pawn.Position, pawn.Map, WanderDist, TraverseParms.For(TraverseMode.PassDoors), (IntVec3 x) => x.Standable(pawn.Map), null, out var result);
		Job job = JobMaker.MakeJob(JobDefOf.RevenantWander, result);
		job.locomotionUrgency = LocomotionUrgency.Walk;
		return job;
	}
}
