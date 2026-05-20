using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_Enslave : WorkGiver_Warden
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ModLister.CheckIdeology("WorkGiver_Warden_Enslave"))
		{
			return null;
		}
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		if (pawn2.InMentalState)
		{
			JobFailReason.Is("PawnIsInMentalState".Translate(pawn2));
			return null;
		}
		bool flag = pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.Enslave);
		bool flag2 = pawn2.guest.IsInteractionEnabled(PrisonerInteractionModeDefOf.ReduceWill);
		if ((flag || flag2) && pawn2.guest.ScheduledForInteraction && pawn2.guest.IsPrisoner && (!flag2 || pawn2.guest.will > 0f) && (!pawn2.Downed || pawn2.InBed()) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking) && pawn.CanReserve(t) && pawn2.Awake() && new HistoryEvent(HistoryEventDefOf.EnslavedPrisoner, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
		{
			if (flag)
			{
				return JobMaker.MakeJob(JobDefOf.PrisonerEnslave, t);
			}
			return JobMaker.MakeJob(JobDefOf.PrisonerReduceWill, t);
		}
		return null;
	}
}
