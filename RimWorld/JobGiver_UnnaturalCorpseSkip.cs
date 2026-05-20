using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public class JobGiver_UnnaturalCorpseSkip : ThinkNode_JobGiver
{
	private const float MoveSpeedCellsFactor = 40f;

	private const float MinDistFromTarget = 5f;

	protected override Job TryGiveJob(Pawn pawn)
	{
		if (pawn.Downed)
		{
			return null;
		}
		if (!Find.Anomaly.TryGetUnnaturalCorpseTrackerForAwoken(pawn, out var tracker))
		{
			return null;
		}
		Pawn haunted = tracker.Haunted;
		if (haunted.DestroyedOrNull())
		{
			return null;
		}
		if (!SkipUtility.CanEntitySkipNow(pawn, AbilityDefOf.UnnaturalCorpseSkip))
		{
			return null;
		}
		if (pawn.CanReachImmediate(haunted, PathEndMode.Touch))
		{
			return null;
		}
		if (TryGetSkipCell(pawn, haunted, out var cell))
		{
			return SkipUtility.GetEntitySkipJob(pawn, cell, AbilityDefOf.UnnaturalCorpseSkip);
		}
		return null;
	}

	protected virtual bool TryGetSkipCell(Pawn pawn, Pawn victim, out IntVec3 cell)
	{
		int maxCells = Mathf.CeilToInt(pawn.GetStatValue(StatDefOf.MoveSpeed) * 40f);
		return TryGetNearbySkipCell(pawn, pawn.PositionHeld, victim, maxCells, out cell);
	}

	protected bool TryGetNearbySkipCell(Pawn pawn, IntVec3 startPos, Pawn victim, int maxCells, out IntVec3 cell)
	{
		return RCellFinder.TryFindRandomCellNearWith(startPos, (IntVec3 x) => ValidateCell(pawn, x, victim.SpawnedParentOrMe, maxCells, pawn.Map), pawn.Map, out cell);
	}

	protected virtual bool ValidateCell(Pawn pawn, IntVec3 start, Thing goal, int maxCells, Map map)
	{
		if (!start.Standable(map) || start.GetFence(map) != null || start.GetDoor(map) != null || start.Fogged(map))
		{
			return false;
		}
		if (start.GetFirstPawn(map) != null)
		{
			return false;
		}
		if (!pawn.Position.InHorDistOf(goal.PositionHeld, maxCells))
		{
			return false;
		}
		if (pawn.Position.InHorDistOf(goal.PositionHeld, 5f))
		{
			return false;
		}
		return pawn.CanReach(start, goal, PathEndMode.Touch, Danger.Deadly);
	}

	protected bool TryGetCellAlongPath(Pawn pawn, Pawn victim, int maxDist, out IntVec3 cell)
	{
		if (pawn.CanReach(pawn.Position, victim.PositionHeld, PathEndMode.Touch, Danger.Deadly) && pawn.pather.Moving && pawn.pather.curPath != null && pawn.pather.curPath.NodesLeftCount > 1)
		{
			for (int i = 1; i < pawn.pather.curPath.NodesLeftCount - 1; i++)
			{
				IntVec3 intVec = pawn.pather.curPath.Peek(i);
				if (ValidateCell(pawn, intVec, victim.SpawnedParentOrMe, maxDist, pawn.Map))
				{
					cell = intVec;
					return true;
				}
			}
		}
		cell = IntVec3.Invalid;
		return false;
	}
}
