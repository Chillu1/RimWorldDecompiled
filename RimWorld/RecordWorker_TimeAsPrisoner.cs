using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeAsPrisoner : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.IsPrisoner;
		}
	}
}
