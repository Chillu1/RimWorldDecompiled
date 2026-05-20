using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_HotSprings : TileMutatorWorker
{
	private const float ShapeNoiseFrequency = 0.015f;

	private const float ShapeNoiseStrength = 25f;

	private const float NoiseFrequency = 0.05f;

	private const float NoiseLacunarity = 1.5f;

	private const float NoisePersistence = 0.5f;

	private const int NoiseOctaves = 4;

	private const float PoolsRadius = 0.35f;

	private const float SpringThreshold = 0.85f;

	private const float StoneThreshold = 0.65f;

	private const float FalloffExp = 0.05f;

	private ModuleBase springNoise;

	public TileMutatorWorker_HotSprings(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			springNoise = new Perlin(0.05000000074505806, 1.5, 0.5, 4, Rand.Int, QualityMode.Medium);
			springNoise = new ScaleBias(0.5, 0.5, springNoise);
			ModuleBase baseShape = MapNoiseUtility.CreateFalloffRadius((float)map.Size.x * 0.35f, map.Center.ToVector2(), 0.05f);
			baseShape = MapNoiseUtility.AddDisplacementNoise(baseShape, 0.015f, 25f, 1);
			NoiseDebugUI.StoreNoiseRender(baseShape, "hot springs area");
			springNoise = new Multiply(springNoise, baseShape);
			NoiseDebugUI.StoreNoiseRender(springNoise, "hot springs");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (springNoise.GetValue(allCell) > 0.65f)
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = springNoise.GetValue(allCell);
			if (value > 0.85f)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.HotSpring);
			}
			else if (value > 0.65f)
			{
				map.terrainGrid.SetTerrain(allCell, GenStep_RocksFromGrid.RockDefAt(allCell).building.naturalTerrain);
			}
		}
	}
}
