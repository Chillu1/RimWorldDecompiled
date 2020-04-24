using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_PrepareCaravan_GatherItems : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return null;
			}
			Lord lord = pawn.GetLord();
			Thing thing = GatherItemsForCaravanUtility.FindThingToHaul(pawn, lord);
			if (thing == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.PrepareCaravan_GatherItems, thing);
			job.lord = lord;
			return job;
		}
	}
}
