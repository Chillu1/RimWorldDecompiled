using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_HaulToPortal : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		MapPortal portal = pawn.mindState.duty.focus.Thing as MapPortal;
		if (EnterPortalUtility.HasJobOnPortal(pawn, portal))
		{
			return EnterPortalUtility.JobOnPortal(pawn, portal);
		}
		return null;
	}
}
