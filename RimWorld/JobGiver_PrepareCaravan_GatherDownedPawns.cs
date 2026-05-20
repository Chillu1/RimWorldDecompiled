using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

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
		IntVec3 intVec = FindRandomDropCell(pawn, pawn2);
		Job job = JobMaker.MakeJob(JobDefOf.PrepareCaravan_GatherDownedPawns, pawn2, intVec);
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
		if (pawn.carryTracker.CarriedThing is Pawn pawn3 && downedPawns.Contains(pawn3))
		{
			return pawn3;
		}
		for (int i = 0; i < downedPawns.Count; i++)
		{
			Pawn pawn4 = downedPawns[i];
			if (pawn4.Downed && pawn4 != pawn && !IsDownedPawnNearExitPoint(pawn4, cell))
			{
				float num2 = pawn.Position.DistanceToSquared(pawn4.Position);
				if ((pawn2 == null || num2 < num) && pawn.CanReserveAndReach(pawn4, PathEndMode.Touch, Danger.Deadly))
				{
					pawn2 = pawn4;
					num = num2;
				}
			}
		}
		return pawn2;
	}

	public static IntVec3 FindRandomDropCell(Pawn pawn, Pawn downedPawn)
	{
		return CellFinder.RandomClosewalkCellNear(((LordJob_FormAndSendCaravan)CaravanFormingUtility.GetFormAndSendCaravanLord(downedPawn).LordJob).ExitSpot, pawn.Map, 6, (IntVec3 x) => x.Standable(pawn.Map) && StoreUtility.IsGoodStoreCell(x, pawn.Map, downedPawn, pawn, pawn.Faction));
	}

	public static bool IsDownedPawnNearExitPoint(Pawn downedPawn, IntVec3 exitPoint)
	{
		return downedPawn.PositionHeld.InHorDistOf(exitPoint, 7f);
	}
}
