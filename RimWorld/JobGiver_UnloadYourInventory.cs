using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_UnloadYourInventory : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.inventory.UnloadEverything)
			{
				return null;
			}
			return JobMaker.MakeJob(JobDefOf.UnloadYourInventory);
		}
	}
}
