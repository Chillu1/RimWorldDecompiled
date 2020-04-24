using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_Leave : LordToil
	{
		private IntVec3 exitSpot;

		public override bool AllowSatisfyLongNeeds => false;

		public override float? CustomWakeThreshold => 0.5f;

		public override bool AllowRestingInBed => false;

		public override bool AllowSelfTend => false;

		public LordToil_PrepareCaravan_Leave(IntVec3 exitSpot)
		{
			this.exitSpot = exitSpot;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				pawn.mindState.duty = new PawnDuty(DutyDefOf.TravelOrWait, exitSpot);
				pawn.mindState.duty.locomotion = LocomotionUrgency.Jog;
			}
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 == 0)
			{
				GatherAnimalsAndSlavesForCaravanUtility.CheckArrived(lord, lord.ownedPawns, exitSpot, "ReadyToExitMap", (Pawn x) => true);
			}
		}
	}
}
