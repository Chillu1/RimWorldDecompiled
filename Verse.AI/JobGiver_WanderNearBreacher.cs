using RimWorld;

namespace Verse.AI
{
	public class JobGiver_WanderNearBreacher : JobGiver_Wander
	{
		public JobGiver_WanderNearBreacher()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return BreachingUtility.FindPawnToEscort(pawn)?.Position ?? IntVec3.Invalid;
		}
	}
}
