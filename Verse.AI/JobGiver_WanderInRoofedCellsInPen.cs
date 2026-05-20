using System;
using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public class JobGiver_WanderInRoofedCellsInPen : JobGiver_Wander
{
	private const int PenRegionsSampleSize = 4;

	private static readonly List<Region> wanderRootRegions = new List<Region>();

	public JobGiver_WanderInRoofedCellsInPen()
	{
		wanderRadius = 10f;
		wanderDestValidator = (Pawn pawn, IntVec3 cell, IntVec3 root) => cell.Roofed(pawn.Map);
	}

	protected override IntVec3 GetExactWanderDest(Pawn pawn)
	{
		Map map = pawn.Map;
		if (!ShouldSeekRoofedCells(pawn))
		{
			return IntVec3.Invalid;
		}
		CompAnimalPenMarker currentPenOf = AnimalPenUtility.GetCurrentPenOf(pawn, allowUnenclosedPens: false);
		if (currentPenOf == null)
		{
			return IntVec3.Invalid;
		}
		IntVec3 intVec = pawn.Position;
		if (!intVec.Roofed(map))
		{
			wanderRootRegions.Clear();
			wanderRootRegions.Add(pawn.GetRegion());
			intVec = FindNearbyRoofedCellIn(pawn.Position, wanderRootRegions, map);
		}
		if (!intVec.IsValid || !intVec.Roofed(map))
		{
			wanderRootRegions.Clear();
			wanderRootRegions.AddRange(currentPenOf.PenState.ConnectedRegions);
			wanderRootRegions.Shuffle();
			wanderRootRegions.TruncateToLength(4);
			intVec = FindNearbyRoofedCellIn(pawn.Position, wanderRootRegions, map);
		}
		wanderRootRegions.Clear();
		if (!intVec.IsValid)
		{
			return IntVec3.Invalid;
		}
		return RCellFinder.RandomWanderDestFor(pawn, intVec, wanderRadius, wanderDestValidator, PawnUtility.ResolveMaxDanger(pawn, maxDanger));
	}

	protected override IntVec3 GetWanderRoot(Pawn pawn)
	{
		throw new NotImplementedException();
	}

	private static bool ShouldSeekRoofedCells(Pawn pawn)
	{
		foreach (GameCondition activeCondition in pawn.Map.GameConditionManager.ActiveConditions)
		{
			if (activeCondition.def.pennedAnimalsSeekShelter)
			{
				return true;
			}
		}
		return false;
	}

	private static IntVec3 FindNearbyRoofedCellIn(IntVec3 root, List<Region> regions, Map map)
	{
		IntVec3 result = IntVec3.Invalid;
		int num = int.MaxValue;
		foreach (Region region in regions)
		{
			if (region.TryFindRandomCellInRegion(SafeCell, out var result2))
			{
				int num2 = root.DistanceToSquared(result2);
				if (num2 < num)
				{
					result = result2;
					num = num2;
				}
				if (num <= 100)
				{
					return result;
				}
			}
		}
		return result;
		bool SafeCell(IntVec3 cell)
		{
			return cell.Roofed(map);
		}
	}
}
