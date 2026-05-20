using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Coast : TileMutatorWorker
{
	protected ModuleBase coastNoise;

	protected const float CoastMacroNoiseFrequency = 0.006f;

	protected const float CoastMacroNoiseStrength = 30f;

	protected const float CoastNoiseFrequency = 0.015f;

	protected const float CoastNoiseStrength = 25f;

	protected virtual float MaxForDeepWater => 0.4f;

	protected virtual float MaxForShallowWater => 0.5f;

	protected virtual float MaxForSand => 0.6f;

	protected virtual FloatRange CoastOffset => new FloatRange(0.1f, 0.2f);

	public TileMutatorWorker_Coast(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		float coastAngle = GetCoastAngle(map.Tile);
		coastNoise = MapNoiseUtility.FalloffAtAngle(coastAngle, CoastOffset.RandomInRange, map);
		coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.006f, 30f, 2);
		coastNoise = MapNoiseUtility.AddDisplacementNoise(coastNoise, 0.015f, 25f);
		NoiseDebugUI.StoreNoiseRender(coastNoise, "coast");
	}

	protected virtual float GetCoastAngle(PlanetTile tile)
	{
		return Find.World.CoastAngleAt(tile, BiomeDefOf.Ocean).GetValueOrDefault();
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (GetNoiseValue(allCell) < MaxForDeepWater)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			TerrainDef terrainDef = CoastTerrainAt(allCell, map);
			if (terrainDef != null && (allCell.GetTerrain(map).categoryType != TerrainDef.TerrainCategoryType.Stone || (allCell.GetEdifice(map) == null && terrainDef.IsWater)))
			{
				map.terrainGrid.SetTerrain(allCell, terrainDef);
			}
		}
	}

	protected virtual float GetNoiseValue(IntVec3 cell)
	{
		return coastNoise.GetValue(cell);
	}

	protected virtual TerrainDef DeepWaterTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.DeepOceanWaterTerrainAt(cell, map);
	}

	protected virtual TerrainDef ShallowWaterTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.ShallowOceanWaterTerrainAt(cell, map);
	}

	protected virtual TerrainDef BeachTerrainAt(IntVec3 cell, Map map)
	{
		return MapGenUtility.BeachTerrainAt(cell, map);
	}

	protected virtual TerrainDef CoastTerrainAt(IntVec3 cell, Map map)
	{
		if (coastNoise == null)
		{
			return null;
		}
		float noiseValue = GetNoiseValue(cell);
		if (noiseValue < MaxForDeepWater)
		{
			return DeepWaterTerrainAt(cell, map);
		}
		if (noiseValue < MaxForShallowWater)
		{
			return ShallowWaterTerrainAt(cell, map);
		}
		if (noiseValue < MaxForSand && MapGenUtility.ShouldGenerateBeachSand(cell, map))
		{
			return BeachTerrainAt(cell, map);
		}
		return null;
	}
}
