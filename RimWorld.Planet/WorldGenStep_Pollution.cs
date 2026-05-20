using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet;

public class WorldGenStep_Pollution : WorldGenStep
{
	private const float MinPollution = 0.25f;

	private const float MaxPollution = 1f;

	private const float PerlinFrequency = 0.1f;

	private readonly List<PlanetTile> tmpTiles = new List<PlanetTile>();

	private readonly Dictionary<PlanetTile, float> tmpTileNoise = new Dictionary<PlanetTile, float>();

	public override int SeedPart => 759372056;

	public override void GenerateFresh(string seed, PlanetLayer layer)
	{
		float pollution = Find.World.info.pollution;
		if (pollution <= 0f)
		{
			for (int i = 0; i < layer.TilesCount; i++)
			{
				Tile tile = layer[i];
				float pollutionOffset = tile.PrimaryBiome.pollutionOffset;
				tile.pollution = Mathf.Clamp01(pollutionOffset);
			}
			return;
		}
		Perlin perlin = new Perlin(0.10000000149011612, 2.0, 0.5, 6, seed.GetHashCode(), QualityMode.Medium);
		tmpTiles.Clear();
		tmpTileNoise.Clear();
		foreach (Tile tile3 in layer.Tiles)
		{
			if (tile3.PrimaryBiome.allowPollution)
			{
				tmpTileNoise.Add(tile3.tile, perlin.GetValue(layer.GetTileCenter(tile3.tile)));
				tmpTiles.Add(tile3.tile);
			}
		}
		tmpTiles.SortByDescending((PlanetTile t) => tmpTileNoise[t]);
		int num = Mathf.RoundToInt((float)tmpTiles.Count * pollution);
		for (int num2 = 0; num2 < num; num2++)
		{
			Tile tile2 = layer[tmpTiles[num2]];
			float value = Mathf.Lerp(0.25f, 1f, tmpTileNoise[tmpTiles[num2]]);
			tile2.pollution = Mathf.Clamp01(value);
		}
		tmpTiles.Clear();
		tmpTileNoise.Clear();
	}
}
