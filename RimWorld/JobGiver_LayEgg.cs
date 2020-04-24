using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_LayEgg : ThinkNode_JobGiver
	{
		private const float LayRadius = 5f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			CompEggLayer compEggLayer = pawn.TryGetComp<CompEggLayer>();
			if (compEggLayer == null || !compEggLayer.CanLayNow)
			{
				return null;
			}
			IntVec3 c = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 5f, null, Danger.Some);
			return JobMaker.MakeJob(JobDefOf.LayEgg, c);
		}
	}
}
