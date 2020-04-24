using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMap : LordToil
	{
		public override bool AllowSatisfyLongNeeds => false;

		public override bool AllowSelfTend => false;

		public virtual DutyDef ExitDuty => DutyDefOf.ExitMapBest;

		protected LordToilData_ExitMap Data => (LordToilData_ExitMap)data;

		public LordToil_ExitMap(LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false, bool interruptCurrentJob = false)
		{
			data = new LordToilData_ExitMap();
			Data.locomotion = locomotion;
			Data.canDig = canDig;
			Data.interruptCurrentJob = interruptCurrentJob;
		}

		public override void UpdateAllDuties()
		{
			LordToilData_ExitMap data = Data;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(ExitDuty);
				pawnDuty.locomotion = data.locomotion;
				pawnDuty.canDig = data.canDig;
				Pawn pawn = lord.ownedPawns[i];
				pawn.mindState.duty = pawnDuty;
				if (Data.interruptCurrentJob && pawn.jobs.curJob != null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
				}
			}
		}
	}
}
