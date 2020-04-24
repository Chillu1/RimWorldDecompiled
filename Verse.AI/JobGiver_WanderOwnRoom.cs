namespace Verse.AI
{
	public class JobGiver_WanderOwnRoom : JobGiver_Wander
	{
		public JobGiver_WanderOwnRoom()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(300, 600);
			locomotionUrgency = LocomotionUrgency.Amble;
			wanderDestValidator = ((Pawn pawn, IntVec3 loc, IntVec3 root) => WanderRoomUtility.IsValidWanderDest(pawn, loc, root));
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return (pawn.MentalState as MentalState_WanderOwnRoom)?.target ?? pawn.Position;
		}
	}
}
