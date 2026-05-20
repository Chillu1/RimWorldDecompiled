using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_LakeWithIslands : TileMutatorWorker_Lake
{
	private const float IslandsPerlinFreq = 0.02f;

	private const float IslandsPerlinLac = 2f;

	private const float IslandsPerlinPers = 0.5f;

	private const int IslandsPerlinOctaves = 2;

	private const float IslandsScale = 0.6f;

	private const float IslandsBias = 0.7f;

	private const float FalloffScale = 2f;

	private const float FalloffBias = -1f;

	private const float FalloffExp = 0.45f;

	protected override float LakeRadius => 0.75f;

	protected override bool GenerateDeepWater => false;

	public TileMutatorWorker_LakeWithIslands(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		base.Init(map);
		ModuleBase input = new Perlin(0.019999999552965164, 2.0, 0.5, 2, Rand.Int, QualityMode.Medium);
		input = new ScaleBias(0.6000000238418579, 0.699999988079071, input);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.015f, 15f);
		NoiseDebugUI.StoreNoiseRender(input, "island base");
		ModuleBase input2 = new ScaleBias(2.0, -1.0, lakeNoise);
		input2 = new Clamp(0.0, 1.0, input2);
		input2 = new Power(input2, new Const(0.44999998807907104));
		NoiseDebugUI.StoreNoiseRender(input2, "island falloff");
		input = new Multiply(input, input2);
		input = new ScaleBias(-1.0, 1.0, input);
		NoiseDebugUI.StoreNoiseRender(input, "island Noise");
		lakeNoise = new Min(lakeNoise, input);
		NoiseDebugUI.StoreNoiseRender(lakeNoise, "lake + island Noise");
	}
}
