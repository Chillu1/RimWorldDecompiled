using Verse;

namespace RimWorld;

public class RecordWorker_TimeInBedForMedicalReasons : RecordWorker
{
	public override bool ShouldMeasureTimeNow(Pawn pawn)
	{
		if (!pawn.InBed())
		{
			return false;
		}
		if (!HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn))
		{
			if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
			{
				if (pawn.needs.rest != null && !(pawn.needs.rest.CurLevel >= 1f))
				{
					return pawn.CurJob.restUntilHealed;
				}
				return true;
			}
			return false;
		}
		return true;
	}
}
