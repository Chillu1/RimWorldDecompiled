namespace Verse.AI;

public class JobGiver_WanderNearRoamingExit : JobGiver_Wander
{
	public JobGiver_WanderNearRoamingExit()
	{
		wanderRadius = 12f;
	}

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (!(pawn.MentalState is MentalState_Roaming mentalState_Roaming))
		{
			return null;
		}
		if (mentalState_Roaming.ShouldExitMapNow())
		{
			return null;
		}
		return base.TryGiveJob(pawn);
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		return (pawn.MentalState as MentalState_Roaming)?.exitDest ?? pawn.Position;
	}
}
