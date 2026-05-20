using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ExecuteEntity : WorkGiver_EntityOnPlatform
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		return GetEntity(t) != null;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		return JobMaker.MakeJob(JobDefOf.ExecuteEntity, t);
	}

	protected override Pawn GetEntity(Thing potentialPlatform)
	{
		if (potentialPlatform is Building_HoldingPlatform { HeldPawn: var heldPawn })
		{
			CompHoldingPlatformTarget compHoldingPlatformTarget = heldPawn?.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget == null || compHoldingPlatformTarget.containmentMode != EntityContainmentMode.Execute)
			{
				return null;
			}
			return heldPawn;
		}
		return null;
	}
}
