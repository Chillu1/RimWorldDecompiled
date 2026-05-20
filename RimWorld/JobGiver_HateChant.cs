using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_HateChant : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		return JobMaker.MakeJob(JobDefOf.HateChanting);
	}
}
