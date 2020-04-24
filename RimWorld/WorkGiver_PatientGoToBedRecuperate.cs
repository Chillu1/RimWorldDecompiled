using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PatientGoToBedRecuperate : WorkGiver
	{
		private static JobGiver_PatientGoToBed jgp = new JobGiver_PatientGoToBed
		{
			respectTimetable = false
		};

		public override Job NonScanJob(Pawn pawn)
		{
			ThinkResult thinkResult = jgp.TryIssueJobPackage(pawn, default(JobIssueParams));
			if (thinkResult.IsValid)
			{
				return thinkResult.Job;
			}
			return null;
		}
	}
}
