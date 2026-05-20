using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class LordToil_PrepareCaravan_RopeAnimals : LordToil
	{
		protected IntVec3 destinationPoint;

		protected int? ropeeLimit;

		public override float? CustomWakeThreshold => 0.5f;

		public override bool AllowRestingInBed => false;

		public LordToil_PrepareCaravan_RopeAnimals(IntVec3 destinationPoint, int? ropeeLimit)
		{
			this.destinationPoint = destinationPoint;
			this.ropeeLimit = ropeeLimit;
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.IsColonist || AnimalPenUtility.NeedsToBeManagedByRope(pawn))
				{
					pawn.mindState.duty = MakeRopeDuty();
					pawn.mindState.duty.ropeeLimit = ropeeLimit;
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.PrepareCaravan_Wait, destinationPoint);
				}
			}
		}

		protected abstract PawnDuty MakeRopeDuty();
	}
}
