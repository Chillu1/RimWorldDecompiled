using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class GatherAnimalsAndSlavesForCaravanUtility
{
	public static void CheckArrived(Lord lord, List<Pawn> pawns, IntVec3 meetingPoint, string memo, Predicate<Pawn> shouldCheckIfArrived, Predicate<Pawn> extraValidator = null)
	{
		bool flag = true;
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			if (shouldCheckIfArrived(pawn) && (!pawn.Spawned || !pawn.Position.InHorDistOf(meetingPoint, 10f) || !pawn.CanReach(meetingPoint, PathEndMode.ClosestTouch, Danger.Deadly) || (extraValidator != null && !extraValidator(pawn))))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			lord.ReceiveMemo(memo);
		}
	}

	public static bool IsRopedByCaravanPawn(Pawn animal)
	{
		Lord lord = animal.GetLord();
		if (lord != null && animal.roping.IsRopedByPawn)
		{
			return animal.roping.RopedByPawn?.GetLord() == lord;
		}
		return false;
	}

	public static bool CanRoperTakeAnimalToDest(Pawn pawn, Pawn animal, IntVec3 destSpot)
	{
		if (pawn.CanReach(animal, PathEndMode.Touch, Danger.Deadly))
		{
			return animal.Map.reachability.CanReach(animal.Position, destSpot, PathEndMode.OnCell, TraverseParms.For(pawn).WithFenceblockedOf(animal));
		}
		return false;
	}
}
