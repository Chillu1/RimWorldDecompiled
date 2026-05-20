namespace Verse.AI;

public class JobGiver_WanderCurrentRoom : JobGiver_Wander
{
	public JobGiver_WanderCurrentRoom()
	{
		wanderRadius = 7f;
		ticksBetweenWandersRange = new IntRange(125, 200);
		locomotionUrgency = LocomotionUrgency.Amble;
		wanderDestValidator = (Pawn pawn, IntVec3 loc, IntVec3 root) => WanderRoomUtility.IsValidWanderDest(pawn, loc, root);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		return pawn.Position;
	}
}
