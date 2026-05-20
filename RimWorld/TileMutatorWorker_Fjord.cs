using RimWorld.Planet;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Fjord : TileMutatorWorker_Coast
{
	private const float FjordSpan = 0.25f;

	private const float ConeGradient = 2f;

	private const float ConeSpan = 0.35f;

	private const float ConeOffset = -0.5f;

	private const float ElevationOffset = -1f;

	private ModuleBase fjordNoise;

	public TileMutatorWorker_Fjord(TileMutatorDef def)
		: base(def)
	{
	}

	public override string GetLabel(PlanetTile tile)
	{
		return def.label;
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float coastAngle = GetCoastAngle(map.Tile);
			fjordNoise = new DistFromAxis((float)map.Size.x * 0.25f);
			fjordNoise = new Rotate(0.0, coastAngle + 90f, 0.0, fjordNoise);
			fjordNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, fjordNoise);
			fjordNoise = new Max(fjordNoise, new Const(MaxForDeepWater));
			ModuleBase input = new DistFromCone(2f, (float)map.Size.x * 0.35f);
			input = new Translate(0.0, 0.0, (float)(-map.Size.x) * -0.5f, input);
			input = new Rotate(0.0, coastAngle + 90f, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			fjordNoise = new SmoothMin(fjordNoise, input, 0.5);
			NoiseDebugUI.StoreNoiseRender(fjordNoise, "fjord shape");
			fjordNoise = MapNoiseUtility.AddDisplacementNoise(fjordNoise, 0.015f, 25f);
			NoiseDebugUI.StoreNoiseRender(fjordNoise, "fjord");
			coastNoise = new SmoothMin(fjordNoise, coastNoise, 0.5);
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = fjordNoise.GetValue(allCell);
			if (value < MaxForSand)
			{
				elevation[allCell] = 0f;
			}
			else
			{
				elevation[allCell] += value + -1f;
			}
		}
	}
}
