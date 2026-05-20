using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_Wetland : TileMutatorWorker
{
	private const float RidgedFreq = 0.03f;

	private const float RidgedLac = 2f;

	private const int RidgedOctaves = 2;

	private const float MudThreshold = 0.015f;

	private const float WaterThreshold = 0.35f;

	private ModuleBase wetlandNoise;

	public TileMutatorWorker_Wetland(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			wetlandNoise = new RidgedMultifractal(0.029999999329447746, 2.0, 2, Rand.Int, QualityMode.High);
			wetlandNoise = new Clamp(0.0, 1.0, wetlandNoise);
			NoiseDebugUI.StoreNoiseRender(wetlandNoise, "wetland");
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (allCell.GetEdifice(map) != null)
			{
				continue;
			}
			TerrainDef terrain = allCell.GetTerrain(map);
			if (terrain.IsWater)
			{
				continue;
			}
			float value = wetlandNoise.GetValue(allCell);
			if (value > 0.35f)
			{
				if (terrain.IsRock)
				{
					map.terrainGrid.SetTerrain(allCell, MapGenUtility.MudTerrainAt(allCell, map));
				}
				else
				{
					map.terrainGrid.SetTerrain(allCell, MapGenUtility.ShallowFreshWaterTerrainAt(allCell, map));
				}
			}
			else if (value > 0.015f && MapGenUtility.ShouldGenerateBeachSand(allCell, map))
			{
				map.terrainGrid.SetTerrain(allCell, MapGenUtility.MudTerrainAt(allCell, map));
			}
		}
	}
}
