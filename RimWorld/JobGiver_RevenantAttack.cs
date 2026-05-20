using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_RevenantAttack : ThinkNode_JobGiver
{
	protected override Job TryGiveJob(Pawn pawn)
	{
		return JobMaker.MakeJob(JobDefOf.RevenantAttack, pawn.mindState.enemyTarget);
	}
}
