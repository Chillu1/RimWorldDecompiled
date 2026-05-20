using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class GenStep_ElevationFertility : GenStep
{
	public static readonly FloatRange ElevationFreqRange = new FloatRange(0.015f, 0.0225f);

	public const int ElevationOctaves = 3;

	public static readonly FloatRange DetailFreqRange = new FloatRange(0.03f, 0.06f);

	public static readonly FloatRange DetailStrengthRange = new FloatRange(5f, 10f);

	public static readonly IntRange DetailOctavesRange = new IntRange(4, 5);

	private static readonly FloatRange WarpFreqRange = new FloatRange(0.01f, 0.02f);

	private static readonly FloatRange WarpStrengthRange = new FloatRange(0f, 15f);

	private static readonly IntRange WarpOctavesRange = new IntRange(3, 4);

	private static readonly FloatRange PreTurbulenceStretchRange = new FloatRange(1f, 1.15f);

	private static readonly FloatRange PostTurbulenceStretchRange = new FloatRange(1f, 1.15f);

	private const float FertilityFreq = 0.021f;

	public override int SeedPart => 826504671;

	public override void Generate(Map map, GenStepParams parms)
	{
		NoiseRenderer.renderSize = new IntVec2(map.Size.x, map.Size.z);
		bool flag = !map.TileInfo.Mutators.Any((TileMutatorDef m) => m.preventNaturalElevation);
		if (map.generatorDef.isUnderground)
		{
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			foreach (IntVec3 allCell in map.AllCells)
			{
				elevation[allCell] = 1f;
			}
		}
		else if (flag)
		{
			ModuleBase moduleBase = new Perlin(ElevationFreqRange.RandomInRange, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.High);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape");
			moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, DetailFreqRange.RandomInRange, DetailStrengthRange.RandomInRange, DetailOctavesRange.RandomInRange);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape + detail");
			moduleBase = new Scale(PreTurbulenceStretchRange.RandomInRange, 1.0, 1.0, moduleBase);
			moduleBase = new Rotate(0.0, Rand.Range(0f, 180f), 0.0, moduleBase);
			moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, WarpFreqRange.RandomInRange, WarpStrengthRange.RandomInRange, WarpOctavesRange.RandomInRange);
			moduleBase = new Scale(PostTurbulenceStretchRange.RandomInRange, 1.0, 1.0, moduleBase);
			moduleBase = new Rotate(0.0, Rand.Range(0f, 180f), 0.0, moduleBase);
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape + detail + warp");
			moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
			float num = 1f;
			switch (map.TileInfo.HillinessForElevationGen)
			{
			case Hilliness.Flat:
				num = MapGenTuning.ElevationFactorFlat;
				break;
			case Hilliness.SmallHills:
				num = MapGenTuning.ElevationFactorSmallHills;
				break;
			case Hilliness.LargeHills:
				num = MapGenTuning.ElevationFactorLargeHills;
				break;
			case Hilliness.Mountainous:
				num = MapGenTuning.ElevationFactorMountains;
				break;
			case Hilliness.Impassable:
				num = MapGenTuning.ElevationFactorImpassableMountains;
				break;
			}
			moduleBase = new Multiply(moduleBase, new Const(num));
			NoiseDebugUI.StoreNoiseRender(moduleBase, "elev world-factored");
			float b = (map.TileInfo.WaterCovered ? 0f : float.MaxValue);
			MapGenFloatGrid elevation2 = MapGenerator.Elevation;
			foreach (IntVec3 allCell2 in map.AllCells)
			{
				elevation2[allCell2] = Mathf.Min(moduleBase.GetValue(allCell2), b);
			}
		}
		ModuleBase input = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, int.MaxValue), QualityMode.High);
		input = new ScaleBias(0.5, 0.5, input);
		NoiseDebugUI.StoreNoiseRender(input, "noiseFert base");
		MapGenFloatGrid fertility = MapGenerator.Fertility;
		foreach (IntVec3 allCell3 in map.AllCells)
		{
			fertility[allCell3] = input.GetValue(allCell3);
		}
	}
}
