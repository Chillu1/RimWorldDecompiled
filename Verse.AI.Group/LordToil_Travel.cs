using RimWorld;

namespace Verse.AI.Group;

public class LordToil_Travel : LordToil
{
	public Danger maxDanger;

	public override IntVec3 FlagLoc => Data.dest;

	protected LordToilData_Travel Data => (LordToilData_Travel)data;

	public override bool AllowSatisfyLongNeeds => false;

	protected virtual float AllArrivedCheckRadius => 10f;

	public LordToil_Travel(IntVec3 dest)
	{
		data = new LordToilData_Travel();
		Data.dest = dest;
	}

	public override void UpdateAllDuties()
	{
		LordToilData_Travel lordToilData_Travel = Data;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			PawnDuty pawnDuty = new PawnDuty(DutyDefOf.TravelOrLeave, lordToilData_Travel.dest);
			pawnDuty.maxDanger = maxDanger;
			lord.ownedPawns[i].mindState.duty = pawnDuty;
		}
	}

	public override void LordToilTick()
	{
		if (Find.TickManager.TicksGame % 205 != 0)
		{
			return;
		}
		LordToilData_Travel lordToilData_Travel = Data;
		bool flag = true;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (!pawn.Position.InHorDistOf(lordToilData_Travel.dest, AllArrivedCheckRadius) || !pawn.CanReach(lordToilData_Travel.dest, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			lord.ReceiveMemo("TravelArrived");
		}
	}

	public bool HasDestination()
	{
		return Data.dest.IsValid;
	}

	public void SetDestination(IntVec3 dest)
	{
		Data.dest = dest;
	}
}
