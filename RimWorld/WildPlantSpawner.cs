using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class WildPlantSpawner : IExposable
{
	private readonly Map map;

	private int cycleIndex;

	private float calculatedWholeMapNumDesiredPlants;

	private float calculatedWholeMapNumDesiredPlantsTmp;

	private int calculatedWholeMapNumNonZeroFertilityCells;

	private int calculatedWholeMapNumNonZeroFertilityCellsTmp;

	private bool hasWholeMapNumDesiredPlantsCalculated;

	[Unsaved(false)]
	private List<ThingDef> cachedWildPlants;

	[Unsaved(false)]
	private List<ThingDef> cachedMutatorWildPlants;

	[Unsaved(false)]
	private Dictionary<ThingDef, float> cachedPlantCommonalities;

	[Unsaved(false)]
	private float? cachedPlantsCommonalitiesSum;

	[Unsaved(false)]
	private float? cachedCavePlantsCommonalitiesSum;

	[Unsaved(false)]
	private bool? cachedHaveAnyPlantsWhichIgnoreFertility;

	[Unsaved(false)]
	private SimpleCurve cachedOverrideDensityForFertilityCurve;

	private static readonly List<ThingDef> allCavePlants = new List<ThingDef>();

	private static readonly List<ThingDef> tmpPossiblePlants = new List<ThingDef>();

	private static readonly List<KeyValuePair<ThingDef, float>> tmpPossiblePlantsWithWeight = new List<KeyValuePair<ThingDef, float>>();

	private static readonly Dictionary<ThingDef, float> distanceSqToNearbyClusters = new Dictionary<ThingDef, float>();

	private static readonly Dictionary<ThingDef, List<float>> nearbyClusters = new Dictionary<ThingDef, List<float>>();

	private static readonly List<KeyValuePair<ThingDef, List<float>>> nearbyClustersList = new List<KeyValuePair<ThingDef, List<float>>>();

	public static readonly FloatRange InitialGrowthRandomRange = new FloatRange(0.15f, 1.5f);

	private const float CavePlantsDensityFactor = 0.5f;

	private const float WaterPlantsDensityFactor = 0.1f;

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

	private static readonly List<ThingDef> tmpWildPlants = new List<ThingDef>();

	private static readonly List<ThingDef> tmpPlantDefsLowerOrder = new List<ThingDef>();

	public float CurrentPlantDensityFactor => map.TileInfo.PlantDensityFactor * map.gameConditionManager.AggregatePlantDensityFactor(map);

	public float CurrentWholeMapNumDesiredPlants
	{
		get
		{
			CellRect cellRect = CellRect.WholeMap(map);
			float currentPlantDensityFactor = CurrentPlantDensityFactor;
			float num = 0f;
			foreach (IntVec3 item in cellRect)
			{
				num += GetDesiredPlantsCountAt(item, currentPlantDensityFactor);
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
				if (item.GetFertility(map) > 0f)
				{
					num++;
				}
			}
			return num;
		}
	}

	public float CachedChanceFromDensity
	{
		get
		{
			CacheWholeMapNumDesiredPlants();
			return calculatedWholeMapNumDesiredPlants / (float)calculatedWholeMapNumNonZeroFertilityCells;
		}
	}

	public SimpleCurve OverrideDensityForFertilityCurve
	{
		get
		{
			if (cachedOverrideDensityForFertilityCurve != null)
			{
				return cachedOverrideDensityForFertilityCurve;
			}
			cachedOverrideDensityForFertilityCurve = new SimpleCurve { new CurvePoint(0f, -1f) };
			foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
			{
				if (mutator.overrideDensityForFertilityCurve != null)
				{
					cachedOverrideDensityForFertilityCurve = mutator.overrideDensityForFertilityCurve;
				}
			}
			return cachedOverrideDensityForFertilityCurve;
		}
	}

	public List<ThingDef> AllWildPlants
	{
		get
		{
			if (cachedWildPlants != null)
			{
				return cachedWildPlants;
			}
			cachedWildPlants = new List<ThingDef>();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (item.category == ThingCategory.Plant && GetCommonalityOfPlant(item) > 0f)
				{
					cachedWildPlants.Add(item);
				}
			}
			return cachedWildPlants;
		}
	}

	public List<ThingDef> MutatorWildPlants
	{
		get
		{
			if (cachedMutatorWildPlants != null)
			{
				return cachedMutatorWildPlants;
			}
			cachedMutatorWildPlants = new List<ThingDef>();
			foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
			{
				foreach (BiomePlantRecord item in mutator.Worker?.AdditionalWildPlants(map.Tile) ?? Enumerable.Empty<BiomePlantRecord>())
				{
					if (!cachedMutatorWildPlants.Contains(item.plant))
					{
						cachedMutatorWildPlants.Add(item.plant);
					}
				}
			}
			return cachedMutatorWildPlants;
		}
	}

	private bool HaveAnyPlantsWhichIgnoreFertility
	{
		get
		{
			if (!cachedHaveAnyPlantsWhichIgnoreFertility.HasValue)
			{
				cachedHaveAnyPlantsWhichIgnoreFertility = false;
				bool? flag = cachedHaveAnyPlantsWhichIgnoreFertility;
				cachedHaveAnyPlantsWhichIgnoreFertility = AllWildPlants.Any((ThingDef p) => p.plant.completelyIgnoreFertility) | flag;
			}
			return cachedHaveAnyPlantsWhichIgnoreFertility.Value;
		}
	}

	public float PlantsCommonalitiesSum
	{
		get
		{
			if (!cachedPlantsCommonalitiesSum.HasValue)
			{
				cachedPlantsCommonalitiesSum = 0f;
				for (int i = 0; i < AllWildPlants.Count; i++)
				{
					cachedPlantsCommonalitiesSum += GetCommonalityOfPlant(AllWildPlants[i]);
				}
			}
			return cachedPlantsCommonalitiesSum.Value;
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
		allCavePlants.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef x) => x.category == ThingCategory.Plant && x.plant.cavePlant && x.plant.cavePlantWeight > 0f));
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
		float currentPlantDensityFactor = CurrentPlantDensityFactor;
		CacheWholeMapNumDesiredPlants();
		int num2 = Mathf.CeilToInt(10000f);
		float cachedChanceFromDensity = CachedChanceFromDensity;
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
			calculatedWholeMapNumDesiredPlantsTmp += GetDesiredPlantsCountAt(intVec, currentPlantDensityFactor);
			if (map.fertilityGrid.FertilityAt(intVec) > 0f)
			{
				calculatedWholeMapNumNonZeroFertilityCellsTmp++;
			}
			float mtb = (GoodRoofForCavePlant(intVec) ? 130f : map.BiomeAt(intVec).wildPlantRegrowDays);
			float num3 = OverrideDensityForFertilityCurve.Evaluate(map.fertilityGrid.FertilityAt(intVec));
			if (Rand.Chance((num3 > 0f) ? num3 : cachedChanceFromDensity) && Rand.MTBEventOccurs(mtb, 60000f, num2) && CanRegrowAt(intVec))
			{
				CheckSpawnWildPlantAt(intVec, currentPlantDensityFactor, calculatedWholeMapNumDesiredPlants);
			}
			cycleIndex++;
		}
	}

	private void CachePlantCommonalitiesIfShould()
	{
		if (cachedPlantCommonalities != null)
		{
			return;
		}
		cachedPlantCommonalities = new Dictionary<ThingDef, float>();
		foreach (BiomeDef biome in map.Biomes)
		{
			foreach (BiomePlantRecord wildPlant in biome.wildPlants)
			{
				if (wildPlant.plant != null)
				{
					if (cachedPlantCommonalities.ContainsKey(wildPlant.plant))
					{
						cachedPlantCommonalities[wildPlant.plant] = (cachedPlantCommonalities[wildPlant.plant] + wildPlant.commonality) / 2f;
					}
					else
					{
						cachedPlantCommonalities.Add(wildPlant.plant, wildPlant.commonality);
					}
				}
			}
		}
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.plant == null || allDef.plant.wildBiomes == null)
			{
				continue;
			}
			for (int i = 0; i < allDef.plant.wildBiomes.Count; i++)
			{
				if (map.Biomes.Contains(allDef.plant.wildBiomes[i].biome))
				{
					if (cachedPlantCommonalities.ContainsKey(allDef))
					{
						cachedPlantCommonalities[allDef] = (cachedPlantCommonalities[allDef] + allDef.plant.wildBiomes[i].commonality) / 2f;
					}
					else
					{
						cachedPlantCommonalities.Add(allDef, allDef.plant.wildBiomes[i].commonality);
					}
				}
			}
		}
		foreach (TileMutatorDef mutator in map.TileInfo.Mutators)
		{
			foreach (BiomePlantRecord item in mutator.Worker?.AdditionalWildPlants(map.Tile) ?? Enumerable.Empty<BiomePlantRecord>())
			{
				if (item.plant != null)
				{
					if (cachedPlantCommonalities.ContainsKey(item.plant))
					{
						cachedPlantCommonalities[item.plant] = Mathf.Max(cachedPlantCommonalities[item.plant], item.commonality);
					}
					else
					{
						cachedPlantCommonalities.Add(item.plant, item.commonality);
					}
				}
			}
		}
		foreach (ThingDef item2 in cachedPlantCommonalities.Keys.ToList())
		{
			float num = cachedPlantCommonalities[item2];
			int num2 = map.Biomes.Count();
			foreach (BiomeDef biome2 in map.Biomes)
			{
				num = ((!item2.plant.cavePlant || biome2.wildPlantsAreCavePlants) ? (num + cachedPlantCommonalities.GetWithFallback(item2, 0f) / (float)num2) : (num + item2.plant.cavePlantWeight / (float)num2));
				foreach (TileMutatorDef mutator2 in map.TileInfo.Mutators)
				{
					if (mutator2.Worker != null)
					{
						num *= mutator2.Worker.PlantCommonalityFactorFor(item2, map.Tile);
					}
				}
			}
			cachedPlantCommonalities[item2] = num;
		}
	}

	private void CacheWholeMapNumDesiredPlants()
	{
		if (!hasWholeMapNumDesiredPlantsCalculated)
		{
			calculatedWholeMapNumDesiredPlants = CurrentWholeMapNumDesiredPlants;
			calculatedWholeMapNumNonZeroFertilityCells = CurrentWholeMapNumNonZeroFertilityCells;
			hasWholeMapNumDesiredPlantsCalculated = true;
		}
	}

	public bool CheckSpawnWildPlantAt(IntVec3 c, float plantDensityFactor, float wholeMapNumDesiredPlants, bool setRandomGrowth = false)
	{
		if (plantDensityFactor <= 0f || c.GetPlant(map) != null || c.GetCover(map) != null || c.GetEdifice(map) != null || (!HaveAnyPlantsWhichIgnoreFertility && map.fertilityGrid.FertilityAt(c) <= 0f) || !PlantUtility.SnowAllowsPlanting(c, map) || !PlantUtility.SandAllowsPlanting(c, map))
		{
			return false;
		}
		bool cavePlants = GoodRoofForCavePlant(c);
		float num = OverrideDensityForFertilityCurve.Evaluate(map.fertilityGrid.FertilityAt(c));
		if (num > 0f)
		{
			if (!Rand.Chance(num))
			{
				return false;
			}
		}
		else if (SaturatedAt(c, plantDensityFactor, cavePlants, wholeMapNumDesiredPlants))
		{
			return false;
		}
		CalculatePlantsWhichCanGrowAt(c, tmpPossiblePlants, cavePlants, plantDensityFactor);
		if (!tmpPossiblePlants.Any())
		{
			return false;
		}
		CalculateDistancesToNearbyClusters(c);
		tmpPossiblePlantsWithWeight.Clear();
		foreach (ThingDef tmpPossiblePlant in tmpPossiblePlants)
		{
			float value = PlantChoiceWeight(tmpPossiblePlant, c, distanceSqToNearbyClusters, wholeMapNumDesiredPlants, plantDensityFactor);
			tmpPossiblePlantsWithWeight.Add(new KeyValuePair<ThingDef, float>(tmpPossiblePlant, value));
		}
		if (!tmpPossiblePlantsWithWeight.TryRandomElementByWeight((KeyValuePair<ThingDef, float> x) => x.Value, out var result))
		{
			return false;
		}
		if (result.Key.plant.wildPlantUseDistanceToShore && !Rand.Chance(GetWaterPlantDistanceToShoreWeight(c)))
		{
			return false;
		}
		SpawnPlant(result.Key, map, c, setRandomGrowth);
		return true;
	}

	public static Plant SpawnPlant(ThingDef plant, Map map, IntVec3 cell, bool setRandomGrowth)
	{
		Plant plant2 = (Plant)ThingMaker.MakeThing(plant);
		if (setRandomGrowth)
		{
			plant2.Growth = Mathf.Clamp01(InitialGrowthRandomRange.RandomInRange);
			if (plant2.def.plant.LimitedLifespan)
			{
				plant2.Age = Rand.Range(0, Mathf.Max(plant2.def.plant.LifespanTicks - 50, 0));
			}
		}
		GenSpawn.Spawn(plant2, cell, map);
		return plant2;
	}

	private float PlantChoiceWeight(ThingDef plantDef, IntVec3 c, Dictionary<ThingDef, float> distanceSqToNearbyClusters, float wholeMapNumDesiredPlants, float plantDensityFactor)
	{
		float commonalityOfPlant = GetCommonalityOfPlant(plantDef);
		float num = GetCommonalityPctOfPlant(plantDef);
		if (Current.ProgramState == ProgramState.Playing)
		{
			num *= plantDef.plant.plantRespawningCommonalityFactor;
		}
		float num2 = commonalityOfPlant;
		if (num2 <= 0f)
		{
			return num2;
		}
		float num3 = 0.5f;
		num3 = (float)map.listerThings.ThingsOfDef(plantDef).Count / (float)map.listerThings.ThingsInGroup(ThingRequestGroup.NonStumpPlant).Count / num;
		num2 *= GlobalPctSelectionWeightBias.Evaluate(num3);
		if (plantDef.plant.GrowsInClusters && num3 < 1.1f)
		{
			float num4 = ((plantDef.plant.cavePlant && !map.BiomeAt(c).wildPlantsAreCavePlants) ? CavePlantsCommonalitiesSum : PlantsCommonalitiesSum);
			float x = commonalityOfPlant * plantDef.plant.wildClusterWeight / (num4 - commonalityOfPlant + commonalityOfPlant * plantDef.plant.wildClusterWeight);
			float outTo = 1f / (MathF.PI * (float)plantDef.plant.wildClusterRadius * (float)plantDef.plant.wildClusterRadius);
			outTo = GenMath.LerpDoubleClamped(num, 1f, 1f, outTo, x);
			if (distanceSqToNearbyClusters.TryGetValue(plantDef, out var value))
			{
				float x2 = Mathf.Sqrt(value);
				num2 *= GenMath.LerpDoubleClamped((float)plantDef.plant.wildClusterRadius * 0.9f, (float)plantDef.plant.wildClusterRadius * 1.1f, plantDef.plant.wildClusterWeight, outTo, x2);
			}
			else
			{
				num2 *= outTo;
			}
		}
		if (plantDef.plant.wildEqualLocalDistribution)
		{
			float f = wholeMapNumDesiredPlants * num;
			float a = (float)Mathf.Max(map.Size.x, map.Size.z) / Mathf.Sqrt(f) * 2f;
			if (plantDef.plant.GrowsInClusters)
			{
				a = Mathf.Max(a, (float)plantDef.plant.wildClusterRadius * 1.6f);
			}
			a = Mathf.Max(a, 7f);
			if (a <= 25f)
			{
				num2 *= LocalPlantProportionsWeightFactor(c, num, plantDensityFactor, a, plantDef);
			}
		}
		return num2;
	}

	private float LocalPlantProportionsWeightFactor(IntVec3 c, float commonalityPct, float plantDensityFactor, float radiusToScan, ThingDef plantDef)
	{
		float numDesiredPlantsLocally = 0f;
		int numPlants = 0;
		int numPlantsThisDef = 0;
		RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
		{
			numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensityFactor);
			numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.NonStumpPlant).Count;
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

	private void CalculatePlantsWhichCanGrowAt(IntVec3 c, List<ThingDef> outPlants, bool cavePlants, float plantDensityFactor)
	{
		outPlants.Clear();
		if (cavePlants)
		{
			for (int i = 0; i < allCavePlants.Count; i++)
			{
				if (allCavePlants[i].CanEverPlantAt(c, map))
				{
					outPlants.Add(allCavePlants[i]);
				}
			}
			return;
		}
		tmpWildPlants.Clear();
		tmpWildPlants.AddRange(map.BiomeAt(c).AllWildPlants);
		tmpWildPlants.AddRange(MutatorWildPlants);
		foreach (ThingDef tmpWildPlant in tmpWildPlants)
		{
			if (tmpWildPlant.IsDeadPlant || !tmpWildPlant.CanEverPlantAt(c, map))
			{
				continue;
			}
			if (!Mathf.Approximately(tmpWildPlant.plant.wildOrder, map.BiomeAt(c).LowestWildAndCavePlantOrder))
			{
				float num = 7f;
				if (tmpWildPlant.plant.GrowsInClusters)
				{
					num = Math.Max(num, (float)tmpWildPlant.plant.wildClusterRadius * 1.5f);
				}
				if (!EnoughLowerOrderPlantsNearby(c, plantDensityFactor, num, tmpWildPlant))
				{
					continue;
				}
			}
			outPlants.Add(tmpWildPlant);
		}
	}

	private bool EnoughLowerOrderPlantsNearby(IntVec3 c, float plantDensityFactor, float radiusToScan, ThingDef plantDef)
	{
		float num = 0f;
		tmpPlantDefsLowerOrder.Clear();
		List<BiomePlantRecord> wildPlants = map.BiomeAt(c).wildPlants;
		for (int i = 0; i < wildPlants.Count; i++)
		{
			if (wildPlants[i].plant.plant.wildOrder < plantDef.plant.wildOrder)
			{
				num += GetCommonalityPctOfPlant(wildPlants[i].plant);
				tmpPlantDefsLowerOrder.Add(wildPlants[i].plant);
			}
		}
		float numDesiredPlantsLocally = 0f;
		int numPlantsLowerOrder = 0;
		RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), radiusToScan), delegate(Region reg)
		{
			numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensityFactor);
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

	private bool SaturatedAt(IntVec3 c, float plantDensityFactor, bool cavePlants, float wholeMapNumDesiredPlants)
	{
		int num = GenRadial.NumCellsInRadius(20f);
		if (wholeMapNumDesiredPlants * ((float)num / (float)map.Area) <= 4f || (!MapGenUtility.IsMixedBiome(map) && !map.BiomeAt(c).wildPlantsCareAboutLocalFertility))
		{
			return (float)map.listerThings.ThingsInGroup(ThingRequestGroup.NonStumpPlant).Count >= wholeMapNumDesiredPlants;
		}
		float numDesiredPlantsLocally = 0f;
		int numPlants = 0;
		RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => c.InHorDistOf(to.extentsClose.ClosestCellTo(c), 20f), delegate(Region reg)
		{
			numDesiredPlantsLocally += GetDesiredPlantsCountIn(reg, c, plantDensityFactor);
			numPlants += reg.ListerThings.ThingsInGroup(ThingRequestGroup.NonStumpPlant).Count;
			return false;
		});
		return (float)numPlants >= numDesiredPlantsLocally;
	}

	private void CalculateDistancesToNearbyClusters(IntVec3 c)
	{
		nearbyClusters.Clear();
		nearbyClustersList.Clear();
		int num = GenRadial.NumCellsInRadius(map.BiomeAt(c).MaxWildAndCavePlantsClusterRadius * 2);
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
					if (!nearbyClusters.TryGetValue(thing.def, out var value))
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
		if (c.Roofed(map))
		{
			return GoodRoofForCavePlant(c);
		}
		return true;
	}

	private bool GoodRoofForCavePlant(IntVec3 c)
	{
		return c.GetRoof(map)?.isNatural ?? false;
	}

	private float GetCommonalityOfPlant(ThingDef plant)
	{
		CachePlantCommonalitiesIfShould();
		if (plant.plant.cavePlant && !map.Biome.wildPlantsAreCavePlants)
		{
			return plant.plant.cavePlantWeight;
		}
		return cachedPlantCommonalities.GetValueOrDefault(plant, 0f);
	}

	public float GetCommonalityPctOfPlant(ThingDef plant)
	{
		if (!plant.plant.cavePlant || map.Biome.wildPlantsAreCavePlants)
		{
			return GetCommonalityOfPlant(plant) / PlantsCommonalitiesSum;
		}
		return GetCommonalityOfPlant(plant) / CavePlantsCommonalitiesSum;
	}

	public float GetBaseDesiredPlantsCountAt(IntVec3 c)
	{
		float num = map.fertilityGrid.FertilityAt(c);
		if (GoodRoofForCavePlant(c))
		{
			num *= 0.5f;
		}
		if (HaveAnyPlantsWhichIgnoreFertility && c.GetTerrain(map).IsWater && num <= 0f)
		{
			num = 0.1f;
		}
		return num;
	}

	public float GetDesiredPlantsCountAt(IntVec3 forCell, float plantDensityFactor)
	{
		float num = map.fertilityGrid.FertilityAt(forCell);
		if (num <= 0f && HaveAnyPlantsWhichIgnoreFertility)
		{
			num = 1f;
		}
		float num2 = map.BiomeAt(forCell).plantDensity * plantDensityFactor * num;
		return Mathf.Min(GetBaseDesiredPlantsCountAt(forCell) * num2, 1f);
	}

	public float GetDesiredPlantsCountIn(Region reg, IntVec3 forCell, float plantDensityFactor)
	{
		float num = map.fertilityGrid.FertilityAt(forCell);
		if (num <= 0f && HaveAnyPlantsWhichIgnoreFertility)
		{
			num = 1f;
		}
		float num2 = map.BiomeAt(forCell).plantDensity * plantDensityFactor * num;
		return Mathf.Min(reg.GetBaseDesiredPlantsCount() * num2, reg.CellCount);
	}

	public float GetWaterPlantDistanceToShoreWeight(IntVec3 rootCell)
	{
		if (!rootCell.GetTerrain(map).IsWater)
		{
			return 1f;
		}
		float distance = float.MaxValue;
		map.floodFiller.FloodFill(rootCell, (IntVec3 c) => true, delegate(IntVec3 c)
		{
			if (map.terrainGrid.BaseTerrainAt(c).IsWater)
			{
				return false;
			}
			distance = c.DistanceTo(rootCell);
			return true;
		});
		return Mathf.Clamp01(1f - distance / 4f);
	}
}
