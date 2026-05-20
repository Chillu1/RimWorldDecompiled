using Verse;
using Verse.AI;

namespace RimWorld
{
	public class LordToil_PrepareCaravan_CollectAnimals : LordToil_PrepareCaravan_RopeAnimals
	{
		public LordToil_PrepareCaravan_CollectAnimals(IntVec3 destinationPoint)
			: base(destinationPoint, int.MaxValue)
		{
		}

		protected override PawnDuty MakeRopeDuty()
		{
			return new PawnDuty(DutyDefOf.PrepareCaravan_CollectAnimals, destinationPoint);
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 != 0)
			{
				return;
			}
			bool flag = true;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (AnimalPenUtility.NeedsToBeManagedByRope(pawn) && !GatherAnimalsAndSlavesForCaravanUtility.IsRopedByCaravanPawn(pawn))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				lord.ReceiveMemo("AllAnimalsCollected");
			}
		}
	}
}
