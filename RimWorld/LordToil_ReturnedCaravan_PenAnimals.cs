using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class LordToil_ReturnedCaravan_PenAnimals : LordToil_PrepareCaravan_RopeAnimals
	{
		private List<Pawn> tmpOwnedPawns = new List<Pawn>();

		public LordToil_ReturnedCaravan_PenAnimals(IntVec3 entryPoint)
			: base(entryPoint, int.MaxValue)
		{
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 100 != 0)
			{
				return;
			}
			tmpOwnedPawns.Clear();
			tmpOwnedPawns.AddRange(lord.ownedPawns);
			bool flag = false;
			bool flag2 = false;
			foreach (Pawn tmpOwnedPawn in tmpOwnedPawns)
			{
				if (AnimalPenUtility.NeedsToBeManagedByRope(tmpOwnedPawn))
				{
					if (tmpOwnedPawns.Contains(tmpOwnedPawn.roping.RopedByPawn))
					{
						flag2 = true;
					}
					else
					{
						lord.Notify_PawnLost(tmpOwnedPawn, PawnLostCondition.LeftVoluntarily);
					}
				}
				else if (tmpOwnedPawn.Spawned && tmpOwnedPawn.IsColonist && !tmpOwnedPawn.Downed && !tmpOwnedPawn.Dead && tmpOwnedPawn.roping.HasAnyRope)
				{
					flag = true;
				}
				else
				{
					lord.Notify_PawnLost(tmpOwnedPawn, PawnLostCondition.LeftVoluntarily);
				}
			}
			if (!flag || !flag2)
			{
				lord.ReceiveMemo("RepenningFinished");
			}
			tmpOwnedPawns.Clear();
		}

		protected override PawnDuty MakeRopeDuty()
		{
			return new PawnDuty(DutyDefOf.ReturnedCaravan_PenAnimals);
		}
	}
}
