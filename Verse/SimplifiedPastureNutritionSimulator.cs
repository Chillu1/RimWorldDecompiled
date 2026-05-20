using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class SimplifiedPastureNutritionSimulator
{
	private struct CacheKey : IEquatable<CacheKey>
	{
		public Map map;

		public ThingDef plantDef;

		public float mapRespawnChance;

		public bool Equals(CacheKey other)
		{
			if (object.Equals(map, other.map) && object.Equals(plantDef, other.plantDef))
			{
				return mapRespawnChance.Equals(other.mapRespawnChance);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is CacheKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(map, plantDef, mapRespawnChance);
		}
	}

	private const float UnderEstimateNutritionFactor = 0.85f;

	private const float CacheGrowthRateRange = 0.05f;

	private static Game cachedGame;

	private static readonly Dictionary<CacheKey, (float growthRate, float perDay)> nutritionCache = new Dictionary<CacheKey, (float, float)>();

	public static float NutritionProducedPerDay(Map map, ThingDef plantDef, float averageGrowthRate, float mapRespawnChance)
	{
		using (ProfilerBlock.Scope("NutritionProducedPerDay"))
		{
			if (Mathf.Approximately(averageGrowthRate, 0f))
			{
				return 0f;
			}
			if (cachedGame != Current.Game)
			{
				cachedGame = Current.Game;
				nutritionCache.Clear();
			}
			CacheKey key = new CacheKey
			{
				map = map,
				mapRespawnChance = mapRespawnChance,
				plantDef = plantDef
			};
			if (nutritionCache.TryGetValue(key, out (float, float) value) && Mathf.Abs(value.Item1 - averageGrowthRate) <= 0.05f)
			{
				return value.Item2;
			}
			using (ProfilerBlock.Scope("Cache_NutritionProducedPerDay"))
			{
				float num = map.Biome.wildPlantRegrowDays / mapRespawnChance;
				float num2 = plantDef.plant.growDays / averageGrowthRate * plantDef.plant.harvestMinGrowth;
				float num3 = plantDef.GetStatValueAbstract(StatDefOf.Nutrition) * PlantUtility.NutritionFactorFromGrowth(plantDef, plantDef.plant.harvestMinGrowth) / (num + num2) * map.wildPlantSpawner.GetCommonalityPctOfPlant(plantDef) * 0.85f;
				nutritionCache[key] = (averageGrowthRate, num3);
				return num3;
			}
		}
	}

	public static float NutritionConsumedPerDay(Pawn animal)
	{
		return NutritionConsumedPerDay(animal.def, animal.ageTracker.CurLifeStage);
	}

	public static float NutritionConsumedPerDay(ThingDef animalDef)
	{
		LifeStageAge lifeStageAge = animalDef.race.lifeStageAges.Last();
		return NutritionConsumedPerDay(animalDef, lifeStageAge.def);
	}

	public static float NutritionConsumedPerDay(ThingDef animalDef, LifeStageDef lifeStageDef)
	{
		return Need_Food.BaseHungerRate(lifeStageDef, animalDef) * 60000f;
	}
}
