using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_IceDunes : TileMutatorWorker
{
	private ModuleBase iceDunesNoise;

	private const float VoronoiFrequency = 0.007f;

	private const float VoronoiRandomness = 0.5f;

	private const float NoiseFrequency = 0.03f;

	private const float NoiseStrength = 10f;

	private const float ScaleFactor = 3f;

	private const float Threshold = 0.2f;

	private const float DisplaceFrequency = 0.02f;

	private const float DisplaceMagnitude = 6.5f;

	public TileMutatorWorker_IceDunes(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			iceDunesNoise = new Voronoi2D(0.007000000216066837, Rand.Int, 0.5f, staggered: true);
			iceDunesNoise = MapNoiseUtility.AddDisplacementNoise(iceDunesNoise, 0.03f, 10f, 3);
			iceDunesNoise = new Scale(3.0, 1.0, 1.0, iceDunesNoise);
			ModuleBase lhs = new Perlin(0.019999999552965164, 2.0, 0.5, 2, Rand.Int, QualityMode.Medium);
			lhs = new Multiply(lhs, new Const(6.5));
			iceDunesNoise = new Displace(iceDunesNoise, lhs, new Const(0.0), new Const(0.0));
			float num = Rand.Range(0f, 180f);
			iceDunesNoise = new Rotate(0.0, num, 0.0, iceDunesNoise);
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!(iceDunesNoise.GetValue(allCell) > 0.2f) && !allCell.GetTerrain(map).IsWater)
			{
				GenSpawn.Spawn(ThingDefOf.SolidIce, allCell, map);
			}
		}
	}
}
