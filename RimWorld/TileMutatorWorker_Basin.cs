using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Basin : TileMutatorWorker_Lake
{
	private ModuleBase basinNoise;

	private const float BasinRadius = 0.35f;

	private const float EntranceGradient = 5f;

	private const float EntranceSpan = 0.05f;

	private const float AdditionalEntranceSpan = 0.02f;

	private const float BasinNoiseFreq = 0.015f;

	private const float BasinNoiseStrength = 35f;

	private const float BasinThreshold = 0.5f;

	private const float MinAngleBetweenEntrances = 70f;

	private const float CoastFalloff = 0.1f;

	private static readonly IntRange AdditionalEntranceCountRange = new IntRange(1, 2);

	protected override float LakeRadius => 0.3f;

	protected override IntVec3 GetLakeCenter(Map map)
	{
		return map.Center;
	}

	public TileMutatorWorker_Basin(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		base.Init(map);
		Rot4 rot = Rot4.Random;
		if (map.TileInfo.IsCoastal)
		{
			rot = Find.World.CoastDirectionAt(map.Tile).Opposite;
		}
		float angle = rot.AsAngle + (float)Rand.Range(-30, 30);
		basinNoise = MapNoiseUtility.CreateFalloffRadius((float)map.Size.x * 0.35f, map.Center.ToVector2(), 1f, invert: false);
		List<float> list = new List<float>();
		list.Add(angle);
		ModuleBase input = new DistFromCone(5f, (float)map.Size.x * 0.05f);
		input = new Rotate(0.0, 360f - angle, 0.0, input);
		input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
		basinNoise = new SmoothMin(basinNoise, input, 0.2);
		int randomInRange = AdditionalEntranceCountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			int num = 100;
			while (list.Any((float ua) => Mathf.Abs(Mathf.DeltaAngle(ua, angle)) < 70f) && num-- > 0)
			{
				angle = Rand.Range(0f, 360f);
			}
			list.Add(angle);
			input = new DistFromCone(5f, (float)map.Size.x * 0.02f);
			input = new Rotate(0.0, angle, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			basinNoise = new SmoothMin(basinNoise, input, 0.2);
		}
		NoiseDebugUI.StoreNoiseRender(basinNoise, "Basin base shape");
		float? num2 = Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake);
		if (num2.HasValue)
		{
			ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num2.Value, 0.1f, map);
			basinNoise = new SmoothMin(basinNoise, rhs, 0.1);
		}
		basinNoise = MapNoiseUtility.AddDisplacementNoise(basinNoise, 0.015f, 35f);
		NoiseDebugUI.StoreNoiseRender(basinNoise, "Basin");
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = basinNoise.GetValue(allCell);
			if (value > 0.5f)
			{
				elevation[allCell] = value;
			}
		}
		base.GeneratePostElevationFertility(map);
	}
}
