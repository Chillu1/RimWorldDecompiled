using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Hack : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Hackable);

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompHackable compHackable = t.TryGetComp<CompHackable>();
		if (compHackable == null)
		{
			return false;
		}
		if (!compHackable.Autohack && !forced)
		{
			return false;
		}
		AcceptanceReport acceptanceReport = compHackable.CanHackNow(pawn);
		if (!acceptanceReport.Accepted && !acceptanceReport.Reason.NullOrEmpty())
		{
			JobFailReason.Is(acceptanceReport.Reason);
		}
		if (!forced && !pawn.CanReserve(t))
		{
			return false;
		}
		if (forced && ModsConfig.IdeologyActive && t.def == ThingDefOf.AncientEnemyTerminal)
		{
			return false;
		}
		return acceptanceReport.Accepted;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (t.TryGetComp<CompHackable>() == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.Hack, t);
		job.playerForced = forced;
		return job;
	}
}
