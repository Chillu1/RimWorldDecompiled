using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Archipelago : TileMutatorWorker_Coast
{
	private const float ArchNoiseFrequency = 0.02f;

	private ModuleBase archNoise;

	protected override float MaxForDeepWater => 0.2f;

	protected override FloatRange CoastOffset => new FloatRange(0.2f);

	public TileMutatorWorker_Archipelago(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			archNoise = new Perlin(0.019999999552965164, 2.0, 0.5, 2, Rand.Int, QualityMode.Medium);
			archNoise = new ScaleBias(0.5, 0.5, archNoise);
			archNoise = MapNoiseUtility.AddDisplacementNoise(archNoise, 0.015f, 25f);
			NoiseDebugUI.StoreNoiseRender(archNoise, "archipelago");
			coastNoise = new SmoothMin(coastNoise, archNoise, 0.2);
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (archNoise.GetValue(allCell) < MaxForSand)
			{
				elevation[allCell] = 0f;
			}
		}
	}
}
