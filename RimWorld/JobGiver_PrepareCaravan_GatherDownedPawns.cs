using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class JobGiver_PrepareCaravan_GatherDownedPawns : ThinkNode_JobGiver
	{
		private const float MaxDownedPawnToExitPointDistance = 7f;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				return null;
			}
			Pawn pawn2 = FindDownedPawn(pawn);
			if (pawn2 == null)
			{
				return null;
			}
			IntVec3 c = FindRandomDropCell(pawn, pawn2);
			Job job = JobMaker.MakeJob(JobDefOf.PrepareCaravan_GatherDownedPawns, pawn2, c);
			job.lord = pawn.GetLord();
			job.count = 1;
			return job;
		}

		private Pawn FindDownedPawn(Pawn pawn)
		{
			float num = 0f;
			Pawn pawn2 = null;
			List<Pawn> downedPawns = ((LordJob_FormAndSendCaravan)pawn.GetLord().LordJob).downedPawns;
			IntVec3 cell = pawn.mindState.duty.focusSecond.Cell;
			for (int i = 0; i < downedPawns.Count; i++)
			{
				Pawn pawn3 = downedPawns[i];
				if (pawn3.Downed && pawn3 != pawn && !IsDownedPawnNearExitPoint(pawn3, cell))
				{
					float num2 = pawn.Position.DistanceToSquared(pawn3.Position);
					if ((pawn2 == null || num2 < num) && pawn.CanReserveAndReach(pawn3, PathEndMode.Touch, Danger.Deadly))
					{
						pawn2 = pawn3;
						num = num2;
					}
				}
			}
			return pawn2;
		}

		private IntVec3 FindRandomDropCell(Pawn pawn, Pawn downedPawn)
		{
			return CellFinder.RandomClosewalkCellNear(pawn.mindState.duty.focusSecond.Cell, pawn.Map, 6, (IntVec3 x) => x.Standable(pawn.Map) && StoreUtility.IsGoodStoreCell(x, pawn.Map, downedPawn, pawn, pawn.Faction));
		}

		public static bool IsDownedPawnNearExitPoint(Pawn downedPawn, IntVec3 exitPoint)
		{
			if (downedPawn.Spawned)
			{
				return downedPawn.Position.InHorDistOf(exitPoint, 7f);
			}
			return false;
		}
	}
}
