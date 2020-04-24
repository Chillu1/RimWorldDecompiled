using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_PatientGoToBedEmergencyTreatment : WorkGiver_PatientGoToBedRecuperate
	{
		public override Job NonScanJob(Pawn pawn)
		{
			if (!HealthAIUtility.ShouldBeTendedNowByPlayerUrgent(pawn))
			{
				return null;
			}
			return base.NonScanJob(pawn);
		}
	}
}
