using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld.Planet;

public static class MultipleCaravansCellFinder
{
	private const int TriesToFindPerfectOppositeSpots = 10;

	private const int TriesToFindGoodEnoughOppositeSpots = 10;

	private const int TriesToFindMatchingPair = 20;

	private const float PerfectIfDistPctToOppositeSpotsAtMost = 0.05f;

	private const float GoodEnoughIfDistPctToOppositeSpotsAtMost = 0.15f;

	private const float SpotDistPctToEdge = 0.2f;

	private const float TryMinDistPctBetweenFallbackEdgeCells = 0.6f;

	public static void FindStartingCellsFor2Groups(Map map, out IntVec3 first, out IntVec3 second)
	{
		for (int i = 0; i < 10; i++)
		{
			if (TryFindOppositeSpots(map, 0.05f, out first, out second))
			{
				return;
			}
		}
		for (int j = 0; j < 10; j++)
		{
			if (TryFindOppositeSpots(map, 0.15f, out first, out second))
			{
				return;
			}
		}
		if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, CellFinder.EdgeRoadChance_Neutral, out first))
		{
			Log.Error("Could not find any valid starting cell for a caravan.");
			first = CellFinder.RandomCell(map);
			second = CellFinder.RandomCell(map);
			return;
		}
		IntVec3 localFirst = first;
		float tryMinDistBetweenSpots = (float)Mathf.Max(map.Size.x, map.Size.z) * 0.6f;
		TraverseParms traverseParams = TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true);
		if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && !x.InHorDistOf(localFirst, tryMinDistBetweenSpots) && map.reachability.CanReach(x, localFirst, PathEndMode.OnCell, traverseParams), map, CellFinder.EdgeRoadChance_Neutral, out second) && !CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map) && map.reachability.CanReach(x, localFirst, PathEndMode.OnCell, traverseParams), map, 0.5f, out second))
		{
			Log.Error("Could not find any valid starting cell for a caravan.");
			second = CellFinder.RandomCell(map);
		}
		else
		{
			first = CellFinder.RandomClosewalkCellNear(first, map, 7);
			second = CellFinder.RandomClosewalkCellNear(second, map, 7);
		}
	}

	private static bool TryFindOppositeSpots(Map map, float maxDistPctToOppositeSpots, out IntVec3 first, out IntVec3 second)
	{
		IntVec3 intVec = RandomSpotNearEdge(map);
		IntVec3 intVec2 = OppositeSpot(intVec, map);
		int num = Mathf.Min(map.Size.x, map.Size.z);
		CellRect cellRect = CellRect.CenteredOn(intVec, Mathf.Max(Mathf.RoundToInt((float)num * maxDistPctToOppositeSpots), 1)).ClipInsideMap(map);
		CellRect cellRect2 = CellRect.CenteredOn(intVec2, Mathf.Max(Mathf.RoundToInt((float)num * maxDistPctToOppositeSpots), 1)).ClipInsideMap(map);
		for (int i = 0; i < 20; i++)
		{
			IntVec3 intVec3 = ((i == 0) ? intVec : cellRect.RandomCell);
			IntVec3 intVec4 = ((i == 0) ? intVec2 : cellRect2.RandomCell);
			if (intVec3.Standable(map) && !intVec3.Fogged(map) && intVec4.Standable(map) && !intVec4.Fogged(map) && map.reachability.CanReach(intVec3, intVec4, PathEndMode.OnCell, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true)))
			{
				first = intVec3;
				second = intVec4;
				return true;
			}
		}
		first = IntVec3.Invalid;
		second = IntVec3.Invalid;
		return false;
	}

	private static IntVec3 RandomSpotNearEdge(Map map)
	{
		CellRect cellRect = CellRect.WholeMap(map);
		cellRect.minX += Mathf.RoundToInt((float)map.Size.x * 0.2f);
		cellRect.minZ += Mathf.RoundToInt((float)map.Size.z * 0.2f);
		cellRect.maxX -= Mathf.RoundToInt((float)map.Size.x * 0.2f);
		cellRect.maxZ -= Mathf.RoundToInt((float)map.Size.z * 0.2f);
		return cellRect.EdgeCells.RandomElement();
	}

	private static IntVec3 OppositeSpot(IntVec3 spot, Map map)
	{
		return new IntVec3(map.Size.x - spot.x, spot.y, map.Size.z - spot.z);
	}
}
