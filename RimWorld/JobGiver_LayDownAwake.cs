using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_LayDownAwake : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.CanReach(pawn.mindState.duty.focusThird, PathEndMode.Touch, Danger.None))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.LayDownAwake, pawn.mindState.duty.focusThird);
		}
	}
}
