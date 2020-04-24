using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PatientGoToBed : ThinkNode
	{
		public bool respectTimetable = true;

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (!HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				return ThinkResult.NoJob;
			}
			if (respectTimetable && RestUtility.TimetablePreventsLayDown(pawn) && !HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) && !HealthAIUtility.ShouldBeTendedNowByPlayer(pawn))
			{
				return ThinkResult.NoJob;
			}
			if (RestUtility.DisturbancePreventsLyingDown(pawn))
			{
				return ThinkResult.NoJob;
			}
			Thing thing = RestUtility.FindPatientBedFor(pawn);
			if (thing == null)
			{
				return ThinkResult.NoJob;
			}
			return new ThinkResult(JobMaker.MakeJob(JobDefOf.LayDown, thing), this);
		}
	}
}
