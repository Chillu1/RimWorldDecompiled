using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_SightstealerAssault : LordToil
{
	public override void UpdateAllDuties()
	{
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			ownedPawn.mindState.duty = new PawnDuty(DutyDefOf.SightstealerAssault);
		}
	}

	public override void Notify_PawnDamaged(Pawn victim, DamageInfo dinfo)
	{
		if (dinfo.Instigator != null && victim.HostileTo(dinfo.Instigator))
		{
			lord.Notify_PawnLost(victim, PawnLostCondition.LeftVoluntarily);
			victim.mindState.enemyTarget = dinfo.Instigator;
			victim.mindState.Notify_EngagedTarget();
			victim.jobs.EndCurrentJob(JobCondition.InterruptForced);
		}
	}
}
