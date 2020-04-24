using Verse;
using Verse.AI;

namespace RimWorld
{
	public class RecordWorker
	{
		public RecordDef def;

		public virtual bool ShouldMeasureTimeNow(Pawn pawn)
		{
			if (def.measuredTimeJobs == null)
			{
				return false;
			}
			Job curJob = pawn.CurJob;
			if (curJob == null)
			{
				return false;
			}
			for (int i = 0; i < def.measuredTimeJobs.Count; i++)
			{
				if (curJob.def == def.measuredTimeJobs[i])
				{
					return true;
				}
			}
			return false;
		}
	}
}
