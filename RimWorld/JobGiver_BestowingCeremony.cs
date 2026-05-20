using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_BestowingCeremony : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.BestowingCeremony, duty.focus.Pawn, duty.focusSecond);
		}
	}
}
