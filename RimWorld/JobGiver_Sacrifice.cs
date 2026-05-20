using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_Sacrifice : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Pawn pawn2 = pawn.mindState.duty.focusSecond.Pawn;
			if (!pawn.CanReserveAndReach(pawn2, PathEndMode.ClosestTouch, Danger.None))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Sacrifice, pawn2, pawn.mindState.duty.focus);
		}
	}
}
