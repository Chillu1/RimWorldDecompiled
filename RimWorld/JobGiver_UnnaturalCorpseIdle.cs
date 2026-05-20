using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseIdle : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return null;
		}
		if (!Find.Anomaly.TryGetUnnaturalCorpseTrackerForAwoken(pawn, out var tracker))
		{
			return null;
		}
		Pawn haunted = tracker.Haunted;
		if (haunted.DestroyedOrNull())
		{
			return null;
		}
		if (haunted.Dead)
		{
			return JobMaker.MakeJob(JobDefOf.Wait, 300);
		}
		return null;
	}
}
