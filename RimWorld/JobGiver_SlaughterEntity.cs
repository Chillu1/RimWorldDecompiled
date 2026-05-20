using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_SlaughterEntity : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.MentalState is MentalState_SlaughterThing { SlaughteredRecently: not false })
		{
			return null;
		}
		Building_HoldingPlatform building_HoldingPlatform = (Building_HoldingPlatform)AnomalyUtility.FindEntityOnPlatform(pawn.Map, EntityQueryType.ForSlaughter).ParentHolder;
		if (building_HoldingPlatform == null || !pawn.CanReserveAndReach(building_HoldingPlatform, PathEndMode.ClosestTouch, Danger.Deadly))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.ExecuteEntity, building_HoldingPlatform);
		job.ignoreDesignations = true;
		return job;
	}
}
