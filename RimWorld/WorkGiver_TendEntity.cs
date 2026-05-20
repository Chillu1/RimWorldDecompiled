using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_TendEntity : WorkGiver_EntityOnPlatform
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		Pawn entity = GetEntity(t);
		if (entity != null)
		{
			return HealthAIUtility.ShouldBeTendedNowByPlayer(entity);
		}
		return false;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn entity = GetEntity(t);
		if (entity == null)
		{
			return null;
		}
		Thing thing = HealthAIUtility.FindBestMedicine(pawn, entity);
		JobDef tendEntity = JobDefOf.TendEntity;
		LocalTargetInfo targetA = t;
		Thing thing2 = thing;
		return JobMaker.MakeJob(tendEntity, targetA, (thing2 != null) ? ((LocalTargetInfo)thing2) : LocalTargetInfo.Invalid);
	}

	protected override Pawn GetEntity(Thing potentialPlatform)
	{
		return GetTendableEnityFromPotentialPlatform(potentialPlatform);
	}

	public static Pawn GetTendableEnityFromPotentialPlatform(Thing potentialPlatform)
	{
		if (potentialPlatform is Building_HoldingPlatform { HeldPawn: var heldPawn })
		{
			if (heldPawn == null)
			{
				return null;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = heldPawn.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget != null && (compHoldingPlatformTarget.containmentMode == EntityContainmentMode.Release || compHoldingPlatformTarget.containmentMode == EntityContainmentMode.Execute))
			{
				return null;
			}
			return heldPawn;
		}
		return null;
	}
}
