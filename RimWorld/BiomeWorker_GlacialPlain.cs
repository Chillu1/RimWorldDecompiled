using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class BiomeWorker_GlacialPlain : BiomeWorker
{
	private Perlin perlin;

	private const float NoiseThreshold = 0.9f;

	private const int SeedPart = 44319114;

	private const float HillinessSmallHillsElevation = 10f;

	private const float HillinessLargeHillsElevation = 15f;

	private const float HillinessMountainousElevation = 18f;

	private const float HillinessImpassableElevation = 18f;

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
		if (tile.rainfall <= 600f)
		{
			return 0f;
		}
		float num = 0f;
		if (tile.hilliness == Hilliness.SmallHills)
		{
			num = 10f;
		}
		else if (tile.hilliness == Hilliness.LargeHills)
		{
			num = 15f;
		}
		else if (tile.hilliness == Hilliness.Mountainous)
		{
			num = 18f;
		}
		else if (tile.hilliness == Hilliness.Impassable)
		{
			num = 18f;
		}
		return 15f + (0f - tile.temperature) + num;
	}

	private bool AllowedAt(PlanetTile tileID)
	{
		return NoiseAt(tileID) >= 0.9f;
	}

	private float NoiseAt(PlanetTile tileID)
	{
		if (perlin == null)
		{
			perlin = new Perlin(0.30000001192092896, 0.4000000059604645, 0.5, 5, normalized: true, invert: false, Gen.HashCombineInt(Find.World.info.Seed, 44319114), QualityMode.Medium);
		}
		Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
		return (float)perlin.GetValue(tileCenter.x, tileCenter.y, tileCenter.z);
	}
}
