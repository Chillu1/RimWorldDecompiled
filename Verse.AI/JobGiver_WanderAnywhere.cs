namespace Verse.AI
{
	public class JobGiver_WanderAnywhere : JobGiver_Wander
	{
		public JobGiver_WanderAnywhere()
		{
			wanderRadius = 7f;
			locomotionUrgency = LocomotionUrgency.Walk;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
