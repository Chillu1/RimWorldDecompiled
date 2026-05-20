using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class TileMutatorWorker_LavaLake : TileMutatorWorker_Lake
{
	private const float LavaThreshold = 0.4f;

	private const float VolcanicRockThreshold = 0.25f;

	protected override float LakeRadius => 0.4f;

	public TileMutatorWorker_LavaLake(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (GetValAt(allCell, map) > 0.25f)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		base.GeneratePostTerrain(map);
		CellRect mapRect = CellRect.WholeMap(map);
		HashSet<IntVec3> visited = new HashSet<IntVec3>();
		HashSet<IntVec3> island = new HashSet<IntVec3>();
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (visited.Contains(allCell) || allCell.GetTerrain(map) == TerrainDefOf.LavaDeep)
			{
				continue;
			}
			island.Clear();
			bool touchesMapEdge = false;
			map.floodFiller.FloodFill(allCell, (IntVec3 x) => x.GetTerrain(map) != TerrainDefOf.LavaDeep, delegate(IntVec3 x)
			{
				visited.Add(x);
				island.Add(x);
				if (mapRect.IsOnEdge(x))
				{
					touchesMapEdge = true;
				}
			});
			if (!touchesMapEdge || island.Count < hashSet.Count)
			{
				foreach (IntVec3 item in island)
				{
					map.terrainGrid.SetTerrain(item, TerrainDefOf.LavaDeep);
				}
			}
			else
			{
				if (island.Count <= hashSet.Count)
				{
					continue;
				}
				foreach (IntVec3 item2 in hashSet)
				{
					map.terrainGrid.SetTerrain(item2, TerrainDefOf.LavaDeep);
				}
				hashSet.Clear();
				hashSet.AddRange(island);
			}
		}
	}

	protected override void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell, map);
		if (valAt > 0.4f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.LavaDeep);
		}
		else if (valAt > 0.25f)
		{
			map.terrainGrid.SetTerrain(cell, TerrainDefOf.VolcanicRock);
		}
	}
}
