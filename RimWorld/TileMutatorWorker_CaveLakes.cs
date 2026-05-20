using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_CaveLakes : TileMutatorWorker_Caves
{
	private static readonly FloatRange LakeRadius = new FloatRange(10f, 15f);

	private static readonly FloatRange LakeSquash = new FloatRange(1f, 1.25f);

	private const float LakeNoiseFrequency = 0.03f;

	private const float LakeNoiseStrength = 5f;

	private const float LakeChancePerCell = 0.005f;

	private const float DeepWaterThreshold = 0.8f;

	private const float WaterThreshold = 0.5f;

	public TileMutatorWorker_CaveLakes(TileMutatorDef def)
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
			CellRect cellRect = allCell.RectAbout(30, 30);
			cellRect.ClipInsideMap(map);
			foreach (IntVec3 item in cellRect)
			{
				float num = (float)input.GetValue(item.x, 0.0, item.z);
				if (!(num < 0.5f))
				{
					item.GetEdifice(map)?.Destroy();
					if (num > 0.8f)
					{
						map.terrainGrid.SetTerrain(item, MapGenUtility.DeepFreshWaterTerrainAt(item, map));
					}
					else if (num > 0.5f)
					{
						map.terrainGrid.SetTerrain(item, MapGenUtility.ShallowFreshWaterTerrainAt(item, map));
					}
				}
			}
		}
	}
}
