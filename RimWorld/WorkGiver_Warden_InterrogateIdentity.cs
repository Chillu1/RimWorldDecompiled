using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_InterrogateIdentity : WorkGiver_Warden
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return false;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t, forced))
		{
			return false;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.InMentalState)
		{
			JobFailReason.Is("PawnIsInMentalState".Translate(pawn2));
			return false;
		}
		if (pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Interrogate) && !pawn2.guest.ScheduledForInteraction)
		{
			JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
			return false;
		}
		return base.HasJobOnThing(pawn, t, forced);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModsConfig.AnomalyActive)
		{
			return null;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Interrogate) && pawn2.guest.ScheduledForInteraction && pawn2.guest.IsPrisoner && (!pawn2.Downed || pawn2.InBed()) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t) && pawn2.Awake())
		{
			return JobMaker.MakeJob(JobDefOf.PrisonerInterrogateIdentity, t);
		}
		return null;
	}
}
