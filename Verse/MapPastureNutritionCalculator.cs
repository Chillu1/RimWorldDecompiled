using System.Collections.Generic;
using RimWorld;

namespace Verse;

public class MapPastureNutritionCalculator
{
	public struct NutritionPerDayPerQuadrum
	{
		private float quadrumOne;

		private float quadrumTwo;

		private float quadrumThree;

		private float quadrumFour;

		public float this[int key]
		{
			get
			{
				return GetValue(key);
			}
			set
			{
				SetValue(key, value);
			}
		}

		public float this[Quadrum key]
		{
			get
			{
				return GetValue((int)key);
			}
			set
			{
				SetValue((int)key, value);
			}
		}

		public float ForQuadrum(Quadrum q)
		{
			return this[q];
		}

		public void AddFrom(NutritionPerDayPerQuadrum other)
		{
			this[0] += other[0];
			this[3] += other[3];
			this[1] += other[1];
			this[2] += other[2];
		}

		public float GetValue(int key)
		{
			return key switch
			{
				0 => quadrumOne, 
				1 => quadrumTwo, 
				2 => quadrumThree, 
				3 => quadrumFour, 
				_ => -1f, 
			};
		}

		public void SetValue(int key, float value)
		{
			switch (key)
			{
			case 0:
				quadrumOne = value;
				break;
			case 1:
				quadrumTwo = value;
				break;
			case 2:
				quadrumThree = value;
				break;
			case 3:
				quadrumFour = value;
				break;
			}
		}
	}

	public Map map;

	private readonly Dictionary<ThingDef, Dictionary<TerrainDef, NutritionPerDayPerQuadrum>> cachedSeasonalDetailed = new Dictionary<ThingDef, Dictionary<TerrainDef, NutritionPerDayPerQuadrum>>();

	private readonly Dictionary<TerrainDef, NutritionPerDayPerQuadrum> cachedSeasonalByTerrain = new Dictionary<TerrainDef, NutritionPerDayPerQuadrum>();

	public void Reset(Map map)
	{
		this.map = map;
		cachedSeasonalDetailed.Clear();
		cachedSeasonalByTerrain.Clear();
	}

	public NutritionPerDayPerQuadrum CalculateAverageNutritionPerDay(TerrainDef terrain)
	{
		if (!cachedSeasonalByTerrain.TryGetValue(terrain, out var value))
		{
			value = default(NutritionPerDayPerQuadrum);
			foreach (ThingDef wildGrazingPlant in map.plantGrowthRateCalculator.WildGrazingPlants)
			{
				NutritionPerDayPerQuadrum other = CalculateAverageNutritionPerDay(wildGrazingPlant, terrain);
				value.AddFrom(other);
			}
			cachedSeasonalByTerrain.TryAdd(terrain, value);
		}
		return value;
	}

	private NutritionPerDayPerQuadrum CalculateAverageNutritionPerDay(ThingDef plantDef, TerrainDef terrain)
	{
		using (ProfilerBlock.Scope("CalculateAverageNutritionPerDay"))
		{
			if (!cachedSeasonalDetailed.TryGetValue(plantDef, out var value))
			{
				value = new Dictionary<TerrainDef, NutritionPerDayPerQuadrum>();
				cachedSeasonalDetailed.Add(plantDef, value);
			}
			if (!value.TryGetValue(terrain, out var value2))
			{
				value2 = default(NutritionPerDayPerQuadrum);
				value.Add(terrain, value2);
				value2[Quadrum.Aprimay] = GetAverageNutritionPerDay(Quadrum.Aprimay, plantDef, terrain);
				value2[Quadrum.Decembary] = GetAverageNutritionPerDay(Quadrum.Decembary, plantDef, terrain);
				value2[Quadrum.Jugust] = GetAverageNutritionPerDay(Quadrum.Jugust, plantDef, terrain);
				value2[Quadrum.Septober] = GetAverageNutritionPerDay(Quadrum.Septober, plantDef, terrain);
			}
			return value2;
		}
	}

	public float GetAverageNutritionPerDayToday(TerrainDef terrainDef)
	{
		using (ProfilerBlock.Scope("GetAverageNutritionPerDayToday"))
		{
			float num = 0f;
			foreach (ThingDef wildGrazingPlant in map.plantGrowthRateCalculator.WildGrazingPlants)
			{
				num += GetAverageNutritionPerDayToday(wildGrazingPlant, terrainDef);
			}
			return num;
		}
	}

	private float GetAverageNutritionPerDayToday(ThingDef plantDef, TerrainDef terrainDef)
	{
		if (!(terrainDef.fertility > 0f))
		{
			return 0f;
		}
		using (ProfilerBlock.Scope("GetAverageNutritionPerDayToday Plant/Terrain"))
		{
			int ticksAbs = Find.TickManager.TicksAbs;
			int nowTicks = ticksAbs - ticksAbs % 60000;
			float growthRate = map.plantGrowthRateCalculator.GrowthRateForDay(nowTicks, plantDef, terrainDef);
			return ComputeNutritionProducedPerDay(plantDef, growthRate);
		}
	}

	public float GetAverageNutritionPerDay(Quadrum quadrum, TerrainDef terrainDef)
	{
		float num = 0f;
		foreach (ThingDef wildGrazingPlant in map.plantGrowthRateCalculator.WildGrazingPlants)
		{
			num += GetAverageNutritionPerDay(quadrum, wildGrazingPlant, terrainDef);
		}
		return num;
	}

	public float GetAverageNutritionPerDay(Quadrum quadrum, ThingDef plantDef, TerrainDef terrainDef)
	{
		float growthRate = map.plantGrowthRateCalculator.QuadrumGrowthRateFor(quadrum, plantDef, terrainDef);
		return ComputeNutritionProducedPerDay(plantDef, growthRate);
	}

	private float ComputeNutritionProducedPerDay(ThingDef plantDef, float growthRate)
	{
		return SimplifiedPastureNutritionSimulator.NutritionProducedPerDay(map, plantDef, growthRate, map.wildPlantSpawner.CachedChanceFromDensity);
	}
}
