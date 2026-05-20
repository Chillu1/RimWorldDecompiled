using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class LordToil_DefendFleshmassHeart : LordToil
{
	private const float DefendRadius = 50f;

	private const float WanderRadius = 12f;

	private const int SwitchDefendPointInterval = 3600;

	private LordToilData_DefendFleshmassHeart Data => (LordToilData_DefendFleshmassHeart)data;

	private LordToil_DefendFleshmassHeart()
	{
		data = new LordToilData_DefendFleshmassHeart();
	}

	public LordToil_DefendFleshmassHeart(Building_FleshmassHeart heart)
		: this()
	{
		Data.heart = heart;
	}

	public override void UpdateAllDuties()
	{
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			UpdateDefendPoint(ownedPawn);
		}
	}

	public override void LordToilTick()
	{
		if (Data.heart.DestroyedOrNull())
		{
			base.Map.lordManager.RemoveLord(lord);
			return;
		}
		foreach (Pawn ownedPawn in lord.ownedPawns)
		{
			if (ownedPawn.IsHashIntervalTick(3600))
			{
				UpdateDefendPoint(ownedPawn);
			}
		}
	}

	private void UpdateDefendPoint(Pawn pawn)
	{
		if (pawn.mindState == null || pawn.mindState.CombatantRecently)
		{
			return;
		}
		IntVec3 result = IntVec3.Invalid;
		int i;
		for (i = 0; i < 100; i++)
		{
			if (!Data.heart.Comp.ContiguousFleshmass.TryRandomElement(out result))
			{
				result = Data.heart.Position;
				break;
			}
			bool flag = false;
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			foreach (IntVec3 intVec in cardinalDirections)
			{
				IntVec3 intVec2 = result + intVec;
				if (intVec2.InBounds(base.Map) && intVec2.Standable(base.Map) && pawn.CanReach(intVec2, PathEndMode.OnCell, Danger.Deadly))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (i == 100 || result == IntVec3.Invalid)
		{
			result = Data.heart.Position;
		}
		pawn.mindState.duty = new PawnDuty(DutyDefOf.DefendFleshmassHeart, result);
		pawn.mindState.duty.focusSecond = result;
		pawn.mindState.duty.radius = 50f;
		pawn.mindState.duty.wanderRadius = 12f;
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
