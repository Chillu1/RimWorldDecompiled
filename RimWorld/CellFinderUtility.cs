using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class CellFinderUtility
{
	private static ByteGrid distToColonyBuilding;

	private static readonly List<IntVec3> tmpColonyBuildingsLocs = new List<IntVec3>();

	private static readonly List<KeyValuePair<IntVec3, float>> tmpDistanceResult = new List<KeyValuePair<IntVec3, float>>();

	public static ByteGrid DistToColonyBuilding => distToColonyBuilding;

	public static void CalculateDistanceToColonyBuildingGrid(Map map)
	{
		if (distToColonyBuilding == null)
		{
			distToColonyBuilding = new ByteGrid(map);
		}
		else if (!distToColonyBuilding.MapSizeMatches(map))
		{
			distToColonyBuilding.ClearAndResizeTo(map);
		}
		distToColonyBuilding.Clear(byte.MaxValue);
		tmpColonyBuildingsLocs.Clear();
		List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
		for (int i = 0; i < allBuildingsColonist.Count; i++)
		{
			Building building = allBuildingsColonist[i];
			if (!building.IsClearableFreeBuilding)
			{
				tmpColonyBuildingsLocs.Add(building.Position);
			}
		}
		Dijkstra<IntVec3>.Run(tmpColonyBuildingsLocs, (IntVec3 x) => DijkstraUtility.AdjacentCellsNeighborsGetter(x, map), (IntVec3 a, IntVec3 b) => (a.x == b.x || a.z == b.z) ? 1f : 1.4142135f, tmpDistanceResult);
		for (int num = 0; num < tmpDistanceResult.Count; num++)
		{
			distToColonyBuilding[tmpDistanceResult[num].Key] = (byte)Mathf.Min(tmpDistanceResult[num].Value, 254.999f);
		}
	}
}
