using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ReleaseEntity : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = AnomalyUtility.FindEntityOnPlatform(pawn.Map, EntityQueryType.ForRelease);
		Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)(pawn2?.ParentHolder);
		if (building_HoldingPlatform == null || !pawn.CanReserveAndReach(building_HoldingPlatform, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.ReleaseEntity, building_HoldingPlatform, pawn2).WithCount(1);
		job.ignoreDesignations = true;
		return job;
	}
}
