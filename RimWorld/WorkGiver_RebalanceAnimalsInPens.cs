using Verse;

namespace RimWorld
{
	public class WorkGiver_RebalanceAnimalsInPens : WorkGiver_TakeToPen
	{
		public WorkGiver_RebalanceAnimalsInPens()
		{
			ropingPriority = RopingPriority.Balanced;
		}
	}
}
