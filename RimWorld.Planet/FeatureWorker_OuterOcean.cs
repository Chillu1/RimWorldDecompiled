using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class FeatureWorker_OuterOcean : FeatureWorker
{
	private readonly List<PlanetTile> group = new List<PlanetTile>();

	private readonly List<PlanetTile> edgeTiles = new List<PlanetTile>();

	public override void GenerateWhereAppropriate(PlanetLayer layer)
	{
		edgeTiles.Clear();
		for (int i = 0; i < layer.TilesCount; i++)
		{
			PlanetTile planetTile = new PlanetTile(i, layer);
			if (IsRoot(planetTile))
			{
				edgeTiles.Add(planetTile);
			}
		}
		if (edgeTiles.Any())
		{
			group.Clear();
			layer.Filler.FloodFill(PlanetTile.Invalid, CanTraverse, delegate(PlanetTile tile, int traversalDist)
			{
				group.Add(tile);
				return false;
			}, int.MaxValue, edgeTiles);
			group.RemoveAll((PlanetTile x) => layer[x].feature != null);
			if (group.Count >= def.minSize && group.Count <= def.maxSize)
			{
				AddFeature(layer, group, group);
			}
		}
	}

	private bool IsRoot(PlanetTile tile)
	{
		if (Find.WorldGrid.IsOnEdge(tile) && CanTraverse(tile))
		{
			return Find.WorldGrid[tile].feature == null;
		}
		return false;
	}

	private bool CanTraverse(PlanetTile tile)
	{
		BiomeDef primaryBiome = Find.WorldGrid[tile].PrimaryBiome;
		if (primaryBiome != BiomeDefOf.Ocean)
		{
			return primaryBiome == BiomeDefOf.Lake;
		}
		return true;
	}
}
