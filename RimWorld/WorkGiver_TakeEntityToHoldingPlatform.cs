using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_TakeEntityToHoldingPlatform : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.HoldingPlatformTarget);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			JobFailReason.Is("IncapableOfCapacity".Translate(PawnCapacityDefOf.Manipulation.label).CapitalizeFirst());
			return false;
		}
		CompHoldingPlatformTarget compHoldingPlatformTarget = t.TryGetComp<CompHoldingPlatformTarget>();
		if (compHoldingPlatformTarget?.targetHolder == null || compHoldingPlatformTarget.targetHolder.Destroyed || compHoldingPlatformTarget.targetHolder.MapHeld != t.MapHeld || compHoldingPlatformTarget.EntityHolder.HeldPawn != null)
		{
			return false;
		}
		if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
		{
			return false;
		}
		if (!pawn.CanReserveAndReach(compHoldingPlatformTarget.targetHolder, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
		{
			return false;
		}
		if (t is Pawn pawn2 && !pawn2.ThreatDisabled(pawn))
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		CompHoldingPlatformTarget compHoldingPlatformTarget = t.TryGetComp<CompHoldingPlatformTarget>();
		if (compHoldingPlatformTarget == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.CarryToEntityHolder, compHoldingPlatformTarget.targetHolder, t);
		job.count = 1;
		return job;
	}
}
