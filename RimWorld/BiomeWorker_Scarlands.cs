using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class BiomeWorker_Scarlands : BiomeWorker
{
	private Perlin perlin;

	private static readonly List<PlanetTile> SearchList = new List<PlanetTile>();

	private const float NoiseThreshold = 0.9f;

	private const int SeedPart = 417235213;

	public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
	{
		if (tile.WaterCovered)
		{
			return -100f;
		}
		if (!BiomeWorker_TemperateForest.Allowed(tile) || !AllowedAt(planetTile))
		{
			return 0f;
		}
		return BiomeWorker_TemperateForest.Score(tile) + 20f;
	}

	private bool AllowedAt(PlanetTile tileID)
	{
		if (NoiseAt(tileID) < 0.9f)
		{
			return false;
		}
		Find.WorldGrid.GetTileNeighbors(tileID, SearchList);
		if (SearchList.Empty())
		{
			return false;
		}
		foreach (PlanetTile search in SearchList)
		{
			if (NoiseAt(search) >= 0.9f)
			{
				return true;
			}
		}
		return false;
	}

	private float NoiseAt(PlanetTile tileID)
	{
		if (perlin == null)
		{
			perlin = new Perlin(0.20000000298023224, 0.5, 0.10000000149011612, 4, normalized: true, invert: false, Gen.HashCombineInt(Find.World.info.Seed, 417235213), QualityMode.Medium);
		}
		Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
		return (float)perlin.GetValue(tileCenter.x, tileCenter.y, tileCenter.z);
	}
}
