using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_LakeWithIsland : TileMutatorWorker_Lake
{
	private const float LakeIslandNoiseFrequency = 0.015f;

	private const float LakeIslandNoiseStrength = 15f;

	private const float LakeIslandMacroNoiseFrequency = 0.006f;

	private const float LakeIslandMacroNoiseStrength = 40f;

	private const float IslandRadius = 0.4f;

	protected override float LakeRadius => 0.8f;

	protected override bool GenerateDeepWater => false;

	public TileMutatorWorker_LakeWithIsland(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		base.Init(map);
		ModuleBase input = new DistFromPoint((float)map.Size.x * 0.4f);
		input = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, input);
		input = new Translate(-map.waterInfo.lakeCenter.x, 0.0, -map.waterInfo.lakeCenter.z, input);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.006f, 40f, 2, map.Tile.tileId);
		input = MapNoiseUtility.AddDisplacementNoise(input, 0.015f, 15f, 4, map.Tile.tileId);
		lakeNoise = new Min(lakeNoise, input);
	}
}
