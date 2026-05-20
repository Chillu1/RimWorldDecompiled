using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_AnimaLinking : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			if (!pawn.CanReserveAndReach(duty.focus, PathEndMode.OnCell, Danger.Deadly))
			{
				return null;
			}
			CompPsylinkable compPsylinkable = duty.focusSecond.Thing?.TryGetComp<CompPsylinkable>();
			if (compPsylinkable == null || !compPsylinkable.CanPsylink(pawn).Accepted)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.LinkPsylinkable, duty.focusSecond, duty.focus);
		}
	}
}
