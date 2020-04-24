namespace Verse.AI
{
	public class JobGiver_RunRandom : JobGiver_Wander
	{
		public JobGiver_RunRandom()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(5, 10);
			locomotionUrgency = LocomotionUrgency.Sprint;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
