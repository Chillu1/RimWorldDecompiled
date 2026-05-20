using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_PatientGoToBed : ThinkNode_JobGiver
{
	public bool respectTimetable = true;

	public bool urgentOnly;

	public override string CrawlingReportStringOverride => base.CrawlingReportStringOverride ?? ((string)"ReportStringCrawlingToBed".Translate());

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (urgentOnly && !HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
		{
			return null;
		}
		if (!HealthAIUtility.ShouldSeekMedicalRest(pawn))
		{
			return null;
		}
		if (respectTimetable && RestUtility.TimetablePreventsLayDown(pawn) && !HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && !HealthAIUtility.ShouldBeTendedNowByPlayer(pawn))
		{
			return null;
		}
		if (RestUtility.DisturbancePreventsLyingDown(pawn))
		{
			return null;
		}
		if (pawn.Downed && !pawn.health.CanCrawl)
		{
			return null;
		}
		if (pawn.GetPosture().InBed() && pawn.Downed)
		{
			return null;
		}
		Thing thing = RestUtility.FindBedFor(pawn, pawn, checkSocialProperness: false);
		if (thing == null)
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.LayDown, thing);
		job.checkOverrideOnExpire = true;
		return job;
	}

	public override ThinkNode DeepCopy(bool resolve = true)
	{
		JobGiver_PatientGoToBed obj = (JobGiver_PatientGoToBed)base.DeepCopy(resolve);
		obj.respectTimetable = respectTimetable;
		return obj;
	}
}
