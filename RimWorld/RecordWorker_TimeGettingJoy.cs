using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RecordWorker_TimeGettingJoy : RecordWorker
	{
		public override bool ShouldMeasureTimeNow(Pawn pawn)
		{
			Job curJob = pawn.CurJob;
			if (curJob != null)
			{
				return curJob.def.joyKind != null;
			}
			return false;
		}
	}
}
