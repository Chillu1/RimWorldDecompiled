using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeInBed : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.InBed();
		}
	}
}
