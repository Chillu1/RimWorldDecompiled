using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_TerraformingScar : TileMutatorWorker
{
	private const float WarpFreq = 0.005f;

	private const float WarpStrength = 75f;

	private const int WarpOctaves = 4;

	private const float PreTurbulenceStretch = 2f;

	private const float PostTurbulenceStretch = 1f;

	public TileMutatorWorker_TerraformingScar(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		ModuleBase moduleBase = new Perlin(GenStep_ElevationFertility.ElevationFreqRange.RandomInRange, 2.0, 0.5, 3, Rand.Range(0, int.MaxValue), QualityMode.High);
		NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape");
		moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, GenStep_ElevationFertility.DetailFreqRange.RandomInRange, GenStep_ElevationFertility.DetailStrengthRange.RandomInRange, GenStep_ElevationFertility.DetailOctavesRange.RandomInRange);
		NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape + detail");
		moduleBase = new Scale(2.0, 1.0, 1.0, moduleBase);
		moduleBase = new Rotate(0.0, Rand.Range(0f, 180f), 0.0, moduleBase);
		moduleBase = MapNoiseUtility.AddDisplacementNoise(moduleBase, 0.005f, 75f);
		moduleBase = new Scale(1.0, 1.0, 1.0, moduleBase);
		moduleBase = new Rotate(0.0, Rand.Range(0f, 180f), 0.0, moduleBase);
		NoiseDebugUI.StoreNoiseRender(moduleBase, "elev shape + detail + warp");
		moduleBase = new ScaleBias(0.5, 0.5, moduleBase);
		float elevationFactorMountains = MapGenTuning.ElevationFactorMountains;
		moduleBase = new Multiply(moduleBase, new Const(elevationFactorMountains));
		NoiseDebugUI.StoreNoiseRender(moduleBase, "elev world-factored");
		float b = (map.TileInfo.WaterCovered ? 0f : float.MaxValue);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			elevation[allCell] = Mathf.Min(moduleBase.GetValue(allCell), b);
		}
	}
}
