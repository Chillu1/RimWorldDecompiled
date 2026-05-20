using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_ExtractBioferrite : WorkGiver_EntityOnPlatform
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
		if (GetEntity(t) == null)
		{
			return null;
		}
		return JobMaker.MakeJob(JobDefOf.ExtractBioferrite, t);
	}

	protected override Pawn GetEntity(Thing potentialPlatform)
	{
		if (potentialPlatform is Building_HoldingPlatform { HeldPawn: var heldPawn })
		{
			if (heldPawn == null)
			{
				return null;
			}
			CompHoldingPlatformTarget compHoldingPlatformTarget = heldPawn.TryGetComp<CompHoldingPlatformTarget>();
			if (compHoldingPlatformTarget == null || !compHoldingPlatformTarget.extractBioferrite)
			{
				return null;
			}
			if (heldPawn.health.hediffSet.HasHediff(HediffDefOf.BioferriteExtracted))
			{
				JobFailReason.Is("BioferriteAlreadyExtracted".Translate(), "ExtractBioferrite".Translate(heldPawn.LabelShort));
				return null;
			}
			if (CompProducesBioferrite.BioferritePerDay(heldPawn) <= 0f)
			{
				return null;
			}
			return heldPawn;
		}
		return null;
	}
}
