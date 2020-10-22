using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class BiomeDef : Def
	{
		public Type workerClass = typeof(BiomeWorker);

		public bool implemented = true;

		public bool canBuildBase = true;

		public bool canAutoChoose = true;

		public bool allowRoads = true;

		public bool allowRivers = true;

		public float animalDensity;

		public float plantDensity;

		public float diseaseMtbDays = 60f;

		public float settlementSelectionWeight = 1f;

		public bool impassable;

		public bool hasVirtualPlants = true;

		public float forageability;

		public ThingDef foragedFood;

		public bool wildPlantsCareAboutLocalFertility = true;

		public float wildPlantRegrowDays = 25f;

		public float movementDifficulty = 1f;

		public List<WeatherCommonalityRecord> baseWeatherCommonalities = new List<WeatherCommonalityRecord>();

		public List<TerrainThreshold> terrainsByFertility = new List<TerrainThreshold>();

		public List<SoundDef> soundsAmbient = new List<SoundDef>();

		public List<TerrainPatchMaker> terrainPatchMakers = new List<TerrainPatchMaker>();

		private List<BiomePlantRecord> wildPlants = new List<BiomePlantRecord>();

		private List<BiomeAnimalRecord> wildAnimals = new List<BiomeAnimalRecord>();

		private List<BiomeDiseaseRecord> diseases = new List<BiomeDiseaseRecord>();

		private List<ThingDef> allowedPackAnimals = new List<ThingDef>();

		public bool hasBedrock = true;

		public bool isExtremeBiome;

		[NoTranslate]
		public string texture;

		[Unsaved(false)]
		private Dictionary<PawnKindDef, float> cachedAnimalCommonalities;

		[Unsaved(false)]
		private Dictionary<ThingDef, float> cachedPlantCommonalities;

		[Unsaved(false)]
		private Dictionary<IncidentDef, float> cachedDiseaseCommonalities;

		[Unsaved(false)]
		private Material cachedMat;

		[Unsaved(false)]
		private List<ThingDef> cachedWildPlants;

		[Unsaved(false)]
		private int? cachedMaxWildPlantsClusterRadius;

		[Unsaved(false)]
		private float cachedPlantCommonalitiesSum;

		[Unsaved(false)]
		private float? cachedLowestWildPlantOrder;

		[Unsaved(false)]
		private BiomeWorker workerInt;

		public BiomeWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (BiomeWorker)Activator.CreateInstance(workerClass);
				}
				return workerInt;
			}
		}

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
					cachedLowestWildPlantOrder = 2.14748365E+09f;
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
					if (CommonalityOfAnimal(allDef) > 0f)
					{
						yield return allDef;
					}
				}
			}
		}

		public float PlantCommonalitiesSum
		{
			get
			{
				CachePlantCommonalitiesIfShould();
				return cachedPlantCommonalitiesSum;
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

		public float CommonalityOfPlant(ThingDef plantDef)
		{
			CachePlantCommonalitiesIfShould();
			if (cachedPlantCommonalities.TryGetValue(plantDef, out var value))
			{
				return value;
			}
			return 0f;
		}

		public float CommonalityPctOfPlant(ThingDef plantDef)
		{
			return CommonalityOfPlant(plantDef) / PlantCommonalitiesSum;
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

		public bool IsPackAnimalAllowed(ThingDef pawn)
		{
			return allowedPackAnimals.Contains(pawn);
		}

		public static BiomeDef Named(string defName)
		{
			return DefDatabase<BiomeDef>.GetNamed(defName);
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
						cachedPlantCommonalities.Add(allDef, allDef.plant.wildBiomes[j].commonality);
					}
				}
			}
			cachedPlantCommonalitiesSum = cachedPlantCommonalities.Sum((KeyValuePair<ThingDef, float> x) => x.Value);
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
		}
	}
}
