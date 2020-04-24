namespace Verse.AI
{
	public class JobGiver_WanderNearFallbackLocation : JobGiver_Wander
	{
		public JobGiver_WanderNearFallbackLocation()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return WanderUtility.BestCloseWanderRoot(pawn.mindState.duty.focusSecond.Cell, pawn);
		}
	}
}
