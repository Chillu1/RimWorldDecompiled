using RimWorld;

namespace Verse.AI.Group
{
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

		public LordToil_DefendPoint(IntVec3 defendPoint, float defendRadius = 28f, float? wanderRadius = null)
			: this()
		{
			Data.defendPoint = defendPoint;
			Data.defendRadius = defendRadius;
			Data.wanderRadius = wanderRadius;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_DefendPoint data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				pawn.mindState.duty = new PawnDuty(DutyDefOf.Defend, data.defendPoint);
				pawn.mindState.duty.focusSecond = data.defendPoint;
				pawn.mindState.duty.radius = ((pawn.kindDef.defendPointRadius >= 0f) ? pawn.kindDef.defendPointRadius : data.defendRadius);
				pawn.mindState.duty.wanderRadius = data.wanderRadius;
			}
		}

		public void SetDefendPoint(IntVec3 defendPoint)
		{
			Data.defendPoint = defendPoint;
		}
	}
}
