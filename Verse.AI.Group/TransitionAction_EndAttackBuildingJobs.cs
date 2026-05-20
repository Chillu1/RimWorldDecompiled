using System.Collections.Generic;
using RimWorld;

namespace Verse.AI.Group;

public class TransitionAction_EndAttackBuildingJobs : TransitionAction
{
	public override void DoAction(Transition trans)
	{
		List<Pawn> ownedPawns = trans.target.lord.ownedPawns;
		for (int i = 0; i < ownedPawns.Count; i++)
		{
			Pawn pawn = ownedPawns[i];
			if (pawn.jobs != null && pawn.jobs.curJob != null && pawn.jobs.curJob.def == JobDefOf.AttackMelee && pawn.jobs.curJob.targetA.Thing is Building)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}
}
