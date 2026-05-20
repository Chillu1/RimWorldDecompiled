using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_CoastalAtoll : TileMutatorWorker_Coast
{
	private ModuleBase innerWaterNoise;

	private const float WaterRadius = 0.95f;

	private const float WaterSquash = 0.8f;

	private const float IslandRadius = 0.65f;

	private const float IslandInnerRadius = 0.5f;

	private const float IslandMacroNoiseFrequency = 0.003f;

	private const float IslandMacroNoiseStrength = 20f;

	private const float IslandNoiseFrequency = 0.015f;

	private const float IslandNoiseStrength = 5f;

	private const float IslandMaxInnerOffset = 0.08f;

	protected override FloatRange CoastOffset => new FloatRange(0.6f);

	protected override float MaxForSand => 0.53f;

	public TileMutatorWorker_CoastalAtoll(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			ModuleBase input = new DistFromPoint((float)map.Size.x * 0.95f);
			input = new Clamp(MaxForDeepWater, 1.0, input);
			input = new Scale(1.0, 1.0, 0.800000011920929, input);
			input = new Rotate(0.0, GetCoastAngle(map.Tile), 0.0, input);
			input = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input);
			input = MapNoiseUtility.AddDisplacementNoise(input, 0.015f, 25f);
			ModuleBase input2 = new DistFromPoint((float)map.Size.x * 0.65f);
			input2 = new ScaleBias(-1.0, 1.0, input2);
			input2 = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input2);
			input2 = MapNoiseUtility.AddDisplacementNoise(input2, 0.003f, 20f, 4, map.Tile.tileId);
			input2 = MapNoiseUtility.AddDisplacementNoise(input2, 0.015f, 5f, 6, map.Tile.tileId);
			ModuleBase input3 = new DistFromPoint((float)map.Size.x * 0.5f);
			input3 = new Clamp(MaxForDeepWater, 1.0, input3);
			input3 = new Translate((float)(-map.Size.x) * (0.5f + Rand.Range(-0.08f, 0.08f)), 0.0, (float)(-map.Size.z) * (0.5f + Rand.Range(-0.08f, 0.08f)), input3);
			input3 = MapNoiseUtility.AddDisplacementNoise(input3, 0.003f, 20f, 4, map.Tile.tileId);
			input3 = MapNoiseUtility.AddDisplacementNoise(input3, 0.015f, 5f, 6, map.Tile.tileId);
			coastNoise = new SmoothMin(coastNoise, input, 0.5);
			coastNoise = new Max(coastNoise, input2);
			coastNoise = new Min(coastNoise, input3);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "atoll & coast");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (coastNoise.GetValue(allCell) < MaxForSand)
			{
				elevation[allCell] = 0f;
			}
		}
	}
}
