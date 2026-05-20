namespace Verse.AI
{
	public class JobGiver_WanderRoped : JobGiver_Wander
	{
		public JobGiver_WanderRoped()
		{
			wanderRadius = 6f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.roping.RopedTo.Cell;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.roping.IsRoped || pawn.roping.IsRopedByPawn)
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}
	}
}
