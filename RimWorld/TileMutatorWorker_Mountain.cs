using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Mountain : TileMutatorWorker
{
	private const float MountainNoiseFrequency = 0.003f;

	private const float MountainNoiseStrength = 20f;

	private const float EdgeMountainSpan = 0.15f;

	private const float EdgeMountainOffset = 0.2f;

	public TileMutatorWorker_Mountain(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		float num = (Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Ocean) ?? Find.World.CoastAngleAt(map.Tile, BiomeDefOf.Lake)) ?? ((float)Rand.Range(0, 360));
		ModuleBase input = new DistFromAxis_Directional((float)map.Size.x * 0.15f);
		input = new Translate((float)(-map.Size.x) * 0.2f, 0.0, 0.0, input);
		input = new ScaleBias(0.5, 0.5, input);
		input = new Clamp(0.0, 1.0, input);
		input = new Rotate(0.0, num, 0.0, input);
		input = new Translate((float)(-map.Size.x) / 2f, 0.0, (float)(-map.Size.z) / 2f, input);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.003f, 20f, 2);
		NoiseDebugUI.StoreNoiseRender(input, "mountain");
		float b = (map.TileInfo.WaterCovered ? 0f : float.MaxValue);
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			elevation[allCell] = Mathf.Min(elevation[allCell] + input.GetValue(allCell), b);
		}
	}
}
