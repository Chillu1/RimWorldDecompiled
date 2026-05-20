using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Cliffs : TileMutatorWorker
{
	private ModuleBase cliffNoise;

	private const float CliffSpan = 0.5f;

	private const float CliffOffset = 0.5f;

	private const float CliffSquash = 0.5f;

	private const float CliffNoiseFreq = 0.015f;

	private const float CliffNoiseStrength = 40f;

	private const float CliffThreshold = 0.75f;

	private const float CoastFalloff = 0.1f;

	public TileMutatorWorker_Cliffs(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float? num = Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean);
			float num2 = num ?? ((float)Rand.Range(0, 360));
			cliffNoise = new DistFromPoint((float)map.Size.x * 0.5f);
			cliffNoise = new Scale(1.0, 1.0, 0.5, cliffNoise);
			cliffNoise = new Translate(0.0, 0.0, (float)(-map.Size.x) * 0.5f, cliffNoise);
			cliffNoise = new Rotate(0.0, num2 + 90f, 0.0, cliffNoise);
			cliffNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, cliffNoise);
			if (num.HasValue)
			{
				ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num.Value, 0.1f, map);
				cliffNoise = new SmoothMin(cliffNoise, rhs, 0.1);
			}
			cliffNoise = MapNoiseUtility.AddDisplacementNoise(cliffNoise, 0.015f, 40f);
			NoiseDebugUI.StoreNoiseRender(cliffNoise, "cliff");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = cliffNoise.GetValue(allCell);
			if (value > 0.75f)
			{
				elevation[allCell] = value;
			}
		}
	}
}
