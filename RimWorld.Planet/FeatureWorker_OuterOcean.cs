using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public class FeatureWorker_OuterOcean : FeatureWorker
	{
		private List<int> group = new List<int>();

		private List<int> edgeTiles = new List<int>();

		public override void GenerateWhereAppropriate()
		{
			WorldGrid worldGrid = Find.WorldGrid;
			int tilesCount = worldGrid.TilesCount;
			edgeTiles.Clear();
			for (int i = 0; i < tilesCount; i++)
			{
				if (IsRoot(i))
				{
					edgeTiles.Add(i);
				}
			}
			if (edgeTiles.Any())
			{
				group.Clear();
				Find.WorldFloodFiller.FloodFill(-1, (int x) => CanTraverse(x), delegate(int tile, int traversalDist)
				{
					group.Add(tile);
					return false;
				}, int.MaxValue, edgeTiles);
				group.RemoveAll((int x) => worldGrid[x].feature != null);
				if (group.Count >= def.minSize && group.Count <= def.maxSize)
				{
					AddFeature(group, group);
				}
			}
		}

		private bool IsRoot(int tile)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			if (worldGrid.IsOnEdge(tile) && CanTraverse(tile))
			{
				return worldGrid[tile].feature == null;
			}
			return false;
		}

		private bool CanTraverse(int tile)
		{
			BiomeDef biome = Find.WorldGrid[tile].biome;
			if (biome != BiomeDefOf.Ocean)
			{
				return biome == BiomeDefOf.Lake;
			}
			return true;
		}
	}
}
