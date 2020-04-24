using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeOnFire : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			return pawn.IsBurning();
		}
	}
}
