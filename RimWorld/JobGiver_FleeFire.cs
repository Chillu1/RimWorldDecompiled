using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_FleeFire : ThinkNode_JobGiver
{
	private const int FleeDistance = 24;

	private const int DistToFireToFlee = 20;

	protected override Job TryGiveJob(Pawn pawn)
	{
		Job job = FleeUtility.FleeLargeFireJob(pawn, 1, 20, 24);
		if (job != null)
		{
			return job;
		}
		return null;
	}
}
