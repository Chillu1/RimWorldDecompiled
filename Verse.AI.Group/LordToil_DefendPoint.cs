using RimWorld;

namespace Verse.AI.Group;

public class LordToil_DefendPoint : LordToil
{
	private bool allowSatisfyLongNeeds = true;

	protected LordToilData_DefendPoint Data => (LordToilData_DefendPoint)data;

	public override IntVec3 FlagLoc => Data.defendPoint;

	public override bool AllowSatisfyLongNeeds => allowSatisfyLongNeeds;

	public LordToil_DefendPoint(bool canSatisfyLongNeeds = true)
	{
		allowSatisfyLongNeeds = canSatisfyLongNeeds;
		data = new LordToilData_DefendPoint();
	}

	public LordToil_DefendPoint(IntVec3 defendPoint, float? defendRadius = null, float? wanderRadius = null)
		: this()
	{
		Data.defendPoint = defendPoint;
		Data.defendRadius = defendRadius ?? 28f;
		Data.wanderRadius = wanderRadius;
	}

	public override void UpdateAllDuties()
	{
		LordToilData_DefendPoint lordToilData_DefendPoint = Data;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			Pawn pawn = lord.ownedPawns[i];
			if (pawn?.mindState != null)
			{
				pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, lordToilData_DefendPoint.defendPoint);
				pawn.mindState.duty.focusSecond = lordToilData_DefendPoint.defendPoint;
				pawn.mindState.duty.radius = ((pawn.kindDef.defendPointRadius >= 0f) ? pawn.kindDef.defendPointRadius : lordToilData_DefendPoint.defendRadius);
				pawn.mindState.duty.wanderRadius = lordToilData_DefendPoint.wanderRadius;
			}
		}
	}

	public void SetDefendPoint(IntVec3 defendPoint)
	{
		Data.defendPoint = defendPoint;
	}
}
