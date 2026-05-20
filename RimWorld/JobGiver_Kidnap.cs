using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_Kidnap : ThinkNode_JobGiver
{
	public const float VictimSearchRadiusInitial = 8f;

	private const float VictimSearchRadiusOngoing = 18f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!RCellFinder.TryFindBestExitSpot(pawn, out var spot))
		{
			return null;
		}
		if (KidnapAIUtility.TryFindGoodKidnapVictim(pawn, 18f, out var victim) && !GenAI.InDangerousCombat(pawn))
		{
			Job job = JobMaker.MakeJob(JobDefOf.Kidnap);
			job.targetA = victim;
			job.targetB = spot;
			job.count = 1;
			return job;
		}
		return null;
	}
}
