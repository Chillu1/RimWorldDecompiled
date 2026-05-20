using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class GenPlanetMorphology
{
	private static readonly HashSet<PlanetTile> tmpOutput = new HashSet<PlanetTile>();

	private static readonly HashSet<PlanetTile> tilesSet = new HashSet<PlanetTile>();

	private static readonly List<PlanetTile> tmpEdgeTiles = new List<PlanetTile>();

	private static readonly List<PlanetTile> tmpNeighbors = new List<PlanetTile>();

	public static void Erode(PlanetLayer layer, List<PlanetTile> tiles, int count, Predicate<PlanetTile> extraPredicate = null)
	{
		if (count <= 0)
		{
			return;
		}
		WorldGrid worldGrid = Find.WorldGrid;
		tilesSet.Clear();
		tilesSet.AddRange(tiles);
		tmpEdgeTiles.Clear();
		for (int i = 0; i < tiles.Count; i++)
		{
			worldGrid.GetTileNeighbors(tiles[i], tmpNeighbors);
			for (int j = 0; j < tmpNeighbors.Count; j++)
			{
				if (!tilesSet.Contains(tmpNeighbors[j]))
				{
					tmpEdgeTiles.Add(tiles[i]);
					break;
				}
			}
		}
		if (!tmpEdgeTiles.Any())
		{
			return;
		}
		tmpOutput.Clear();
		Predicate<PlanetTile> passCheck = ((extraPredicate == null) ? ((Predicate<PlanetTile>)((PlanetTile x) => tilesSet.Contains(x))) : ((Predicate<PlanetTile>)((PlanetTile x) => tilesSet.Contains(x) && extraPredicate(x))));
		layer.Filler.FloodFill(PlanetTile.Invalid, passCheck, delegate(PlanetTile tile, int traversalDist)
		{
			if (traversalDist >= count)
			{
				tmpOutput.Add(tile);
			}
			return false;
		}, int.MaxValue, tmpEdgeTiles);
		tiles.Clear();
		tiles.AddRange(tmpOutput);
	}

	public static void Dilate(PlanetLayer layer, List<PlanetTile> tiles, int count, Predicate<PlanetTile> extraPredicate = null)
	{
		if (count <= 0)
		{
			return;
		}
		layer.Filler.FloodFill(PlanetTile.Invalid, extraPredicate ?? ((Predicate<PlanetTile>)((PlanetTile x) => true)), delegate(PlanetTile tile, int traversalDist)
		{
			if (traversalDist > count)
			{
				return true;
			}
			if (traversalDist != 0)
			{
				tiles.Add(tile);
			}
			return false;
		}, int.MaxValue, tiles);
	}

	public static void Open(PlanetLayer layer, List<PlanetTile> tiles, int count)
	{
		Erode(layer, tiles, count);
		Dilate(layer, tiles, count);
	}

	public static void Close(PlanetLayer layer, List<PlanetTile> tiles, int count)
	{
		Dilate(layer, tiles, count);
		Erode(layer, tiles, count);
	}
}
