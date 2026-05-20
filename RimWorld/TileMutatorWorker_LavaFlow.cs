using Verse;
using Verse.Noise;

namespace RimWorld;

public class TileMutatorWorker_LavaFlow : TileMutatorWorker
{
	private const int IncidentMTBDays = 6;

	private const float RidgedFreq = 0.03f;

	private const float RidgedLac = 2f;

	private const int RidgedOctaves = 3;

	private const float RockThreshold = 0.85f;

	private ModuleBase rockNoise;

	public TileMutatorWorker_LavaFlow(TileMutatorDef def)
		: base(def)
	{
	}

	public override void Init(Map map)
	{
		if (ModsConfig.OdysseyActive)
		{
			rockNoise = new RidgedMultifractal(0.029999999329447746, 2.0, 3, Rand.Int, QualityMode.High);
			rockNoise = new Clamp(0.0, 1.0, rockNoise);
			rockNoise = new Invert(rockNoise);
			rockNoise = new ScaleBias(1.0, 1.0, rockNoise);
			NoiseDebugUI.StoreNoiseRender(rockNoise, "lava flow");
		}
	}

	public override void GeneratePostElevationFertility(Map map)
	{
		MapGenFloatGrid elevation = MapGenerator.Elevation;
		foreach (IntVec3 allCell in map.AllCells)
		{
			if (!(rockNoise.GetValue(allCell) > 0.85f))
			{
				elevation[allCell] = 0f;
			}
		}
	}

	public override void GeneratePostTerrain(Map map)
	{
		TerrainPatchMaker terrainPatchMaker = map.Biome.terrainPatchMakers.FirstOrDefault((TerrainPatchMaker x) => x.isPond);
		foreach (IntVec3 allCell in map.AllCells)
		{
			TerrainDef terrain = allCell.GetTerrain(map);
			if (terrain.IsWater)
			{
				continue;
			}
			if (terrain == TerrainDefOf.VolcanicRock)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.Soil);
			}
			TerrainDef terrainDef = terrainPatchMaker?.TerrainAt(allCell, map, allCell.GetFertility(map));
			if (terrainDef != null)
			{
				map.terrainGrid.SetTerrain(allCell, terrainDef);
				if (terrainDef == TerrainDefOf.VolcanicRock)
				{
					map.terrainGrid.SetTerrain(allCell, TerrainDefOf.CooledLava);
				}
			}
			else if (!(rockNoise.GetValue(allCell) > 0.85f) && terrain != TerrainDefOf.LavaDeep)
			{
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.VolcanicRock);
				map.terrainGrid.SetTerrain(allCell, TerrainDefOf.CooledLava);
			}
		}
		terrainPatchMaker?.Cleanup();
	}

	public override void Tick(Map map)
	{
		IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentDefOf.LavaFlow.category, map);
		if (!map.GameConditionManager.ConditionIsActive(GameConditionDefOf.LavaFlow) && Rand.MTBEventOccurs(6f, 60000f, 1f) && IncidentDefOf.LavaFlow.Worker.CanFireNow(parms))
		{
			Find.Storyteller.incidentQueue.Add(IncidentDefOf.LavaFlow, Find.TickManager.TicksGame, parms);
		}
	}
}
