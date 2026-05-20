using RimWorld.Planet;

namespace Verse.AI
{
	public class JobGiver_ExitMapFollowOverseer : JobGiver_ExitMapBest
	{
		public JobGiver_ExitMapFollowOverseer()
		{
			failIfCantJoinOrCreateCaravan = true;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!CaravanExitMapUtility.CanExitMapAndJoinOrCreateCaravanNow(pawn))
			{
				return null;
			}
			return base.TryGiveJob(pawn);
		}
	}
}
