using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_ChimeraAttackNearbyHumans : ThinkNode_JobGiver
{
	public float attackRadius = 10f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.mindState?.duty?.def == DutyDefOf.ChimeraStalkFlee)
		{
			return null;
		}
		List<Pawn> allHumanlikeSpawned = pawn.Map.mapPawns.AllHumanlikeSpawned;
		for (int i = 0; i < allHumanlikeSpawned.Count; i++)
		{
			if (!allHumanlikeSpawned[i].DeadOrDowned && pawn.Position.InHorDistOf(allHumanlikeSpawned[i].Position, attackRadius))
			{
				return JobMaker.MakeJob(JobDefOf.ChimeraSwitchToAttackMode);
			}
		}
		return null;
	}
}
