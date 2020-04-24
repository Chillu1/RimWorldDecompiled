using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeInMentalState : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.InMentalState;
		}
	}
}
