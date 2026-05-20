using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_TransferEntity : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.EntityHolder);

	public override bool ShouldSkip(Pawn pawn, bool forced = false)
	{
		return !ModsConfig.AnomalyActive;
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		Pawn entity = GetEntity(t);
		if (entity == null)
		{
			return false;
		}
		CompHoldingPlatformTarget compHoldingPlatformTarget = entity.TryGetComp<CompHoldingPlatformTarget>();
		if (compHoldingPlatformTarget == null)
		{
			return false;
		}
		if (compHoldingPlatformTarget.targetHolder == null)
		{
			return false;
		}
		if (!pawn.CanReserve(compHoldingPlatformTarget.targetHolder, 1, -1, null, forced) || !pawn.CanReserve(entity, 1, -1, null, forced))
		{
			return false;
		}
		if (compHoldingPlatformTarget.HeldPlatform.IsForbidden(pawn) || compHoldingPlatformTarget.targetHolder.IsForbidden(pawn))
		{
			JobFailReason.Is("ForbiddenLower".Translate());
			return false;
		}
		return compHoldingPlatformTarget.HeldPlatform != compHoldingPlatformTarget.targetHolder;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn entity = GetEntity(t);
		if (entity == null)
		{
			return null;
		}
		CompHoldingPlatformTarget compHoldingPlatformTarget = entity.TryGetComp<CompHoldingPlatformTarget>();
		if (compHoldingPlatformTarget == null)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.TransferBetweenEntityHolders, t, compHoldingPlatformTarget.targetHolder, entity).WithCount(1);
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
			if (compHoldingPlatformTarget != null && compHoldingPlatformTarget.HeldPlatform == compHoldingPlatformTarget.targetHolder)
			{
				return null;
			}
			return heldPawn;
		}
		return null;
	}
}
