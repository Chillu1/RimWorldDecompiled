using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Deactivated : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.Deactivated, pawn.Position);
		job.forceSleep = true;
		return job;
	}
}
