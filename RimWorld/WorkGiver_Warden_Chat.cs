using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_Chat : WorkGiver_Warden
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return false;
		}
		Pawn pawn2 = (Pawn)t;
		PrisonerInteractionModeDef exclusiveInteractionMode = pawn2.guest.ExclusiveInteractionMode;
		if (pawn2.InMentalState)
		{
			JobFailReason.Is("PawnIsInMentalState".Translate(pawn2));
			return false;
		}
		if ((exclusiveInteractionMode == PrisonerInteractionModeDefOf.AttemptRecruit || exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance) && !pawn2.guest.ScheduledForInteraction)
		{
			JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
			return false;
		}
		return base.HasJobOnThing(pawn, t, forced);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		PrisonerInteractionModeDef exclusiveInteractionMode = pawn2.guest.ExclusiveInteractionMode;
		if ((exclusiveInteractionMode == PrisonerInteractionModeDefOf.AttemptRecruit || exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance) && pawn2.guest.ScheduledForInteraction && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && (!pawn2.Downed || pawn2.InBed()) && pawn.CanReserve(t) && pawn2.Awake())
		{
			if (exclusiveInteractionMode == PrisonerInteractionModeDefOf.ReduceResistance && pawn2.guest.Resistance <= 0f)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.PrisonerAttemptRecruit, t);
		}
		return null;
	}
}
