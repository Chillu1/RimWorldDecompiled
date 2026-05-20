using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class GenStep_RemoveTinyIslands : GenStep
{
	private static HashSet<IntVec3> tmpVisited = new HashSet<IntVec3>();

	private static List<IntVec3> tmpIsland = new List<IntVec3>();

	public override int SeedPart => 56712394;

	public override void Generate(Map map, GenStepParams parms)
	{
		CellRect mapRect = CellRect.WholeMap(map);
		int num = 0;
		tmpVisited.Clear();
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (tmpVisited.Contains(allCell) || Impassable(allCell))
			{
				continue;
			}
			int area = 0;
			bool touchesMapEdge = false;
			map.floodFiller.FloodFill(allCell, (IntVec3 x) => !Impassable(x), delegate(IntVec3 x)
			{
				tmpVisited.Add(x);
				area++;
				if (mapRect.IsOnEdge(x))
				{
					touchesMapEdge = true;
				}
			});
			if (touchesMapEdge)
			{
				num = Mathf.Max(num, area);
			}
		}
		if (num < 30)
		{
			return;
		}
		tmpVisited.Clear();
		foreach (IntVec3 allCell2 in map.AllCells)
		{
			if (tmpVisited.Contains(allCell2) || Impassable(allCell2))
			{
				continue;
			}
			tmpIsland.Clear();
			TerrainDef adjacentImpassableTerrain = null;
			bool touchesMapEdge2 = false;
			map.floodFiller.FloodFill(allCell2, delegate(IntVec3 x)
			{
				if (Impassable(x))
				{
					adjacentImpassableTerrain = x.GetTerrain(map);
					return false;
				}
				return true;
			}, delegate(IntVec3 x)
			{
				tmpVisited.Add(x);
				tmpIsland.Add(x);
				if (mapRect.IsOnEdge(x))
				{
					touchesMapEdge2 = true;
				}
			});
			if ((tmpIsland.Count <= num / 20 || (!touchesMapEdge2 && tmpIsland.Count < num / 2)) && adjacentImpassableTerrain != null)
			{
				for (int num2 = 0; num2 < tmpIsland.Count; num2++)
				{
					map.terrainGrid.SetTerrain(tmpIsland[num2], adjacentImpassableTerrain);
				}
			}
		}
		bool Impassable(IntVec3 x)
		{
			return x.GetTerrain(map).passability == Traversability.Impassable;
		}
	}
}
