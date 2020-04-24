using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_SlaughterRandomAnimal : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			MentalState_Slaughterer mentalState_Slaughterer = pawn.MentalState as MentalState_Slaughterer;
			if (mentalState_Slaughterer != null && mentalState_Slaughterer.SlaughteredRecently)
			{
				return null;
			}
			Pawn pawn2 = SlaughtererMentalStateUtility.FindAnimal(pawn);
			if (pawn2 == null || !pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Slaughter, pawn2);
			job.ignoreDesignations = true;
			return job;
		}
	}
}
