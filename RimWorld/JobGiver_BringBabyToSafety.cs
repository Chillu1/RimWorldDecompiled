using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_BringBabyToSafety : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Pawn pawn2 = ChildcareUtility.FindUnsafeBaby(pawn, AutofeedMode.Urgent);
		if (pawn2 == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.BringBabyToSafetyUnforced, pawn2);
		job.count = 1;
		return job;
	}
}
