using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_Stage : LordToil
	{
		public override IntVec3 FlagLoc => Data.stagingPoint;

		private LordToilData_Stage Data => (LordToilData_Stage)data;

		public override bool ForceHighStoryDanger => true;

		public LordToil_Stage(IntVec3 stagingLoc)
		{
			data = new LordToilData_Stage();
			Data.stagingPoint = stagingLoc;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_Stage data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				lord.ownedPawns[i].mindState.duty = new PawnDuty(DutyDefOf.Defend, data.stagingPoint);
				lord.ownedPawns[i].mindState.duty.radius = 28f;
			}
		}
	}
}
