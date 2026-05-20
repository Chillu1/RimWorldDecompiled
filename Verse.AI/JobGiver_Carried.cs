using RimWorld;

namespace Verse.AI
{
	public class JobGiver_Carried : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.Awake() && pawn.CurJob != null)
			{
				return pawn.CurJob;
			}
			return JobMaker.MakeJob(JobDefOf.Carried);
		}
	}
}
