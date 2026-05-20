namespace Verse.AI;

public class JobGiver_WanderNearMaster : JobGiver_Wander
{
	public JobGiver_WanderNearMaster()
	{
		wanderRadius = 3f;
		ticksBetweenWandersRange = new IntRange(125, 200);
		wanderDestValidator = (Pawn p, IntVec3 c, IntVec3 root) => (!MustUseRootRoom(p) || root.GetRoom(p.Map) == null || WanderRoomUtility.IsValidWanderDest(p, c, root)) ? true : false;
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		return WanderUtility.BestCloseWanderRoot(pawn.playerSettings.Master.PositionHeld, pawn);
	}

	private bool MustUseRootRoom(Pawn pawn)
	{
		return !pawn.playerSettings.Master.playerSettings.animalsReleased;
	}
}
