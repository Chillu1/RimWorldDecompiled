using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class WildPlantSpawner : IExposable
	{
		private Map map;

		private int cycleIndex;

		private float calculatedWholeMapNumDesiredPlants;

		private float calculatedWholeMapNumDesiredPlantsTmp;

		private int calculatedWholeMapNumNonZeroFertilityCells;

		private int calculatedWholeMapNumNonZeroFertilityCellsTmp;

		private bool hasWholeMapNumDesiredPlantsCalculated;

		private float? cachedCavePlantsCommonalitiesSum;

		private static List<ThingDef> allCavePlants = new List<ThingDef>();

		private static List<ThingDef> tmpPossiblePlants = new List<ThingDef>();

		private static List<KeyValuePair<ThingDef, float>> tmpPossiblePlantsWithWeight = new List<KeyValuePair<ThingDef, float>>();

		private static Dictionary<ThingDef, float> distanceSqToNearbyClusters = new Dictionary<ThingDef, float>();

		private static Dictionary<ThingDef, List<float>> nearbyClusters = new Dictionary<ThingDef, List<float>>();

		private static List<KeyValuePair<ThingDef, List<float>>> nearbyClustersList = new List<KeyValuePair<ThingDef, List<float>>>();

		private const float CavePlantsDensityFactor = 0.5f;

		private const int PlantSaturationScanRadius = 20;

		private const float MapFractionCheckPerTick = 0.0001f;

		private const float ChanceToRegrow = 0.012f;

		private const float CavePlantChanceToRegrow = 0.0001f;

		private const float BaseLowerOrderScanRadius = 7f;

		private const float LowerOrderScanRadiusWildClusterRadiusFactor = 1.5f;

		private const float MinDesiredLowerOrderPlantsToConsiderSkipping = 4f;

		private const float MinLowerOrderPlantsPct = 0.57f;

		private const float LocalPlantProportionsMaxScanRadius = 25f;

		private const float MaxLocalProportionsBias = 7f;

		private const float CavePlantRegrowDays = 130f;

		private static readonly SimpleCurve GlobalPctSelectionWeightBias = new SimpleCurve
		{
			new CurvePoint(0f, 3f),
			new CurvePoint(1f, 1f),
			new CurvePoint(1.5f, 0.25f),
			new CurvePoint(3f, 0.02f)
		};

		private static List<ThingDef> tmpPlantDefsLowerOrder = new List<ThingDef>();

		public float CurrentPlantDensity => map.Biome.plantDensity * map.gameConditionManager.AggregatePlantDensityFactor(map);

		public float CurrentWholeMapNumDesiredPlants
		{
			get
			{
				CellRect cellRect = CellRect.WholeMap(map);
				float currentPlantDensity = CurrentPlantDensity;
				float num = 0f;
				foreach (IntVec3 item in cellRect)
				{
					num += GetDesiredPlantsCountAt(item, item, currentPlantDensity);
				}
				return num;
			}
		}

		public int CurrentWholeMapNumNonZeroFertilityCells
		{
			get
			{
				CellRect cellRect = CellRect.WholeMap(map);
				int num = 0;
				foreach (IntVec3 item in cellRect)
				{
					if (item.GetTerrain(map).fertility > 0f)
					{
						num++;
					}
				}
				return num;
			}
		}

		public float CavePlantsCommonalitiesSum
		{
			get
			{
				if (!cachedCavePlantsCommonalitiesSum.HasValue)
				{
					cachedCavePlantsCommonalitiesSum = 0f;
					for (int i = 0; i < allCavePlants.Count; i++)
					{
						cachedCavePlantsCommonalitiesSum += GetCommonalityOfPlant(allCavePlants[i]);
					}
				}
				return cachedCavePlantsCommonalitiesSum.Value;
			}
		}

		public WildPlantSpawner(Map map)
		{
			this.map = map;
		}

		public static void ResetStaticData()
		{
			allCavePlants.Clear();
			allCavePlants.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Plant && x.plant.cavePlant));
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref cycleIndex, "cycleIndex", 0);
			Scribe_Values.Look(ref calculatedWholeMapNumDesiredPlants, "calculatedWholeMapNumDesiredPlants", 0f);
			Scribe_Values.Look(ref calculatedWholeMapNumDesiredPlantsTmp, "calculatedWholeMapNumDesiredPlantsTmp", 0f);
			Scribe_Values.Look(ref hasWholeMapNumDesiredPlantsCalculated, "hasWholeMapNumDesiredPlantsCalculated", defaultValue: true);
			Scribe_Values.Look(ref calculatedWholeMapNumNonZeroFertilityCells, "calculatedWholeMapNumNonZeroFertilityCells", 0);
			Scribe_Values.Look(ref calculatedWholeMapNumNonZeroFertilityCellsTmp, "calculatedWholeMapNumNonZeroFertilityCellsTmp", 0);
		}

		public void WildPlantSpawnerTick()
		{
			if (DebugSettings.fastEcology || DebugSettings.fastEcologyRegrowRateOnly)
			{
				for (int i = 0; i < 2000; i++)
				{
					WildPlantSpawnerTickInternal();
				}
			}
			else
			{
				WildPlantSpawnerTickInternal();
			}
		}

		private void WildPlantSpawnerTickInternal()
		{
			int area = map.Area;
			int num = Mathf.CeilToInt((float)area * 0.0001f);
			float currentPlantDensity = CurrentPlantDensity;
			if (!hasWholeMapNumDesiredPlantsCalculated)
			{
				calculatedWholeMapNumDesiredPlants = CurrentWholeMapNumDesiredPlants;
				calculatedWholeMapNumNonZeroFertilityCells = CurrentWholeMapNumNonZeroFertilityCells;
				hasWholeMapNumDesiredPlantsCalculated = true;
			}
			int num2 = Mathf.CeilToInt(10000f);
			float chance = calculatedWholeMapNumDesiredPlants / (float)calculatedWholeMapNumNonZeroFertilityCells;
			for (int i = 0; i < num; i++)
			{
				if (cycleIndex >= area)
				{
					calculatedWholeMapNumDesiredPlants = calculatedWholeMapNumDesiredPlantsTmp;
					calculatedWholeMapNumDesiredPlantsTmp = 0f;
					calculatedWholeMapNumNonZeroFertilityCells = calculatedWholeMapNumNonZeroFertilityCellsTmp;
					calculatedWholeMapNumNonZeroFertilityCellsTmp = 0;
					cycleIndex = 0;
				}
				IntVec3 intVec = map.cellsInRandomOrder.Get(cycleIndex);
				calculatedWholeMapNumDesiredPlantsTmp += GetDesiredPlantsCountAt(intVec, intVec, currentPlantDensity);
				if (intVec.GetTerrain(map).fertility > 0f)
				{
					calculatedWholeMapNumNonZeroFertilityCellsTmp++;
				}
				float mtb = GoodRoofForCavePlant(intVec) ? 130f : map.Biome.wildPlantRegrowDays;
				if (Rand.Chance(chance) && Rand.MTBEventOccurs(mtb, 60000f, num2) && CanRegrowAt(intVec))
				{
					CheckSpawnWildPlantAt(intVec, currentPlantDensity, calculatedWholeMapNumDesiredPlants);
				}
				cycleIndex++;
			}
		}

		public bool CheckSpawnWildPlantAt(IntVec3 c, float plantDensity, float wholeMapNumDesiredPlants, bool setRandomGrowth = false)
		{
			if (plantDensity <= 0f || c.GetPlant(map) != null || c.GetCover(map) != null || c.GetEdifice(map) != null || map.fertilityGrid.FertilityAt(c) <= 0f || !PlantUtility.SnowAllowsPlanting(c, map))
			{
				return false;
			}
			bool cavePlants = GoodRoofForCavePlant(c);
			if (SaturatedAt(c, plantDensity, cavePlants, wholeMapNumDesiredPlants))
			{
				return false;
			}
			CalculatePlantsWhichCanGrowAt(c, tmpPossiblePlants, cavePlants, plantDensity);
			if (!tmpPossiblePlants.Any())
			{
				return false;
			}
			CalculateDistancesToNearbyClusters(c);
			tmpPossiblePlantsWithWeight.Clear();
			for (int i = 0; i < tmpPossiblePlants.Count; i++)
			{
				float value = PlantChoiceWeight(tmpPossiblePlants[i], c, distanceSqToNearbyClusters, wholeMapNumDesiredPlants, plantDensity);
				tmpPossiblePlantsWithWeight.Add(new KeyValuePair<ThingDef, float>(tmpPossiblePlants[i], value));
			}
			if (!tmpPossiblePlantsWithWeight.TryRandomElementByWeight((KeyValuePair<ThingDef, float> x) => x.Value, out KeyValuePair<ThingDef, float> result))
			{
				return false;
			}
			Plant plant = (Plant)ThingMaker.MakeThing(result.Key);
			if (setRandomGrowth)
			{
				plant.Growth = Rand.Range(0.07f, 1f);
				if (plant.def.plant.LimitedLifespan)
				{
					plant.Age = Rand.Range(0, Mathf.Max(plant.def.plant.LifespanTicks - 50, 0));
				}
			}
			GenSpawn.Spawn(plant, c, map);
			return true;
		}

		private float PlantChoiceWeight(ThingDef plantDef, IntVec3 c, Dictionary<ThingDef, float> distanceSqToNearbyClusters, float wholeMapNumDesiredPlants, float plantDensity)
		{
			float commonalityOfPlant = GetCommonalityOfPlant(plantDef);
			float commonalityPctOfPlant = GetCommonalityPctOfPlant(plantDef);
			float num = commonalityOfPlant;
			if (num <= 0f)
			{
				return num;
			}
			float num2 = 0.5f;
			if ((float)map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count > wholeMapNumDesiredPlants / 2f && !plantDef.plant.cavePlant)
			{
				num2 = (float)map.listerThings.ThingsOfDef(plantDef).Count / (float)map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count / commonalityPctOfPlant;
				num *= GlobalPctSelectionWeightBias.Evaluate(num2);
			}
			if (plantDef.plant.GrowsInClusters && num2 < 1.1f)
			{
				float num3 = plantDef.plant.cavePlant ? CavePlantsCommonalitiesSum : map.Biome.PlantCommonalitiesSum;
				float x = commonalityOfPlant * plantDef.plant.wildClusterWeight / (num3 - commonalityOfPlant + commonalityOfPlant * plantDef.plant.wildClusterWeight);
				float outTo = 1f / ((float)Math.PI * (float)plantDef.plant.wildClusterRadius * (float)plantDef.plant.wildClusterRadius);
				outTo = GenMath.LerpDoubleClamped(commonalityPctOfPlant, 1f, 1f, outTo, x);
				if (distanceSqToNearbyClusters.TryGetValue(plantDef, out float value))
				{
					float x2 = Mathf.Sqrt(value);
					num *= GenMath.LerpDoubleClamped((float)plantDef.plant.wildClusterRadius * 0.9f, (float)plantDef.plant.wildClusterRadius * 1.1f, plantDef.plant.wildClusterWeight, outTo, x2);
				}
				else
				{
					num *= outTo;
				}
			}
			if (plantDef.plant.wildEqualLocalDistribution)
			{
				float f = wholeMapNumDesiredPlants * commonalityPctOfPlant;
				float a = (float)Mathf.Max(map.Size.x, map.Size.z) / Mathf.Sqrt(f) * 2f;
				if (plantDef.plant.GrowsInClusters)
				{
					a = Mathf.Max(a, (float)plantDef.plant.wildClusterRadius * 1.6f);
				}
				a = Mathf.Max(a, 7f);
				if (a <= 25f)
				{
					num *= LocalPlantProportionsWeightFactor(c, commonalityPctOfPlant, plantDensity, a, plantDef);
				}
			}
			return num;
		}

		private float LocalPlantProportionsWeightFactor(IntVec3 c, float commonalityPct, float plantDensity, float radiusToScan, ThingDef plantDef)
		{
			float numDesiredPlantsLocally = 0f;
			int numPlants = 0;
			int numPlantsThisDef = 0;
			RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
			{
				numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensity);
				numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.Plant).Count;
				numPlantsThisDef += reg.ListerThings.ThingsOfDef(plantDef).Count;
				return false;
			});
			if (numDesiredPlantsLocally * commonalityPct < 2f)
			{
				return 1f;
			}
			if ((float)numPlants <= numDesiredPlantsLocally * 0.5f)
			{
				return 1f;
			}
			float t = (float)numPlantsThisDef / (float)numPlants / commonalityPct;
			return Mathf.Lerp(7f, 1f, t);
		}

		private void CalculatePlantsWhichCanGrowAt(IntVec3 c, List<ThingDef> outPlants, bool cavePlants, float plantDensity)
		{
			outPlants.Clear();
			if (cavePlants)
			{
				for (int i = 0; i < allCavePlants.Count; i++)
				{
					if (allCavePlants[i].CanEverPlantAt_NewTemp(c, map))
					{
						outPlants.Add(allCavePlants[i]);
					}
				}
				return;
			}
			List<ThingDef> allWildPlants = map.Biome.AllWildPlants;
			for (int j = 0; j < allWildPlants.Count; j++)
			{
				ThingDef thingDef = allWildPlants[j];
				if (!thingDef.CanEverPlantAt_NewTemp(c, map))
				{
					continue;
				}
				if (thingDef.plant.wildOrder != map.Biome.LowestWildAndCavePlantOrder)
				{
					float num = 7f;
					if (thingDef.plant.GrowsInClusters)
					{
						num = Math.Max(num, (float)thingDef.plant.wildClusterRadius * 1.5f);
					}
					if (!EnoughLowerOrderPlantsNearby(c, plantDensity, num, thingDef))
					{
						continue;
					}
				}
				outPlants.Add(thingDef);
			}
		}

		private bool EnoughLowerOrderPlantsNearby(IntVec3 c, float plantDensity, float radiusToScan, ThingDef plantDef)
		{
			float num = 0f;
			tmpPlantDefsLowerOrder.Clear();
			List<ThingDef> allWildPlants = map.Biome.AllWildPlants;
			for (int i = 0; i < allWildPlants.Count; i++)
			{
				if (allWildPlants[i].plant.wildOrder < plantDef.plant.wildOrder)
				{
					num += GetCommonalityPctOfPlant(allWildPlants[i]);
					tmpPlantDefsLowerOrder.Add(allWildPlants[i]);
				}
			}
			float numDesiredPlantsLocally = 0f;
			int numPlantsLowerOrder = 0;
			RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
			{
				numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensity);
				for (int j = 0; j < tmpPlantDefsLowerOrder.Count; j++)
				{
					numPlantsLowerOrder += reg.ListerThings.ThingsOfDef(tmpPlantDefsLowerOrder[j]).Count;
				}
				return false;
			});
			float num2 = numDesiredPlantsLocally * num;
			if (num2 < 4f)
			{
				return true;
			}
			return (float)numPlantsLowerOrder / num2 >= 0.57f;
		}

		private bool SaturatedAt(IntVec3 c, float plantDensity, bool cavePlants, float wholeMapNumDesiredPlants)
		{
			int num = GenRadial.NumCellsInRadius(20f);
			if (wholeMapNumDesiredPlants * ((float)num / (float)map.Area) <= 4f || !map.Biome.wildPlantsCareAboutLocalFertility)
			{
				return (float)map.listerThings.ThingsInGroup(ThingRequestGroup.Plant).Count >= wholeMapNumDesiredPlants;
			}
			float numDesiredPlantsLocally = 0f;
			int numPlants = 0;
			RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), 20f), delegate(Region reg)
			{
				numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensity);
				numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.Plant).Count;
				return false;
			});
			return (float)numPlants >= numDesiredPlantsLocally;
		}

		private void CalculateDistancesToNearbyClusters(IntVec3 c)
		{
			nearbyClusters.Clear();
			nearbyClustersList.Clear();
			int num = GenRadial.NumCellsInRadius(map.Biome.MaxWildAndCavePlantsClusterRadius * 2);
			for (int i = 0; i < num; i++)
			{
				IntVec3 intVec = c + GenRadial.RadialPattern[i];
				if (!intVec.InBounds(map))
				{
					continue;
				}
				List<Thing> list = map.thingGrid.ThingsListAtFast(intVec);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing = list[j];
					if (thing.def.category == ThingCategory.Plant && thing.def.plant.GrowsInClusters)
					{
						float item = intVec.DistanceToSquared(c);
						if (!nearbyClusters.TryGetValue(thing.def, out List<float> value))
						{
							value = SimplePool<List<float>>.Get();
							nearbyClusters.Add(thing.def, value);
							nearbyClustersList.Add(new KeyValuePair<ThingDef, List<float>>(thing.def, value));
						}
						value.Add(item);
					}
				}
			}
			distanceSqToNearbyClusters.Clear();
			for (int k = 0; k < nearbyClustersList.Count; k++)
			{
				List<float> value2 = nearbyClustersList[k].Value;
				value2.Sort();
				distanceSqToNearbyClusters.Add(nearbyClustersList[k].Key, value2[value2.Count / 2]);
				value2.Clear();
				SimplePool<List<float>>.Return(value2);
			}
		}

		private bool CanRegrowAt(IntVec3 c)
		{
			if (c.GetTemperature(map) > 0f)
			{
				if (c.Roofed(map))
				{
					return GoodRoofForCavePlant(c);
				}
				return true;
			}
			return false;
		}

		private bool GoodRoofForCavePlant(IntVec3 c)
		{
			return c.GetRoof(map)?.isNatural ?? false;
		}

		private float GetCommonalityOfPlant(ThingDef plant)
		{
			if (!plant.plant.cavePlant)
			{
				return map.Biome.CommonalityOfPlant(plant);
			}
			return plant.plant.cavePlantWeight;
		}

		private float GetCommonalityPctOfPlant(ThingDef plant)
		{
			if (!plant.plant.cavePlant)
			{
				return map.Biome.CommonalityPctOfPlant(plant);
			}
			return GetCommonalityOfPlant(plant) / CavePlantsCommonalitiesSum;
		}

		public float GetBaseDesiredPlantsCountAt(IntVec3 c)
		{
			float num = c.GetTerrain(map).fertility;
			if (GoodRoofForCavePlant(c))
			{
				num *= 0.5f;
			}
			return num;
		}

		public float GetDesiredPlantsCountAt(IntVec3 c, IntVec3 forCell, float plantDensity)
		{
			return Mathf.Min(GetBaseDesiredPlantsCountAt(c) * plantDensity * forCell.GetTerrain(map).fertility, 1f);
		}

		public float GetDesiredPlantsCountIn(Region reg, IntVec3 forCell, float plantDensity)
		{
			return Mathf.Min(reg.GetBaseDesiredPlantsCount() * plantDensity * forCell.GetTerrain(map).fertility, reg.CellCount);
		}
	}
}
