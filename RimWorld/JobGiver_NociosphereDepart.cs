using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_NociosphereDepart : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		CompActivity compActivity = pawn.TryGetComp<CompActivity>();
		CompNociosphere compNociosphere = pawn.TryGetComp<CompNociosphere>();
		if (compActivity == null)
		{
			Log.ErrorOnce("JobGiver_NociosphereDepart tried to execute on a pawn with no CompActivity component.", 1562529);
			return null;
		}
		if (compNociosphere == null)
		{
			Log.ErrorOnce("JobGiver_NociosphereDepart tried to execute on a pawn with no CompNociosphere component.", 83712521);
			return null;
		}
		if (!compActivity.ShouldGoPassive())
		{
			return null;
		}
		if (!compNociosphere.IsUnstable)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.NociosphereDepart);
	}
}
