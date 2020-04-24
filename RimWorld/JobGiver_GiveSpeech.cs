using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GiveSpeech : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			Building_Throne building_Throne = duty.focusSecond.Thing as Building_Throne;
			if (building_Throne == null || building_Throne.AssignedPawn != pawn)
			{
				return null;
			}
			if (!pawn.CanReach(building_Throne, PathEndMode.InteractionCell, Danger.None))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.GiveSpeech, duty.focusSecond);
		}
	}
}
