using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_TakeBioferriteOutOfHarvester : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.BioferriteHarvester);

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Building_BioferriteHarvester { unloadingEnabled: not false, ReadyForHauling: not false }))
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.TakeBioferriteOutOfHarvester, t);
	}
}
