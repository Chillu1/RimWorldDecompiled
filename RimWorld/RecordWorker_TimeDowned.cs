using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeDowned : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.Downed;
		}
	}
}
