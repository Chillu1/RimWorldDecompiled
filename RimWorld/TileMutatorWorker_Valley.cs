using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Valley : TileMutatorWorker
{
	private ModuleBase valleyNoise;

	private const float ValleySpan = 0.3f;

	private const float ValleyNoiseFreq = 0.015f;

	private const float ValleyNoiseStrength = 30f;

	private const float ValleyFalloffExp = 0.5f;

	private const float CoastFalloff = 0.2f;

	public TileMutatorWorker_Valley(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float? num = Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake);
			float num2 = num ?? ((float)Rand.Range(0, 360));
			valleyNoise = new DistFromAxis((float)map.Size.x * 0.3f);
			valleyNoise = new Power(valleyNoise, new Const(0.5));
			valleyNoise = new ScaleBias(2.0, -1.0, valleyNoise);
			valleyNoise = new Rotate(0.0, num2 + 90f, 0.0, valleyNoise);
			valleyNoise = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, valleyNoise);
			if (num.HasValue)
			{
				ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num.Value, 0.2f, map);
				valleyNoise = new SmoothMin(valleyNoise, rhs, 0.1);
			}
			valleyNoise = MapNoiseUtility.AddDisplacementNoise(valleyNoise, 0.015f, 30f);
			NoiseDebugUI.StoreNoiseRender(valleyNoise, "valley");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		float b = (map.TileInfo.WaterCovered ? 0f : float.MaxValue);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			elevation[allCell] = Mathf.Min(elevation[allCell] + valleyNoise.GetValue(allCell), b);
		}
	}
}
