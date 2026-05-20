using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Chasm : TileMutatorWorker
{
	private const float OpeningNoiseFreq = 0.015f;

	private const float OpeningNoiseStrength = 40f;

	private const float OpeningRadius = 0.4f;

	private const float ChasmThreshold = 0.5f;

	private static readonly FloatRange OpeningSquashRange = new FloatRange(1f, 1.3f);

	private static readonly IntRange EntranceCountRange = new IntRange(3, 6);

	private const float EntranceSpan = 0.06f;

	private const float EntranceNoiseFreq = 0.007f;

	private const float EntranceNoiseStrength = 50f;

	private const int EntranceNoiseOctaves = 6;

	private const float MinAngleBetweenEntrances = 40f;

	private const float CoastFalloff = 0.1f;

	private ModuleBase chasmNoise;

	public TileMutatorWorker_Chasm(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		chasmNoise = new DistFromPoint((float)map.Size.x * 0.4f);
		chasmNoise = new Scale(OpeningSquashRange.RandomInRange, 1.0, 1.0, chasmNoise);
		chasmNoise = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, chasmNoise);
		chasmNoise = new Translate(-map.Center.x, 0.0, -map.Center.z, chasmNoise);
		float? num = Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake);
		if (num.HasValue)
		{
			ModuleBase rhs = MapNoiseUtility.FalloffAtAngle(num.Value, 0.1f, map);
			chasmNoise = new SmoothMin(chasmNoise, rhs, 0.2);
		}
		NoiseDebugUI.StoreNoiseRender(chasmNoise, "opening shape");
		chasmNoise = MapNoiseUtility.AddDisplacementNoise(chasmNoise, 0.015f, 40f);
		NoiseDebugUI.StoreNoiseRender(chasmNoise, "opening");
		List<float> list = new List<float>();
		ModuleBase moduleBase = new Const(1.0);
		int randomInRange = EntranceCountRange.RandomInRange;
		for (int i = 0; i < randomInRange; i++)
		{
			float entranceAngle = Rand.Range(0f, 360f);
			int num2 = 100;
			while (list.Any((float ua) => Mathf.Abs(Mathf.DeltaAngle(ua, entranceAngle)) < 40f) && num2-- > 0)
			{
				entranceAngle = Rand.Range(0f, 360f);
			}
			list.Add(entranceAngle);
			ModuleBase input = new DistFromAxis((float)map.Size.x * 0.06f);
			input = new ScaleBias(-1.0, 1.0, input);
			input = new Multiply(input, new CutOff(invert: false, zAxis: true));
			input = new ScaleBias(-1.0, 1.0, input);
			input = new Rotate(0.0, entranceAngle, 0.0, input);
			input = new Translate(-map.Center.x, 0.0, -map.Center.z, input);
			moduleBase = new SmoothMin(moduleBase, input, 0.2);
		}
		moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, 0.007f, 50f, 6);
		NoiseDebugUI.StoreNoiseRender(moduleBase, "entrances");
		chasmNoise = new SmoothMin(chasmNoise, moduleBase, 0.2);
		NoiseDebugUI.StoreNoiseRender(chasmNoise, "chasm");
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			float value = chasmNoise.GetValue(allCell);
			if (value > 0.5f)
			{
				elevation[allCell] = value;
			}
		}
	}
}
