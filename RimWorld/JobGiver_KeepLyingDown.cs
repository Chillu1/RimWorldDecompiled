using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_KeepLyingDown : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.GetPosture().Laying())
			{
				return pawn.CurJob;
			}
			return JobMaker.MakeJob(JobDefOf.LayDown, pawn.Position);
		}
	}
}
