using Verse;
using Verse.AI;

namespace RimWorld;

public static class CaptureUtility
{
	public static void OrderArrest(Pawn arrestor, Pawn victim)
	{
		if (TryGetBed(arrestor, victim, out var bed))
		{
			Job job = JobMaker.MakeJob(JobDefOf.Arrest, victim, bed);
			job.count = 1;
			job.playerForced = true;
			arrestor.jobs.StartJob(job, JobCondition.InterruptForced);
		}
	}

	public static bool CanArrest(Pawn arrestor, Pawn victim, out string reason)
	{
		if (!TryGetBed(arrestor, victim, out var _))
		{
			reason = "NoPrisonerBedShort".Translate();
			return false;
		}
		reason = null;
		return true;
	}

	public static bool TryGetBed(Pawn arrestor, Pawn victim, out Thing bed)
	{
		bed = RestUtility.FindBedFor(victim, arrestor, checkSocialProperness: false, ignoreOtherReservations: false, GuestStatus.Prisoner);
		if (bed == null)
		{
			bed = RestUtility.FindBedFor(victim, arrestor, checkSocialProperness: false, ignoreOtherReservations: true, GuestStatus.Prisoner);
		}
		return bed != null;
	}
}
