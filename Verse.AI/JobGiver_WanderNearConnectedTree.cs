namespace Verse.AI
{
	public class JobGiver_WanderNearConnectedTree : JobGiver_Wander
	{
		public JobGiver_WanderNearConnectedTree()
		{
			wanderRadius = 12f;
			ticksBetweenWandersRange = new IntRange(130, 250);
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.connections.ConnectedThings.FirstOrDefault((Thing x) => x.Spawned && x.Map == pawn.Map && pawn.CanReach(x, PathEndMode.Touch, Danger.Deadly)) == null)
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.connections.ConnectedThings[0].Position;
		}
	}
}
