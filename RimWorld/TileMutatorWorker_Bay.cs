using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Bay : TileMutatorWorker_Coast
{
	private const float BaySpan = 0.6f;

	private const float BayOffset = 0.1f;

	private const float BaySquash = 0.75f;

	private const float BayMacroNoiseFrequency = 0.003f;

	private const float BayMacroNoiseStrength = 70f;

	private const float ElevationThreshold = 0.4f;

	private ModuleBase bayNoise;

	protected override FloatRange CoastOffset => new FloatRange(0.2f, 0.3f);

	public TileMutatorWorker_Bay(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float coastAngle = GetCoastAngle(map.Tile);
			bayNoise = new DistFromPoint((float)map.Size.x * 0.6f);
			bayNoise = new Clamp(MaxForDeepWater, 1.0, bayNoise);
			bayNoise = new Scale(1.0, 1.0, 0.75, bayNoise);
			bayNoise = new Translate(0.0, 0.0, (float)(-map.Size.x) * 0.1f, bayNoise);
			bayNoise = new Rotate(0.0, coastAngle + 90f, 0.0, bayNoise);
			bayNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, bayNoise);
			NoiseDebugUI.StoreNoiseRender(bayNoise, "bay shape");
			bayNoise = MapNoiseUtility.AddDisplacementNoise(bayNoise, 0.003f, 70f, 2);
			bayNoise = MapNoiseUtility.AddDisplacementNoise(bayNoise, 0.015f, 25f);
			NoiseDebugUI.StoreNoiseRender(bayNoise, "bay noise");
			coastNoise = new SmoothMin(bayNoise, coastNoise, 0.5);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "bay & coast");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (bayNoise.GetValue(allCell) < 0.4f)
			{
				elevation[allCell] = 0f;
			}
		}
	}
}
