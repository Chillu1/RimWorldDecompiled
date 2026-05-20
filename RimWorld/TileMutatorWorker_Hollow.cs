using LudeonTK;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Hollow : TileMutatorWorker
{
	private ModuleBase hollowNoise;

	[TweakValue("Hollow", 0f, 1f)]
	private static float HollowRadius = 0.25f;

	[TweakValue("Hollow", 0f, 0.1f)]
	private static float HollowNoiseFreq = 0.015f;

	[TweakValue("Hollow", 0f, 50f)]
	private static float HollowNoiseStrength = 40f;

	[TweakValue("Hollow", -1f, 1f)]
	private static float HollowOffset = -0.175f;

	[TweakValue("Hollow", -1f, 1f)]
	private static float ConeOffset = 0.025f;

	[TweakValue("Hollow", -1f, 1f)]
	private static float InnerConeOffset = -0.375f;

	[TweakValue("Hollow", 0f, 1f)]
	private static float HollowThreshold = 0.5f;

	[TweakValue("Hollow", 0f, 5f)]
	private static float ConeGradient = 0.5f;

	[TweakValue("Hollow", 0f, 5f)]
	private static float InnerConeGradient = 3f;

	[TweakValue("Hollow", 0f, 1f)]
	private static float ConeSpan = 0.25f;

	[TweakValue("Hollow", 0f, 1f)]
	private static float Smoothing = 0.5f;

	[TweakValue("Hollow", 0f, 1f)]
	private static float CoastFalloff = 0.1f;

	public TileMutatorWorker_Hollow(TileMutatorDef def)
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
			hollowNoise = MapNoiseUtility.CreateFalloffRadius((float)map.Size.x * HollowRadius, Vector2.zero, 1f, invert: false);
			hollowNoise = new Translate(0.0, 0.0, (float)(-map.Size.x) * HollowOffset, hollowNoise);
			hollowNoise = new Rotate(0.0, num2 + 90f, 0.0, hollowNoise);
			hollowNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, hollowNoise);
			ModuleBase input = new DistFromCone(ConeGradient, (float)map.Size.x * ConeSpan);
			input = new Translate(0.0, 0.0, (float)(-map.Size.x) * ConeOffset, input);
			input = new Rotate(0.0, num2 + 90f, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			hollowNoise = new SmoothMin(hollowNoise, input, Smoothing);
			input = new DistFromCone(InnerConeGradient, (float)map.Size.x * ConeSpan);
			input = new Translate(0.0, 0.0, (float)(-map.Size.x) * InnerConeOffset, input);
			input = new Rotate(0.0, num2 + 90f, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			hollowNoise = new SmoothMin(hollowNoise, input, Smoothing);
			if (num.HasValue)
			{
				ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num.Value, CoastFalloff, map);
				hollowNoise = new SmoothMin(hollowNoise, rhs, 0.1);
			}
			NoiseDebugUI.StoreNoiseRender(hollowNoise, "Hollow base shape");
			hollowNoise = MapNoiseUtility.AddDisplacementNoise(hollowNoise, HollowNoiseFreq, HollowNoiseStrength);
			NoiseDebugUI.StoreNoiseRender(hollowNoise, "Hollow noise", map.Size.ToIntVec2);
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = hollowNoise.GetValue(allCell);
			if (value > HollowThreshold)
			{
				elevation[allCell] = value;
			}
		}
	}
}
