using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Dunes : TileMutatorWorker
{
	private ModuleBase dunesNoise;

	private const float VoronoiFrequency = 0.0025f;

	private const float VoronoiRandomnessX = 0.15f;

	private const float VoronoiRandomnessZ = 0.5f;

	private const float NoiseFrequency = 0.015f;

	private const float NoiseStrength = 20f;

	private const int NoiseOctaves = 2;

	private const float StretchFactor = 15f;

	private const float ScaleFactor = 1f;

	private const float Threshold = 0.2f;

	private const float DisplaceFrequency = 0.02f;

	private const float DisplaceMagnitude = 7.5f;

	public TileMutatorWorker_Dunes(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			dunesNoise = new Voronoi2D(0.0024999999441206455, Rand.Int, 0.15f, 0.5f, staggered: true);
			dunesNoise = MapNoiseUtility.AddDisplacementNoise(dunesNoise, 0.015f, 20f, 2);
			dunesNoise = new Scale(1.0, 1.0, 15.0, dunesNoise);
			dunesNoise = new Scale(1.0, 1.0, 1.0, dunesNoise);
			ModuleBase lhs = new Perlin(0.019999999552965164, 2.0, 0.5, 2, Rand.Int, QualityMode.Medium);
			lhs = new Multiply(lhs, new Const(7.5));
			dunesNoise = new Displace(dunesNoise, new Const(0.0), new Const(0.0), lhs);
			dunesNoise = new Rotate(0.0, Rand.Range(0f, 180f), 0.0, dunesNoise);
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!(dunesNoise.GetValue(allCell) > 0.2f) && allCell.GetTerrain(map) == TerrainDefOf.Sand)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.SoftSand);
			}
		}
	}
}
