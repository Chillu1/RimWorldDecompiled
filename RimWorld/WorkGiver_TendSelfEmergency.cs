using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_TendSelfEmergency : WorkGiver_TendSelf
	{
		private static JobGiver_SelfTend jgp = new JobGiver_SelfTend();

		public override Job NonScanJob(Pawn pawn)
		{
			if (!HasJobOnThing(pawn, pawn) || !HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(pawn))
			{
				return null;
			}
			ThinkResult thinkResult = jgp.TryIssueJobPackage(pawn, default(JobIssueParams));
			if (thinkResult.IsValid)
			{
				return thinkResult.Job;
			}
			return null;
		}
	}
}
