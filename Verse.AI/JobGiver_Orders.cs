using RimWorld;

namespace Verse.AI
{
	public class JobGiver_Orders : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.Drafted)
			{
				return JobMaker.MakeJob(JobDefOf.Wait_Combat, pawn.Position);
			}
			return null;
		}
	}
}
