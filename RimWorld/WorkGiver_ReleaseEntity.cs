using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ReleaseEntity : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override string PostProcessedGerund(Job job)
	{
		return "ReleasingEntity".Translate(def.gerund.Named("GERUND"), job.targetB.Label.Named("TARGETLABEL"));
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (GetEntity(t) == null)
		{
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn entity = GetEntity(t);
		if (entity == null)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.ReleaseEntity, t, entity).WithCount(1);
	}

	private Pawn GetEntity(Thing thing)
	{
		if (thing is Building_HoldingPlatform { HeldPawn: var heldPawn })
		{
			if (heldPawn == null)
			{
				return null;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = heldPawn.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null && compHoldingPlatformTarget.containmentMode != EntityContainmentMode.Release)
			{
				return null;
			}
			return heldPawn;
		}
		return null;
	}
}
