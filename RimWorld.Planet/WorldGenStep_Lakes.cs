using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public class WorldGenStep_Lakes : WorldGenStep
{
	private const int LakeMaxSize = 15;

	public override int SeedPart => 401463656;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		GenerateLakes(layer);
	}

	private void GenerateLakes(PlanetLayer layer)
	{
		bool[] touched = new bool[layer.TilesCount];
		List<int> oceanChunk = new List<int>();
		foreach (Tile tile2 in layer.Tiles)
		{
			PlanetTile tile = tile2.tile;
			if (touched[tile.tileId] || layer[tile.tileId].PrimaryBiome != BiomeDefOf.Ocean)
			{
				continue;
			}
			layer.Filler.FloodFill(tile, (PlanetTile tid) => layer[tid].PrimaryBiome == BiomeDefOf.Ocean, delegate(PlanetTile planetTile)
			{
				oceanChunk.Add(planetTile.tileId);
				touched[planetTile.tileId] = true;
			});
			if (oceanChunk.Count <= 15)
			{
				for (int num = 0; num < oceanChunk.Count; num++)
				{
					layer[oceanChunk[num]].PrimaryBiome = BiomeDefOf.Lake;
				}
			}
			oceanChunk.Clear();
		}
	}
}
