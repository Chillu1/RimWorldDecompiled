using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_KeepLyingDown : ThinkNode_JobGiver
	{
		public bool useBed;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.GetPosture().Laying() && pawn.CurJob != null)
			{
				return pawn.CurJob;
			}
			return JobMaker.MakeJob(JobDefOf.LayDown, (pawn.InBed() && useBed) ? new LocalTargetInfo(pawn.CurrentBed()) : ((LocalTargetInfo)pawn.Position));
		}

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_KeepLyingDown obj = (JobGiver_KeepLyingDown)base.DeepCopy(resolve);
			obj.useBed = useBed;
			return obj;
		}
	}
}
