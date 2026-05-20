using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_LavaCrater : TileMutatorWorker_LavaLake
{
	private const float RockThreshold = 0.15f;

	private const float NoiseFrequency = 0.015f;

	private ModuleBase craterNoise;

	public TileMutatorWorker_LavaCrater(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		base.Init(map);
		ModuleBase lhs = new Perlin(0.014999999664723873, 2.0, 0.5, 6, Rand.Int, QualityMode.Medium);
		ModuleBase input = new DistFromPoint((float)map.Size.x * LakeRadius);
		input = new Clamp(0.0, 1.0, input);
		input = new Invert(input);
		input = new ScaleBias(1.0, 1.0, input);
		input = new Translate(-map.waterInfo.lakeCenter.x, 0.0, -map.waterInfo.lakeCenter.z, input);
		craterNoise = new Blend(lhs, input, new Const(0.7));
		NoiseDebugUI.StoreNoiseRender(craterNoise, "crater");
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if ((float)craterNoise.GetValue(allCell.x, 0.0, allCell.z) > 0.15f)
			{
				elevation[allCell] = 1f;
			}
		}
		base.GeneratePostElevationFertility(map);
	}
}
