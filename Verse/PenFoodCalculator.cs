using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;

namespace Verse;

public class PenFoodCalculator
{
	public class PenAnimalInfo
	{
		public ThingDef animalDef;

		public bool example;

		public int count;

		public float nutritionConsumptionPerDay;

		private string cachedToolTip;

		public int TotalCount => count;

		public float TotalNutritionConsumptionPerDay => nutritionConsumptionPerDay;

		public PenAnimalInfo(ThingDef animalDef)
		{
			this.animalDef = animalDef;
		}

		public string ToolTip(PenFoodCalculator calc)
		{
			if (cachedToolTip == null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int value = Mathf.FloorToInt(calc.NutritionPerDayToday / SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(animalDef));
				stringBuilder.Append("PenFoodTab_AnimalTypeAnimalCapacity".Translate()).Append(": ").Append(value)
					.AppendLine();
				stringBuilder.AppendLine();
				stringBuilder.Append("PenFoodTab_NutritionConsumedPerDay".Translate(NamedArgumentUtility.Named(animalDef, "ANIMALDEF"))).AppendLine(":");
				List<LifeStageAge> lifeStageAges = animalDef.race.lifeStageAges;
				for (int i = 0; i < lifeStageAges.Count; i++)
				{
					LifeStageDef def = lifeStageAges[i].def;
					float value2 = SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(animalDef, def);
					stringBuilder.Append("- ").Append(def.LabelCap).Append(": ")
						.AppendLine(NutritionPerDayToString(value2, withUnits: false));
				}
				cachedToolTip = stringBuilder.ToString();
			}
			return cachedToolTip;
		}
	}

	public class PenFoodItemInfo
	{
		public ThingDef itemDef;

		public int totalCount;

		public float totalNutritionAvailable;

		public PenFoodItemInfo(ThingDef itemDef)
		{
			this.itemDef = itemDef;
		}
	}

	public const ToStringStyle NutritionStringStyle = ToStringStyle.FloatMaxTwo;

	private readonly AnimalPenConnectedDistrictsCalculator connectedDistrictsCalc = new AnimalPenConnectedDistrictsCalculator();

	private readonly AnimalPenBlueprintEnclosureCalculator blueprintEnclosureCalc = new AnimalPenBlueprintEnclosureCalculator();

	private readonly MapPastureNutritionCalculator mapCalc = new MapPastureNutritionCalculator();

	private readonly List<PenAnimalInfo> animals = new List<PenAnimalInfo>();

	private readonly List<PenFoodItemInfo> stockpiled = new List<PenFoodItemInfo>();

	private string cachedNaturalGrowthRateTooltip;

	private string cachedTotalConsumedTooltip;

	private string cachedStockpileTooltip;

	public MapPastureNutritionCalculator.NutritionPerDayPerQuadrum nutritionPerDayPerQuadrum;

	public float sumStockpiledNutritionAvailableNow;

	public int numCells;

	public int numCellsSoil;

	private float sumNutritionPerDayToday;

	private readonly List<PenAnimalInfo> tmpAddedExampleAnimals = new List<PenAnimalInfo>();

	public float NutritionPerDayToday => sumNutritionPerDayToday;

	public List<PenAnimalInfo> ActualAnimalInfos => animals;

	public List<PenFoodItemInfo> AllStockpiledInfos => stockpiled;

	public bool Unenclosed => numCells == 0;

	public float SumNutritionConsumptionPerDay
	{
		get
		{
			float num = 0f;
			foreach (PenAnimalInfo animal in animals)
			{
				num += animal.TotalNutritionConsumptionPerDay;
			}
			return num;
		}
	}

	public PenAnimalInfo GetAnimalInfo(ThingDef animalDef)
	{
		foreach (PenAnimalInfo animal in animals)
		{
			if (animal.animalDef == animalDef)
			{
				return animal;
			}
		}
		PenAnimalInfo penAnimalInfo = new PenAnimalInfo(animalDef);
		animals.Add(penAnimalInfo);
		return penAnimalInfo;
	}

	public PenFoodItemInfo GetStockpiledInfo(ThingDef itemDef)
	{
		foreach (PenFoodItemInfo item in stockpiled)
		{
			if (item.itemDef == itemDef)
			{
				return item;
			}
		}
		PenFoodItemInfo penFoodItemInfo = new PenFoodItemInfo(itemDef);
		stockpiled.Add(penFoodItemInfo);
		return penFoodItemInfo;
	}

	public string NaturalGrowthRateTooltip()
	{
		if (cachedNaturalGrowthRateTooltip == null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PenFoodTab_NaturalNutritionGrowthRateDescription".Translate());
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("PenFoodTab_NaturalNutritionGrowthRateSeasonal".Translate());
			stringBuilder.AppendLine();
			stringBuilder.Append("PenFoodTab_GrowthPerSeason".Translate()).AppendLine(":");
			Vector2 vector = Find.WorldGrid.LongLatOf(mapCalc.map.Tile);
			for (int i = 0; i < 4; i++)
			{
				Quadrum quadrum = (Quadrum)i;
				stringBuilder.Append("- ").Append(quadrum.Label()).Append(" (")
					.Append(quadrum.GetSeason(vector.y).Label())
					.Append("): ");
				stringBuilder.AppendLine(NutritionPerDayToString(nutritionPerDayPerQuadrum.ForQuadrum(quadrum), withUnits: false));
			}
			cachedNaturalGrowthRateTooltip = stringBuilder.ToString();
		}
		return cachedNaturalGrowthRateTooltip;
	}

	public string TotalConsumedToolTop()
	{
		if (cachedTotalConsumedTooltip == null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PenFoodTab_NutritionConsumptionDescription".Translate());
			cachedTotalConsumedTooltip = stringBuilder.ToString();
		}
		return cachedTotalConsumedTooltip;
	}

	public string StockpileToolTip()
	{
		if (cachedStockpileTooltip == null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PenFoodTab_StockpileTotalDescription".Translate());
			stringBuilder.AppendLine();
			foreach (PenFoodItemInfo item in stockpiled)
			{
				stringBuilder.Append("- ").Append(item.itemDef.LabelCap).Append(" x")
					.Append(item.totalCount)
					.Append(": ");
				stringBuilder.AppendLine(NutritionToString(item.totalNutritionAvailable));
			}
			cachedStockpileTooltip = stringBuilder.ToString();
		}
		return cachedStockpileTooltip;
	}

	private void Reset(Map map)
	{
		mapCalc.Reset(map);
		animals.Clear();
		stockpiled.Clear();
		cachedNaturalGrowthRateTooltip = null;
		cachedTotalConsumedTooltip = null;
		cachedStockpileTooltip = null;
		nutritionPerDayPerQuadrum = default(MapPastureNutritionCalculator.NutritionPerDayPerQuadrum);
		sumNutritionPerDayToday = 0f;
		sumStockpiledNutritionAvailableNow = 0f;
		numCells = 0;
		numCellsSoil = 0;
	}

	public List<PenAnimalInfo> ComputeExampleAnimals(List<ThingDef> animalDefs)
	{
		tmpAddedExampleAnimals.Clear();
		foreach (ThingDef animalDef in animalDefs)
		{
			LifeStageAge lifeStageAge = animalDef.race.lifeStageAges.Last();
			PenAnimalInfo penAnimalInfo = new PenAnimalInfo(animalDef);
			penAnimalInfo.example = true;
			penAnimalInfo.nutritionConsumptionPerDay = SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(animalDef, lifeStageAge.def);
			tmpAddedExampleAnimals.Add(penAnimalInfo);
		}
		SortAnimals(tmpAddedExampleAnimals);
		return tmpAddedExampleAnimals;
	}

	public void ResetAndProcessPen(CompAnimalPenMarker marker)
	{
		ResetAndProcessPen(marker.parent.Position, marker.parent.Map, considerBlueprints: false);
	}

	public void ResetAndProcessPen(IntVec3 position, Map map, bool considerBlueprints)
	{
		Reset(map);
		if (map != null)
		{
			if (considerBlueprints)
			{
				ProcessBlueprintPen(position, map);
			}
			else
			{
				ProcessRealPen(position, map);
			}
			SortResults(map);
		}
	}

	private void SortResults(Map map)
	{
		stockpiled.Sort(FoodSorter);
		SortAnimals(animals);
		static int FoodSorter(PenFoodItemInfo a, PenFoodItemInfo b)
		{
			return -1 * a.totalNutritionAvailable.CompareTo(b.totalNutritionAvailable);
		}
	}

	private static void SortAnimals(List<PenAnimalInfo> infos)
	{
		infos.Sort(AnimalSorter);
		static int AnimalSorter(PenAnimalInfo a, PenAnimalInfo b)
		{
			return -1 * a.TotalNutritionConsumptionPerDay.CompareTo(b.TotalNutritionConsumptionPerDay);
		}
	}

	private void ProcessBlueprintPen(IntVec3 markerPos, Map map)
	{
		blueprintEnclosureCalc.VisitPen(markerPos, map);
		if (!blueprintEnclosureCalc.isEnclosed)
		{
			return;
		}
		foreach (IntVec3 item in blueprintEnclosureCalc.cellsFound)
		{
			ProcessCell(item, map);
		}
	}

	private void ProcessRealPen(IntVec3 markerPos, Map map)
	{
		using (ProfilerBlock.Scope("PenFoodCalculator.ProcessRealPen"))
		{
			foreach (District item in connectedDistrictsCalc.CalculateConnectedDistricts(markerPos, map))
			{
				foreach (Region region in item.Regions)
				{
					ProcessRegion(region);
				}
			}
			connectedDistrictsCalc.Reset();
		}
	}

	private void ProcessRegion(Region region)
	{
		using (ProfilerBlock.Scope("PenFoodCalculator.ProcessRegion"))
		{
			foreach (IntVec3 cell in region.Cells)
			{
				ProcessCell(cell, region.Map);
			}
		}
	}

	private void ProcessCell(IntVec3 c, Map map)
	{
		using (ProfilerBlock.Scope("ProcessTerrain"))
		{
			ProcessTerrain(c, map);
		}
		foreach (Thing thing in c.GetThingList(map))
		{
			if (thing is Pawn { IsAnimal: not false } pawn)
			{
				ProcessAnimal(pawn);
			}
			else if (thing.def.category == ThingCategory.Item && thing.IngestibleNow && MapPlantGrowthRateCalculator.IsEdibleByPastureAnimals(thing.def))
			{
				ProcessStockpiledFood(thing);
			}
		}
	}

	private void ProcessTerrain(IntVec3 c, Map map)
	{
		numCells++;
		if (c.GetEdifice(map) == null)
		{
			TerrainDef terrain = c.GetTerrain(map);
			if (terrain.IsSoil)
			{
				numCellsSoil++;
			}
			MapPastureNutritionCalculator.NutritionPerDayPerQuadrum other = mapCalc.CalculateAverageNutritionPerDay(terrain);
			nutritionPerDayPerQuadrum.AddFrom(other);
			sumNutritionPerDayToday += mapCalc.GetAverageNutritionPerDayToday(terrain);
		}
	}

	private void ProcessStockpiledFood(Thing thing)
	{
		PenFoodItemInfo stockpiledInfo = GetStockpiledInfo(thing.def);
		float num = thing.GetStatValue(StatDefOf.Nutrition) * (float)thing.stackCount;
		stockpiledInfo.totalCount += thing.stackCount;
		stockpiledInfo.totalNutritionAvailable += num;
		sumStockpiledNutritionAvailableNow += num;
	}

	private void ProcessAnimal(Pawn animal)
	{
		if (MapPlantGrowthRateCalculator.IsPastureAnimal(animal.def) && animal.Spawned)
		{
			PenAnimalInfo animalInfo = GetAnimalInfo(animal.def);
			animalInfo.count++;
			animalInfo.nutritionConsumptionPerDay += SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(animal);
		}
	}

	public static string NutritionToString(float value, bool withUnits = true)
	{
		string text = value.ToStringByStyle(ToStringStyle.FloatMaxTwo);
		if (withUnits)
		{
			return text + " " + "PenFoodTab_Nutrition_Unit".Translate();
		}
		return text;
	}

	public static string NutritionPerDayToString(float value, bool withUnits = true)
	{
		string text = value.ToStringByStyle(ToStringStyle.FloatMaxTwo);
		if (withUnits)
		{
			return text + " " + "PenFoodTab_NutritionPerDay_Unit".Translate();
		}
		return text;
	}

	public float CapacityOf(Quadrum q, ThingDef animal)
	{
		return nutritionPerDayPerQuadrum.ForQuadrum(q) / SimplifiedPastureNutritionSimulator.NutritionConsumedPerDay(animal);
	}

	public Quadrum GetSummerOrBestQuadrum()
	{
		Vector2 location = Find.WorldGrid.LongLatOf(mapCalc.map.Tile);
		Quadrum? quadrum = null;
		float num = 0f;
		foreach (Quadrum quadrum2 in QuadrumUtility.Quadrums)
		{
			if (quadrum2.GetSeason(location) == Season.Summer)
			{
				return quadrum2;
			}
			float num2 = nutritionPerDayPerQuadrum.ForQuadrum(quadrum2);
			if (!quadrum.HasValue || num2 > num)
			{
				quadrum = quadrum2;
				num = num2;
			}
		}
		return quadrum.Value;
	}

	public string PenSizeDescription()
	{
		if (Unenclosed)
		{
			return "PenSizeDesc_Unenclosed".Translate();
		}
		if (numCellsSoil < 50)
		{
			return "PenSizeDesc_VerySmall".Translate();
		}
		if (numCellsSoil < 100)
		{
			return "PenSizeDesc_Small".Translate();
		}
		if (numCellsSoil < 400)
		{
			return "PenSizeDesc_Medium".Translate();
		}
		return "PenSizeDesc_Large".Translate();
	}
}
