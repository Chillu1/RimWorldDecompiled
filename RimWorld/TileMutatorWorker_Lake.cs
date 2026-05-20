using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Lake : TileMutatorWorker
{
	protected ModuleBase lakeNoise;

	public const float LakeNoiseFrequency = 0.015f;

	public const float LakeNoiseStrength = 15f;

	public const float LakeMacroNoiseFrequency = 0.006f;

	public const float LakeMacroNoiseStrength = 40f;

	public const float DeepWaterThreshold = 0.75f;

	public const float WaterThreshold = 0.5f;

	public const float BeachThreshold = 0.45f;

	private const float LakeMaxOffset = 0.4f;

	public static readonly FloatRange LakeSquashRange = new FloatRange(1f, 1.3f);

	protected virtual float LakeRadius => 0.6f;

	protected virtual float LakeFalloffDecay => 1f;

	protected virtual bool GenerateDeepWater => true;

	public TileMutatorWorker_Lake(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			map.waterInfo.lakeCenter = GetLakeCenter(map);
			lakeNoise = new DistFromPoint((float)map.Size.x * LakeRadius);
			lakeNoise = new ScaleBias(-1.0, 1.0, lakeNoise);
			lakeNoise = new Scale(LakeSquashRange.RandomInRange, 1.0, 1.0, lakeNoise);
			lakeNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, lakeNoise);
			lakeNoise = new Translate(-map.waterInfo.lakeCenter.x, 0.0, -map.waterInfo.lakeCenter.z, lakeNoise);
			lakeNoise = new Clamp(0.0, 1.0, lakeNoise);
			lakeNoise = new Power(lakeNoise, new Const(LakeFalloffDecay));
			lakeNoise = MapNoiseUtility.AddDisplacementNoise(lakeNoise, 0.006f, 40f, 2, map.Tile.tileId);
			lakeNoise = MapNoiseUtility.AddDisplacementNoise(lakeNoise, 0.015f, 15f, 4, map.Tile.tileId);
			NoiseDebugUI.StoreNoiseRender(lakeNoise, "lake");
		}
	}

	protected virtual IntVec3 GetLakeCenter(Map map)
	{
		if (Find.World.landmarks.landmarks.TryGetValue(map.Tile, out var value) && value.isComboLandmark)
		{
			return map.Center;
		}
		Rand.PushState(map.Tile.tileId);
		IntVec3 result = new IntVec3(map.Center.x + Mathf.RoundToInt((float)map.Size.x * Rand.Range(-0.4f, 0.4f)), 0, map.Center.z + Mathf.RoundToInt((float)map.Size.z * Rand.Range(-0.4f, 0.4f)));
		Rand.PopState();
		return result;
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (GetValAt(allCell, map) > 0.45f)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			ProcessCell(allCell, map);
		}
	}

	protected virtual void ProcessCell(IntVec3 cell, Map map)
	{
		float valAt = GetValAt(cell, map);
		if (GenerateDeepWater && valAt > 0.75f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.DeepFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.5f)
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.ShallowFreshWaterTerrainAt(cell, map));
		}
		else if (valAt > 0.45f && MapGenUtility.ShouldGenerateBeachSand(cell, map))
		{
			map.terrainGrid.SetTerrain(cell, MapGenUtility.LakeshoreTerrainAt(cell, map));
		}
	}

	protected virtual float GetValAt(IntVec3 cell, Map map)
	{
		return (float)lakeNoise.GetValue(cell.x, 0.0, cell.z);
	}
}
