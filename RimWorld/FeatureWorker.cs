using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public abstract class FeatureWorker
{
	public FeatureDef def;

	protected static bool[] visited;

	protected static int[] groupSize;

	protected static int[] groupID;

	private static readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	private static readonly HashSet<PlanetTile> tmpTilesForTextDrawPosCalculationSet = new HashSet<PlanetTile>();

	private static readonly List<PlanetTile> tmpEdgeTiles = new List<PlanetTile>();

	private static readonly List<(PlanetTile tile, int traversalDistance)> tmpTraversedTiles = new List<(PlanetTile, int)>();

	public abstract void GenerateWhereAppropriate(PlanetLayer layer);

	protected void AddFeature(PlanetLayer layer, List<PlanetTile> members, List<PlanetTile> tilesForTextDrawPosCalculation)
	{
		WorldFeature worldFeature = new WorldFeature(def, layer);
		worldFeature.name = NameGenerator.GenerateName(def.nameMaker, Find.WorldFeatures.features.Select((WorldFeature x) => x.name), appendNumberIfNameUsed: false, "r_name");
		WorldGrid worldGrid = Find.WorldGrid;
		for (int num = 0; num < members.Count; num++)
		{
			worldGrid[members[num]].feature = worldFeature;
		}
		AssignBestDrawPos(layer, worldFeature, tilesForTextDrawPosCalculation);
		Find.WorldFeatures.features.Add(worldFeature);
	}

	private void AssignBestDrawPos(PlanetLayer layer, WorldFeature newFeature, List<PlanetTile> tilesForTextDrawPosCalculation)
	{
		WorldGrid worldGrid = Find.WorldGrid;
		tmpEdgeTiles.Clear();
		tmpTilesForTextDrawPosCalculationSet.Clear();
		tmpTilesForTextDrawPosCalculationSet.AddRange(tilesForTextDrawPosCalculation);
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < tilesForTextDrawPosCalculation.Count; i++)
		{
			PlanetTile planetTile = tilesForTextDrawPosCalculation[i];
			zero += worldGrid.GetTileCenter(planetTile);
			bool flag = worldGrid.IsOnEdge(planetTile);
			if (!flag)
			{
				worldGrid.GetTileNeighbors(planetTile, tmpNeighbors);
				for (int j = 0; j < tmpNeighbors.Count; j++)
				{
					if (!tmpTilesForTextDrawPosCalculationSet.Contains(tmpNeighbors[j]))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				tmpEdgeTiles.Add(planetTile);
			}
		}
		zero /= (float)tilesForTextDrawPosCalculation.Count;
		if (!tmpEdgeTiles.Any())
		{
			tmpEdgeTiles.Add(tilesForTextDrawPosCalculation.RandomElement());
		}
		int bestTileDist = 0;
		tmpTraversedTiles.Clear();
		layer.Filler.FloodFill(PlanetTile.Invalid, (Predicate<PlanetTile>)((PlanetTile item) => tmpTilesForTextDrawPosCalculationSet.Contains(item)), (Predicate<PlanetTile, int>)Process, int.MaxValue, (IEnumerable<PlanetTile>)tmpEdgeTiles);
		PlanetTile tile = PlanetTile.Invalid;
		float num = -1f;
		for (int num2 = 0; num2 < tmpTraversedTiles.Count; num2++)
		{
			if (tmpTraversedTiles[num2].traversalDistance == bestTileDist)
			{
				float sqrMagnitude = (worldGrid.GetTileCenter(tmpTraversedTiles[num2].tile) - zero).sqrMagnitude;
				if (!tile.Valid || sqrMagnitude < num)
				{
					tile = tmpTraversedTiles[num2].tile;
					num = sqrMagnitude;
				}
			}
		}
		float maxDrawSizeInTiles = (float)bestTileDist * 2f * 1.2f;
		newFeature.drawCenter = worldGrid.GetTileCenter(tile);
		newFeature.maxDrawSizeInTiles = maxDrawSizeInTiles;
		bool Process(PlanetTile item, int traversalDist)
		{
			tmpTraversedTiles.Add((item, traversalDist));
			bestTileDist = traversalDist;
			return false;
		}
	}

	protected static void ClearVisited(PlanetLayer layer)
	{
		ClearOrCreate(layer, ref visited);
	}

	protected static void ClearGroupSizes(PlanetLayer layer)
	{
		ClearOrCreate(layer, ref groupSize);
	}

	protected static void ClearGroupIDs(PlanetLayer layer)
	{
		ClearOrCreate(layer, ref groupID);
	}

	private static void ClearOrCreate<T>(PlanetLayer layer, ref T[] array)
	{
		int tilesCount = layer.TilesCount;
		if (array == null || array.Length != tilesCount)
		{
			array = new T[tilesCount];
		}
		else
		{
			Array.Clear(array, 0, array.Length);
		}
	}
}
