using RimWorld;

namespace Verse.AI.Group
{
	public class LordToil_ExitMapNear : LordToil
	{
		private IntVec3 near;

		private float radius;

		private LocomotionUrgency locomotion;

		private bool canDig;

		public override bool AllowSatisfyLongNeeds => false;

		public override bool AllowSelfTend => false;

		public LordToil_ExitMapNear(IntVec3 near, float radius, LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false)
		{
			this.near = near;
			this.radius = radius;
			this.locomotion = locomotion;
			this.canDig = canDig;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				PawnDuty pawnDuty = new PawnDuty(DutyDefOf.ExitMapNearDutyTarget, near, radius);
				pawnDuty.locomotion = locomotion;
				pawnDuty.canDig = canDig;
				lord.ownedPawns[i].mindState.duty = pawnDuty;
			}
		}
	}
}
