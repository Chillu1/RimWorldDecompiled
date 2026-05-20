using System;
using System.Collections.Generic;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld;

public class BiomeDef : Def
{
	public Type workerClass;

	public bool implemented = true;

	public bool generatesNaturally = true;

	public bool canBuildBase = true;

	public bool canAutoChoose = true;

	public bool allowRoads = true;

	public bool allowRivers = true;

	public bool allowFarmingCamps = true;

	public float animalDensity;

	public float plantDensity;

	public float diseaseMtbDays = 60f;

	public float settlementSelectionWeight = 1f;

	public float campSelectionWeight = 1f;

	public float pollutionOffset;

	public bool impassable;

	public bool hasVirtualPlants = true;

	public float forageability;

	public ThingDef foragedFood;

	public bool wildPlantsCareAboutLocalFertility = true;

	public bool wildPlantsAreCavePlants;

	public float wildPlantRegrowDays = 25f;

	public float movementDifficulty = 1f;

	public List<WeatherCommonalityRecord> baseWeatherCommonalities = new List<WeatherCommonalityRecord>();

	public List<TerrainThreshold> terrainsByFertility = new List<TerrainThreshold>();

	public List<SoundDef> soundsAmbient = new List<SoundDef>();

	public List<TerrainPatchMaker> terrainPatchMakers = new List<TerrainPatchMaker>();

	public List<BiomePlantRecord> wildPlants = new List<BiomePlantRecord>();

	private List<BiomeAnimalRecord> wildAnimals = new List<BiomeAnimalRecord>();

	private List<BiomeAnimalRecord> pollutionWildAnimals = new List<BiomeAnimalRecord>();

	private List<BiomeAnimalRecord> coastalWildAnimals = new List<BiomeAnimalRecord>();

	private List<BiomeDiseaseRecord> diseases = new List<BiomeDiseaseRecord>();

	private List<ThingDef> allowedPackAnimals = new List<ThingDef>();

	public bool hasBedrock = true;

	public bool isExtremeBiome;

	public bool isWaterBiome;

	public bool allowPollution = true;

	public bool wildAnimalsCanWanderInto = true;

	public bool noAmbientWind;

	public bool inVacuum;

	public bool disableSkyLighting;

	public bool disableShadows;

	public bool noGravel;

	public bool canExitMap = true;

	public bool onlyAllowWhitelistedArrivalModes;

	public bool isBackgroundBiome;

	public float wildAnimalScariaChance;

	public float geyserCountFactor = 1f;

	public float? constantOutdoorTemperature;

	public TerrainDef coastalBeachTerrain;

	public TerrainDef lakeBeachTerrain;

	public TerrainDef riverbankTerrain;

	public TerrainDef mudTerrain;

	public TerrainDef gravelTerrain;

	public TerrainDef waterShallowTerrain;

	public TerrainDef waterDeepTerrain;

	public TerrainDef oceanShallowTerrain;

	public TerrainDef oceanDeepTerrain;

	public TerrainDef waterMovingShallowTerrain;

	public TerrainDef waterMovingChestDeepTerrain;

	public IntRange riverbankSizeRange;

	public List<GameConditionDef> biomeMapConditions = new List<GameConditionDef>();

	public List<GenStepDef> extraGenSteps = new List<GenStepDef>();

	public List<GenStepDef> preventGenSteps = new List<GenStepDef>();

	public List<ThingDef> extraRockTypes;

	public List<ThingDef> forceRockTypes;

	public float maxFishPopulation;

	public BiomeFishTypes fishTypes;

	public List<PlanetLayerDef> layerWhitelist;

	public List<PlanetLayerDef> layerBlacklist;

	[MustTranslate]
	public string settleWarning;

	public Color? fogOfWarColor;

	public OrbitalDebrisDef orbitalDebris;

	[NoTranslate]
	public string texture;

	[Unsaved(false)]
	private Dictionary<PawnKindDef, float> cachedAnimalCommonalities;

	[Unsaved(false)]
	private Dictionary<PawnKindDef, float> cachedPollutionAnimalCommonalities;

	[Unsaved(false)]
	private Dictionary<PawnKindDef, float> cachedCoastalAnimalCommonalities;

	[Unsaved(false)]
	private Dictionary<ThingDef, float> cachedPlantCommonalities;

	[Unsaved(false)]
	private Dictionary<IncidentDef, float> cachedDiseaseCommonalities;

	[Unsaved(false)]
	private Material cachedMat;

	[Unsaved(false)]
	private List<ThingDef> cachedWildPlants;

	[Unsaved(false)]
	private float? cachedLowestWildPlantOrder;

	[Unsaved(false)]
	private int? cachedMaxWildPlantsClusterRadius;

	private BiomeWorker worker;

	public BiomeWorker Worker => worker ?? (worker = GenWorker<BiomeWorker>.Get(workerClass));

	public Material DrawMaterial
	{
		get
		{
			if (cachedMat == null)
			{
				if (texture.NullOrEmpty())
				{
					return null;
				}
				if (this == BiomeDefOf.Ocean || this == BiomeDefOf.Lake)
				{
					cachedMat = MaterialAllocator.Create(WorldMaterials.WorldOcean);
				}
				else if (!allowRoads && !allowRivers)
				{
					cachedMat = MaterialAllocator.Create(WorldMaterials.WorldIce);
				}
				else
				{
					cachedMat = MaterialAllocator.Create(WorldMaterials.WorldTerrain);
				}
				cachedMat.mainTexture = ContentFinder<Texture2D>.Get(texture);
			}
			return cachedMat;
		}
	}

	public List<ThingDef> AllWildPlants
	{
		get
		{
			if (cachedWildPlants == null)
			{
				cachedWildPlants = new List<ThingDef>();
				foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
				{
					if (item.category == ThingCategory.Plant && CommonalityOfPlant(item) > 0f)
					{
						cachedWildPlants.Add(item);
					}
				}
			}
			return cachedWildPlants;
		}
	}

	public int MaxWildAndCavePlantsClusterRadius
	{
		get
		{
			if (!cachedMaxWildPlantsClusterRadius.HasValue)
			{
				cachedMaxWildPlantsClusterRadius = 0;
				List<ThingDef> allWildPlants = AllWildPlants;
				for (int i = 0; i < allWildPlants.Count; i++)
				{
					if (allWildPlants[i].plant.GrowsInClusters)
					{
						cachedMaxWildPlantsClusterRadius = Mathf.Max(cachedMaxWildPlantsClusterRadius.Value, allWildPlants[i].plant.wildClusterRadius);
					}
				}
				List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					if (allDefsListForReading[j].category == ThingCategory.Plant && allDefsListForReading[j].plant.cavePlant)
					{
						cachedMaxWildPlantsClusterRadius = Mathf.Max(cachedMaxWildPlantsClusterRadius.Value, allDefsListForReading[j].plant.wildClusterRadius);
					}
				}
			}
			return cachedMaxWildPlantsClusterRadius.Value;
		}
	}

	public float LowestWildAndCavePlantOrder
	{
		get
		{
			if (!cachedLowestWildPlantOrder.HasValue)
			{
				cachedLowestWildPlantOrder = 2.1474836E+09f;
				List<ThingDef> allWildPlants = AllWildPlants;
				for (int i = 0; i < allWildPlants.Count; i++)
				{
					cachedLowestWildPlantOrder = Mathf.Min(cachedLowestWildPlantOrder.Value, allWildPlants[i].plant.wildOrder);
				}
				List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
				for (int j = 0; j < allDefsListForReading.Count; j++)
				{
					if (allDefsListForReading[j].category == ThingCategory.Plant && allDefsListForReading[j].plant.cavePlant)
					{
						cachedLowestWildPlantOrder = Mathf.Min(cachedLowestWildPlantOrder.Value, allDefsListForReading[j].plant.wildOrder);
					}
				}
			}
			return cachedLowestWildPlantOrder.Value;
		}
	}

	public IEnumerable<PawnKindDef> AllWildAnimals
	{
		get
		{
			foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				if (CommonalityOfAnimal(allDef) > 0f || CommonalityOfPollutionAnimal(allDef) > 0f || CommonalityOfCoastalAnimal(allDef) > 0f)
				{
					yield return allDef;
				}
			}
		}
	}

	public float TreeDensity
	{
		get
		{
			float num = 0f;
			float num2 = 0f;
			CachePlantCommonalitiesIfShould();
			foreach (KeyValuePair<ThingDef, float> cachedPlantCommonality in cachedPlantCommonalities)
			{
				num += cachedPlantCommonality.Value;
				if (cachedPlantCommonality.Key.plant.IsTree)
				{
					num2 += cachedPlantCommonality.Value;
				}
			}
			if (num == 0f)
			{
				return 0f;
			}
			return num2 / num * plantDensity;
		}
	}

	public int TreeSightingsPerHourFromCaravan => Mathf.FloorToInt(TreeDensity * 25f);

	public TerrainDef TerrainForAffordance(TerrainAffordanceDef affordance)
	{
		foreach (TerrainThreshold item in terrainsByFertility)
		{
			if (item.terrain.affordances.Contains(affordance))
			{
				return item.terrain;
			}
		}
		return TerrainDefOf.Soil;
	}

	public float CommonalityOfAnimal(PawnKindDef animalDef)
	{
		if (cachedAnimalCommonalities == null)
		{
			cachedAnimalCommonalities = new Dictionary<PawnKindDef, float>();
			for (int i = 0; i < wildAnimals.Count; i++)
			{
				cachedAnimalCommonalities.Add(wildAnimals[i].animal, wildAnimals[i].commonality);
			}
			foreach (PawnKindDef allDef in DefDatabase<PawnKindDef>.AllDefs)
			{
				if (allDef.RaceProps.wildBiomes == null)
				{
					continue;
				}
				for (int j = 0; j < allDef.RaceProps.wildBiomes.Count; j++)
				{
					if (allDef.RaceProps.wildBiomes[j].biome == this)
					{
						cachedAnimalCommonalities.Add(allDef, allDef.RaceProps.wildBiomes[j].commonality);
					}
				}
			}
		}
		if (cachedAnimalCommonalities.TryGetValue(animalDef, out var value))
		{
			return value;
		}
		return 0f;
	}

	public float CommonalityOfPollutionAnimal(PawnKindDef animalDef)
	{
		if (!ModsConfig.BiotechActive)
		{
			return 0f;
		}
		if (cachedPollutionAnimalCommonalities == null)
		{
			cachedPollutionAnimalCommonalities = new Dictionary<PawnKindDef, float>();
			for (int i = 0; i < pollutionWildAnimals.Count; i++)
			{
				cachedPollutionAnimalCommonalities.Add(pollutionWildAnimals[i].animal, pollutionWildAnimals[i].commonality);
			}
		}
		if (cachedPollutionAnimalCommonalities.TryGetValue(animalDef, out var value))
		{
			return value;
		}
		return 0f;
	}

	public float CommonalityOfCoastalAnimal(PawnKindDef animalDef)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return 0f;
		}
		if (cachedCoastalAnimalCommonalities == null)
		{
			cachedCoastalAnimalCommonalities = new Dictionary<PawnKindDef, float>();
			for (int i = 0; i < coastalWildAnimals.Count; i++)
			{
				cachedCoastalAnimalCommonalities.Add(coastalWildAnimals[i].animal, coastalWildAnimals[i].commonality);
			}
		}
		if (cachedCoastalAnimalCommonalities.TryGetValue(animalDef, out var value))
		{
			return value;
		}
		return 0f;
	}

	public bool ShouldSpawnAnimalOnCoast(PawnKindDef animalDef)
	{
		return CommonalityOfCoastalAnimal(animalDef) > CommonalityOfAnimal(animalDef);
	}

	public float CommonalityOfPlant(ThingDef plantDef)
	{
		CachePlantCommonalitiesIfShould();
		if (cachedPlantCommonalities.TryGetValue(plantDef, out var value))
		{
			return value;
		}
		return 0f;
	}

	public float CommonalityOfDisease(IncidentDef diseaseInc)
	{
		if (cachedDiseaseCommonalities == null)
		{
			cachedDiseaseCommonalities = new Dictionary<IncidentDef, float>();
			for (int i = 0; i < diseases.Count; i++)
			{
				cachedDiseaseCommonalities.Add(diseases[i].diseaseInc, diseases[i].commonality);
			}
			foreach (IncidentDef allDef in DefDatabase<IncidentDef>.AllDefs)
			{
				if (allDef.diseaseBiomeRecords == null)
				{
					continue;
				}
				for (int j = 0; j < allDef.diseaseBiomeRecords.Count; j++)
				{
					if (allDef.diseaseBiomeRecords[j].biome == this)
					{
						cachedDiseaseCommonalities.Add(allDef.diseaseBiomeRecords[j].diseaseInc, allDef.diseaseBiomeRecords[j].commonality);
					}
				}
			}
		}
		if (cachedDiseaseCommonalities.TryGetValue(diseaseInc, out var value))
		{
			return value;
		}
		return 0f;
	}

	private void CachePlantCommonalitiesIfShould()
	{
		if (cachedPlantCommonalities != null)
		{
			return;
		}
		cachedPlantCommonalities = new Dictionary<ThingDef, float>();
		for (int i = 0; i < wildPlants.Count; i++)
		{
			if (wildPlants[i].plant != null)
			{
				cachedPlantCommonalities.Add(wildPlants[i].plant, wildPlants[i].commonality);
			}
		}
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.plant == null || allDef.plant.wildBiomes == null)
			{
				continue;
			}
			for (int j = 0; j < allDef.plant.wildBiomes.Count; j++)
			{
				if (allDef.plant.wildBiomes[j].biome == this)
				{
					if (cachedPlantCommonalities.ContainsKey(allDef))
					{
						cachedPlantCommonalities[allDef] = (cachedPlantCommonalities[allDef] + allDef.plant.wildBiomes[j].commonality) / 2f;
					}
					else
					{
						cachedPlantCommonalities.Add(allDef, allDef.plant.wildBiomes[j].commonality);
					}
				}
			}
		}
	}

	public bool IsPackAnimalAllowed(ThingDef pawn)
	{
		return allowedPackAnimals.Contains(pawn);
	}

	public static BiomeDef Named(string defName)
	{
		return DefDatabase<BiomeDef>.GetNamed(defName);
	}

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (!Prefs.DevMode)
		{
			yield break;
		}
		foreach (BiomeAnimalRecord wa in wildAnimals)
		{
			if (wildAnimals.Count((BiomeAnimalRecord a) => a.animal == wa.animal) > 1)
			{
				yield return "Duplicate animal record: " + wa.animal.defName;
			}
		}
		if (ModsConfig.BiotechActive)
		{
			foreach (BiomeAnimalRecord pa in pollutionWildAnimals)
			{
				if (pollutionWildAnimals.Count((BiomeAnimalRecord a) => a.animal == pa.animal) > 1)
				{
					yield return "Duplicate pollution animal record: " + pa.animal.defName;
				}
			}
		}
		if (!ModsConfig.OdysseyActive)
		{
			yield break;
		}
		foreach (BiomeAnimalRecord ca in coastalWildAnimals)
		{
			if (coastalWildAnimals.Count((BiomeAnimalRecord a) => a.animal == ca.animal) > 1)
			{
				yield return "Duplicate coastal animal record: " + ca.animal.defName;
			}
		}
	}
}
