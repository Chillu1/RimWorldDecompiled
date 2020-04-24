using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeDrafted : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.Drafted;
		}
	}
}
