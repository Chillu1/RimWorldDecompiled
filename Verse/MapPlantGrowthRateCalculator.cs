using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace Verse;

public class MapPlantGrowthRateCalculator
{
	private class GrowthRateAccumulator
	{
		public readonly ThingDef plantDef;

		private float sumGrowthRateForTemperature;

		private float sumGrowthRateForGlow;

		private int numSamples;

		public float GrowthRateForTemperature
		{
			get
			{
				if (numSamples != 0)
				{
					return sumGrowthRateForTemperature / (float)numSamples;
				}
				return 0f;
			}
		}

		public float GrowthRateForGlow
		{
			get
			{
				if (numSamples != 0)
				{
					return sumGrowthRateForGlow / (float)numSamples;
				}
				return 0f;
			}
		}

		public GrowthRateAccumulator(ThingDef plantDef)
		{
			this.plantDef = plantDef;
		}

		public void Accumulate(float grTemp, float grGlow)
		{
			sumGrowthRateForTemperature += grTemp;
			sumGrowthRateForGlow += grGlow;
			numSamples++;
		}

		public void Accumulate(GrowthRateAccumulator other)
		{
			sumGrowthRateForTemperature += other.sumGrowthRateForTemperature;
			sumGrowthRateForGlow += other.sumGrowthRateForGlow;
			numSamples += other.numSamples;
		}
	}

	private class PlantGrowthRates
	{
		public readonly Dictionary<ThingDef, GrowthRateAccumulator> byPlant = new Dictionary<ThingDef, GrowthRateAccumulator>();

		public GrowthRateAccumulator For(ThingDef plantDef)
		{
			if (!byPlant.TryGetValue(plantDef, out var value))
			{
				value = new GrowthRateAccumulator(plantDef);
				byPlant.Add(plantDef, value);
			}
			return value;
		}
	}

	private Vector2 longLat;

	private PlanetTile tile;

	private BiomeDef biome;

	private bool dirty = true;

	private readonly List<ThingDef> includeAnimalTypes = new List<ThingDef>();

	private List<TerrainDef> terrainDefs;

	private List<ThingDef> wildGrazingPlants;

	private readonly Dictionary<Quadrum, PlantGrowthRates> seasonalGrowthRates = new Dictionary<Quadrum, PlantGrowthRates>();

	private readonly List<PlantGrowthRates> dailyGrowthRates = new List<PlantGrowthRates>();

	public List<TerrainDef> TerrainDefs
	{
		get
		{
			ComputeIfDirty();
			return terrainDefs;
		}
	}

	public List<ThingDef> WildGrazingPlants
	{
		get
		{
			ComputeIfDirty();
			return wildGrazingPlants;
		}
	}

	public List<ThingDef> GrazingAnimals
	{
		get
		{
			ComputeIfDirty();
			return includeAnimalTypes;
		}
	}

	public void BuildFor(PlanetTile tile)
	{
		this.tile = tile;
		longLat = Find.WorldGrid.LongLatOf(tile);
		biome = Find.WorldGrid[tile].PrimaryBiome;
		ComputeIfDirty();
	}

	public void BuildFor(Map map)
	{
		tile = map.Tile;
		longLat = Find.WorldGrid.LongLatOf(map.Tile);
		biome = map.Biome;
		ComputeIfDirty();
	}

	public float GrowthRateForDay(int nowTicks, ThingDef plantDef, TerrainDef terrainDef)
	{
		using (ProfilerBlock.Scope("GrowthRateForDay"))
		{
			ComputeIfDirty();
			int index = nowTicks / 60000 % 60;
			return ComputeGrowthRate(plantDef, terrainDef, dailyGrowthRates[index]);
		}
	}

	public float QuadrumGrowthRateFor(Quadrum quadrum, ThingDef plantDef, TerrainDef terrainDef)
	{
		ComputeIfDirty();
		return ComputeGrowthRate(plantDef, terrainDef, seasonalGrowthRates[quadrum]);
	}

	private static float ComputeGrowthRate(ThingDef plantDef, TerrainDef terrainDef, PlantGrowthRates rates)
	{
		if (!plantDef.plant.completelyIgnoreFertility && terrainDef.fertility < plantDef.plant.fertilityMin)
		{
			return 0f;
		}
		GrowthRateAccumulator growthRateAccumulator = rates.For(plantDef);
		return PlantUtility.GrowthRateFactorFor_Fertility(plantDef, terrainDef.fertility) * growthRateAccumulator.GrowthRateForTemperature * growthRateAccumulator.GrowthRateForGlow;
	}

	private void ComputeIfDirty()
	{
		if (!dirty)
		{
			return;
		}
		using (ProfilerBlock.Scope("MapPlantGrowthRateCalculator.ComputeIfDirty"))
		{
			dirty = false;
			includeAnimalTypes.Clear();
			seasonalGrowthRates.Clear();
			dailyGrowthRates.Clear();
			seasonalGrowthRates.Add(Quadrum.Aprimay, new PlantGrowthRates());
			seasonalGrowthRates.Add(Quadrum.Decembary, new PlantGrowthRates());
			seasonalGrowthRates.Add(Quadrum.Jugust, new PlantGrowthRates());
			seasonalGrowthRates.Add(Quadrum.Septober, new PlantGrowthRates());
			AddIncludedAnimals();
			terrainDefs = DefDatabase<TerrainDef>.AllDefsListForReading;
			wildGrazingPlants = biome.AllWildPlants.Where(IsEdibleByPastureAnimals).ToList();
			CalculateDailyFertility();
			CalculateSeasonalFertility();
		}
	}

	private void CalculateDailyFertility()
	{
		for (int i = 0; i < 60; i++)
		{
			int num = i * 60000;
			int nowTicks = num - num % 60000;
			PlantGrowthRates plantGrowthRates = new PlantGrowthRates();
			dailyGrowthRates.Add(plantGrowthRates);
			foreach (ThingDef wildGrazingPlant in wildGrazingPlants)
			{
				SimulateGrowthRateForDay(nowTicks, plantGrowthRates.For(wildGrazingPlant));
			}
		}
	}

	private void CalculateSeasonalFertility()
	{
		for (int i = 0; i < dailyGrowthRates.Count; i++)
		{
			Quadrum key = GenDate.Quadrum(i * 60000, longLat.x);
			PlantGrowthRates plantGrowthRates = seasonalGrowthRates[key];
			foreach (KeyValuePair<ThingDef, GrowthRateAccumulator> item in dailyGrowthRates[i].byPlant)
			{
				plantGrowthRates.For(item.Value.plantDef).Accumulate(item.Value);
			}
		}
	}

	private void AddIncludedAnimals()
	{
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (IsPastureAnimal(allDef))
			{
				includeAnimalTypes.Add(allDef);
			}
		}
		includeAnimalTypes.Sort((ThingDef a, ThingDef b) => string.CompareOrdinal(a.label, b.label));
	}

	public static bool IsPastureAnimal(ThingDef td)
	{
		if (!td.IsCorpse && td.race != null && td.race.Animal)
		{
			return td.race.Roamer;
		}
		return false;
	}

	public static bool IsEdibleByPastureAnimals(ThingDef foodDef)
	{
		if (foodDef.ingestible == null)
		{
			return false;
		}
		if (foodDef.ingestible.preferability == FoodPreferability.Undefined)
		{
			return false;
		}
		return (FoodTypeFlags.VegetarianRoughAnimal & foodDef.ingestible.foodType) != 0;
	}

	private void SimulateGrowthRateForDay(int nowTicks, GrowthRateAccumulator growthRates)
	{
		int num = nowTicks - nowTicks % 60000;
		int num2 = 24;
		int num3 = 60000 / num2;
		for (int i = 0; i < num2; i++)
		{
			int num4 = num + i * num3;
			float cellTemp = Find.World.tileTemperatures.OutdoorTemperatureAt(tile, num4);
			float glow = GenCelestial.CelestialSunGlow(tile, num4);
			float grTemp = PlantUtility.GrowthRateFactorFor_Temperature(growthRates.plantDef, cellTemp);
			float grGlow = PlantUtility.GrowthRateFactorFor_Light(growthRates.plantDef, glow);
			growthRates.Accumulate(grTemp, grGlow);
		}
	}
}
