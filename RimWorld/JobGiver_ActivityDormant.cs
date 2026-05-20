using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ActivityDormant : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		Job job = JobMaker.MakeJob(JobDefOf.ActivityDormant);
		job.forceSleep = true;
		return job;
	}
}
