using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_StartRoaming : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!MentalStateWorker_Roaming.CanRoamNow(pawn))
			{
				return null;
			}
			if (!pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Roaming))
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.Wait, 10);
		}
	}
}
