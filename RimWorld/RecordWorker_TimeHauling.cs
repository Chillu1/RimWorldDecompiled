using Verse;

namespace RimWorld
{
	public class RecordWorker_TimeHauling : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (!pawn.Dead)
			{
				return pawn.carryTracker.CarriedThing != null;
			}
			return false;
		}
	}
}
