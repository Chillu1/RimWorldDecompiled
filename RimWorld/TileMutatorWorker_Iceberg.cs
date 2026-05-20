using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Iceberg : TileMutatorWorker_Coast
{
	private ModuleBase icebergNoise;

	private const float Radius = 0.25f;

	private const float BergNoiseFrequency = 0.015f;

	private const float BergNoiseStrength = 20f;

	private const float Exponent = 0.4f;

	private const float BergDeepWaterThreshold = 0f;

	private const float BergThreshold = 0.35f;

	private const float BergWallThreshold = 0.55f;

	private const float WaterSmoothMinStrength = 0.25f;

	private const float WaterRadius = 0.95f;

	private const float WaterSquash = 0.75f;

	private static readonly FloatRange BergSquashRange = new FloatRange(0.65f, 0.9f);

	protected override FloatRange CoastOffset => new FloatRange(0.6f);

	public TileMutatorWorker_Iceberg(TileMutatorDef def)
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
			input = new Scale(1.0, 1.0, 0.75, input);
			input = new Rotate(0.0, GetCoastAngle(map.Tile), 0.0, input);
			input = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input);
			input = MapNoiseUtility.AddDisplacementNoise(input, 0.015f, 25f);
			coastNoise = new SmoothMin(coastNoise, input, 0.25);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "coast with space for iceberg");
			icebergNoise = new DistFromPoint((float)map.Size.x * 0.25f);
			icebergNoise = new ScaleBias(-1.0, 1.0, icebergNoise);
			icebergNoise = new Scale(BergSquashRange.RandomInRange, 1.0, 1.0, icebergNoise);
			icebergNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, icebergNoise);
			icebergNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, icebergNoise);
			icebergNoise = MapNoiseUtility.AddDisplacementNoise(icebergNoise, 0.015f, 20f);
			icebergNoise = new Clamp(0.0, 1.0, icebergNoise);
			icebergNoise = new Power(icebergNoise, new Const(0.4000000059604645));
			NoiseDebugUI.StoreNoiseRender(icebergNoise, "iceberg");
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		base.GeneratePostTerrain(map);
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = icebergNoise.GetValue(allCell);
			if (value > 0.35f)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Ice);
			}
			else if (value > 0f)
			{
				map.terrainGrid.SetTerrain(allCell, MapGenUtility.ShallowOceanWaterTerrainAt(allCell, map));
			}
			if (value > 0.55f)
			{
				GenSpawn.Spawn(ThingDefOf.SolidIce, allCell, map);
			}
		}
	}
}
