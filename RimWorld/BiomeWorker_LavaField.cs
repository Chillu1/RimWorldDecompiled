using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class BiomeWorker_LavaField : BiomeWorker
{
	private Perlin perlin;

	private const float NoiseThreshold = 0.9f;

	private const int SeedPart = 967525656;

	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (!AllowedAt(planetTile))
		{
			return 0f;
		}
		float num;
		if (BiomeWorker_Desert.Allowed(tile))
		{
			num = BiomeWorker_Desert.Score(tile) - 0.0001f;
		}
		else
		{
			if (!BiomeWorker_AridShrubland.Allowed(tile))
			{
				return 0f;
			}
			num = BiomeWorker_AridShrubland.Score(tile) - 0.0001f;
		}
		if ((int)tile.hilliness >= 2)
		{
			num += 0.0002f;
		}
		return num;
	}

	private bool AllowedAt(PlanetTile tile)
	{
		if (perlin == null)
		{
			perlin = new Perlin(0.30000001192092896, 0.5, 0.5, 4, normalized: true, invert: false, Gen.HashCombineInt(Find.World.info.Seed, 967525656), QualityMode.Medium);
		}
		Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
		return (float)perlin.GetValue(tileCenter.x, tileCenter.y, tileCenter.z) >= 0.9f;
	}
}
