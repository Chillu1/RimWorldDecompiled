using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeUnderRoof : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (!pawn.Spawned)
			{
				return false;
			}
			return pawn.Position.Roofed(pawn.Map);
		}
	}
}
