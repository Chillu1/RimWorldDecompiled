using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_LavaCaves : TileMutatorWorker_Caves
{
	private static readonly FloatRange LakeRadius = new FloatRange(4f, 6f);

	private static readonly FloatRange LakeSquash = new FloatRange(1f, 1.25f);

	private const float LakeNoiseFrequency = 0.03f;

	private const float LakeNoiseStrength = 5f;

	private const float LavaPocketChancePerCell = 0.005f;

	private const float LavaThreshold = 0.5f;

	private const float CooledLavaThreshold = 0.2f;

	public TileMutatorWorker_LavaCaves(TileMutatorDef def)
		: base(def)
	{
	}

	public override void GeneratePostTerrain(Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		MapGenFloatGrid caves = MapGenerator.Caves;
		base.GeneratePostTerrain(map);
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (caves[allCell] <= 0f || allCell.GetTerrain(map).IsWater || !Rand.Chance(0.005f))
			{
				continue;
			}
			ModuleBase input = new DistFromPoint(LakeRadius.RandomInRange);
			input = new ScaleBias(-1.0, 1.0, input);
			input = new Scale(LakeSquash.RandomInRange, 1.0, 1.0, input);
			input = new Rotate(0.0, Rand.Range(0f, 360f), 0.0, input);
			input = new Translate(-allCell.x, 0.0, -allCell.z, input);
			input = MapNoiseUtility.AddDisplacementNoise(input, 0.03f, 5f, 4, map.Tile.tileId);
			CellRect cellRect = allCell.RectAbout(20, 20);
			cellRect.ClipInsideMap(map);
			foreach (IntVec3 item in cellRect)
			{
				float num = (float)input.GetValue(item.x, 0.0, item.z);
				if (!(num < 0.2f))
				{
					item.GetEdifice(map)?.Destroy();
					if (num > 0.5f)
					{
						map.terrainGrid.SetTerrain(item, TerrainDefOf.LavaDeep);
					}
					else if (num > 0.2f)
					{
						map.terrainGrid.SetTerrain(item, TerrainDefOf.CooledLava);
					}
				}
			}
		}
	}
}
