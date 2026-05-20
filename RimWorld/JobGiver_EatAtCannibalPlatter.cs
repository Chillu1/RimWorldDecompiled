using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class JobGiver_EatAtCannibalPlatter : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		LordJob_Ritual lordJob_Ritual = pawn.GetLord().LordJob as LordJob_Ritual;
		if (!GatheringsUtility.TryFindRandomCellAroundTarget(pawn, lordJob_Ritual.selectedTarget.Thing, out var result) && !GatheringsUtility.TryFindRandomCellInGatheringArea(pawn, CellValid, out result))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.EatAtCannibalPlatter, lordJob_Ritual.selectedTarget.Thing, result);
		job.doUntilGatheringEnded = true;
		job.expiryInterval = lordJob_Ritual.DurationTicks;
		return job;
		bool CellValid(IntVec3 c)
		{
			foreach (IntVec3 item in GenRadial.RadialCellsAround(c, 1f, useCenter: true))
			{
				if (!pawn.CanReserveSittableOrSpot(item))
				{
					return false;
				}
			}
			return true;
		}
	}
}
