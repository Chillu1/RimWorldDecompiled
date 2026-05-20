using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Cove : TileMutatorWorker_Coast
{
	private ModuleBase coveNoise;

	private const float CoveRadius = 0.5f;

	private const float CoveOffset = -0.2f;

	private const float CoveMacroNoiseFrequency = 0.003f;

	private const float CoveMacroNoiseStrength = 30f;

	private const float EntranceGradient = 5f;

	private const float EntranceSpan = 0.1f;

	private const float EntranceScale = 0.25f;

	private const float EntranceBias = 0f;

	private const float EntranceXOffset = 0.1f;

	private const float ElevationThreshold = 0.4f;

	public TileMutatorWorker_Cove(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			base.Init(map);
			float coastAngle = GetCoastAngle(map.Tile);
			coveNoise = MapNoiseUtility.CreateFalloffRadius((float)map.Size.x * 0.5f, Vector2.zero, 1f, invert: false);
			coveNoise = new Translate(0.0, 0.0, (float)(-map.Size.x) * -0.2f, coveNoise);
			coveNoise = new Rotate(0.0, coastAngle + 90f, 0.0, coveNoise);
			coveNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, coveNoise);
			float num = Rand.Range(-0.1f, 0.1f);
			ModuleBase input = new DistFromCone(5f, (float)map.Size.x * 0.1f);
			input = new Translate((float)(-map.Size.x) * num, 0.0, 0.0, input);
			input = new Rotate(0.0, coastAngle + 90f, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			input = new ScaleBias(0.25, 0.0, input);
			coveNoise = new SmoothMin(coveNoise, input, 0.2);
			NoiseDebugUI.StoreNoiseRender(coveNoise, "cove shape");
			coveNoise = MapNoiseUtility.AddDisplacementNoise(coveNoise, 0.003f, 30f, 2);
			coveNoise = MapNoiseUtility.AddDisplacementNoise(coveNoise, 0.015f, 25f);
			NoiseDebugUI.StoreNoiseRender(coveNoise, "cove noise");
			coveNoise = new Max(coveNoise, new Const(MaxForDeepWater));
			coastNoise = new SmoothMin(coveNoise, coastNoise, 0.5);
			NoiseDebugUI.StoreNoiseRender(coastNoise, "cove & coast");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		base.GeneratePostElevationFertility(map);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (coveNoise.GetValue(allCell) < 0.4f)
			{
				elevation[allCell] = 0f;
			}
		}
	}
}
