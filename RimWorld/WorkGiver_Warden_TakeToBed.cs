using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_TakeToBed : WorkGiver_Warden
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ShouldTakeCareOfPrisoner(pawn, t, forced))
		{
			return null;
		}
		return TryMakeJob(pawn, t, forced);
	}

	public static Job TryMakeJob(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = (Pawn)t;
		Job job = TakeDownedToBedJob(pawn2, pawn);
		if (job != null)
		{
			return job;
		}
		Job job2 = TakeToPreferredBedJob(pawn2, pawn);
		if (job2 != null)
		{
			return job2;
		}
		if (pawn2.ownership.OwnedBed == null)
		{
			JobFailReason.Is("NoPrisonerBedShort".Translate());
		}
		return null;
	}

	private static Job TakeToPreferredBedJob(Pawn prisoner, Pawn warden)
	{
		if (prisoner.Downed || !warden.CanReserve(prisoner))
		{
			return null;
		}
		if (RestUtility.FindBedFor(prisoner, prisoner, checkSocialProperness: true, ignoreOtherReservations: false, GuestStatus.Prisoner) != null)
		{
			return null;
		}
		Room room = prisoner.GetRoom();
		Building_Bed building_Bed = RestUtility.FindBedFor(prisoner, warden, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
		if (building_Bed != null && building_Bed.GetRoom() != room)
		{
			Job job = JobMaker.MakeJob(JobDefOf.EscortPrisonerToBed, prisoner, building_Bed);
			job.count = 1;
			return job;
		}
		return null;
	}

	private static Job TakeDownedToBedJob(Pawn prisoner, Pawn warden)
	{
		if (!prisoner.Downed || !HealthAIUtility.ShouldSeekMedicalRest(prisoner) || prisoner.InBed() || !warden.CanReserve(prisoner))
		{
			return null;
		}
		Building_Bed building_Bed = RestUtility.FindBedFor(prisoner, warden, checkSocialProperness: true, ignoreOtherReservations: false, GuestStatus.Prisoner);
		if (building_Bed != null)
		{
			Job job = JobMaker.MakeJob(JobDefOf.TakeWoundedPrisonerToBed, prisoner, building_Bed);
			job.count = 1;
			return job;
		}
		return null;
	}

	public static void TryTakePrisonerToBed(Pawn prisoner, Pawn warden)
	{
		if (prisoner.Spawned && !prisoner.InAggroMentalState && !prisoner.IsForbidden(warden) && !prisoner.IsFormingCaravan() && warden.CanReserveAndReach(prisoner, PathEndMode.OnCell, warden.NormalMaxDanger(), 1, -1, null, ignoreOtherReservations: true))
		{
			Job job = TryMakeJob(warden, prisoner, forced: true);
			if (job != null)
			{
				warden.jobs.StartJob(job, JobCondition.InterruptForced);
			}
		}
	}
}
