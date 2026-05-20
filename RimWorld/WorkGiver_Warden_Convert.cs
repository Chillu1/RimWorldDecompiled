using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_Convert : WorkGiver_Warden
{
	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckIdeology("WorkGiver_Warden_Convert"))
		{
			return false;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return false;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.InMentalState)
		{
			JobFailReason.Is("PawnIsInMentalState".Translate(pawn2));
			return false;
		}
		if (pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Convert) && !pawn2.guest.ScheduledForInteraction)
		{
			JobFailReason.Is("PrisonerInteractedTooRecently".Translate());
			return false;
		}
		return base.HasJobOnThing(pawn, t, forced);
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckIdeology("WorkGiver_Warden_Convert"))
		{
			return null;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Convert) && pawn2.guest.ScheduledForInteraction && pawn2.guest.IsPrisoner && (!pawn2.Downed || pawn2.InBed()) && pawn2.Ideo != pawn.Ideo && pawn.Ideo == pawn2.guest.ideoForConversion && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t) && pawn2.Awake())
		{
			return JobMaker.MakeJob(JobDefOf.PrisonerConvert, t);
		}
		return null;
	}
}
