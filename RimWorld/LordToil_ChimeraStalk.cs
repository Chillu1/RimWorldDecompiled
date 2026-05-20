using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_ChimeraStalk : LordToil
{
	public override void UpdateAllDuties()
	{
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			if (lord.ownedPawns[i].mindState.duty?.def != DutyDefOf.ChimeraStalkFlee)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.ChimeraStalkFlee);
				lord.ownedPawns[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}
	}

	public override void Notify_PawnJobDone(Pawn p, JobCondition condition)
	{
		base.Notify_PawnJobDone(p, condition);
		if (p.mindState.duty?.def == DutyDefOf.ChimeraStalkFlee && p.CurJobDef == JobDefOf.Wait_Wander)
		{
			p.mindState.duty = new PawnDuty(DutyDefOf.ChimeraStalkWander);
		}
	}
}
