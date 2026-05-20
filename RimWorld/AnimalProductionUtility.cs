using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class AnimalProductionUtility
{
	public static IEnumerable<StatDrawEntry> AnimalProductionStats(ThingDef d)
	{
		float num = GestationDaysLitter(d);
		if (num > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_GestationTime".Translate(), "PeriodDays".Translate(num.ToString("#.##")), "Stat_Animal_GestationTimeDesc".Translate(), 10000);
		}
		IntRange intRange = OffspringRange(d);
		if (intRange != IntRange.One)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_LitterSize".Translate(), intRange.ToString(), "Stat_Animal_LitterSizeDesc".Translate(), 9990);
		}
		float num2 = DaysToAdulthood(d);
		if (num2 > 0f)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_GrowthTime".Translate(), "PeriodDays".Translate(num2), "Stat_Animal_GrowthTimeDesc".Translate(), 9980);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MeatPerDayDuringGrowth".Translate(), MeatPerDayDuringGrowth(d).ToString("#.##"), "Stat_Animal_MeatPerDayDuringGrowthDesc".Translate(), 9960);
		}
		yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_AdultMeatAmount".Translate(), AdultMeatAmount(d).ToString("F0"), "Stat_Animal_AdultMeatAmountDesc".Translate(), 9970, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(d.race.meatDef)));
		if (Herbivore(d))
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_GrassToMaintain".Translate(), GrassToMaintain(d).ToString("#.##"), "Stat_Animal_GrassToMaintainDesc".Translate(), 9950);
		}
		CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties != null)
		{
			ThingDef thingDef = compProperties.eggUnfertilizedDef ?? compProperties.eggFertilizedDef;
			if (thingDef != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggType".Translate(), thingDef.LabelCap, "Stat_Animal_EggTypeDesc".Translate(), 9940, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(thingDef)));
			}
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggsPerYear".Translate(), EggsPerYear(d).ToStringByStyle(ToStringStyle.FloatMaxTwo), "Stat_Animal_EggsPerYearDesc".Translate(), 9930);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggNutrition".Translate(), EggNutrition(d).ToStringByStyle(ToStringStyle.FloatMaxTwo), "Stat_Animal_EggNutritionDesc".Translate(), 9920);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggNutritionYearly".Translate(), EggNutritionPerYear(d).ToStringByStyle(ToStringStyle.FloatMaxTwo), "Stat_Animal_EggNutritionYearlyDesc".Translate(), 9910);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggMarketValue".Translate(), EggMarketValue(d).ToStringMoney(), "Stat_Animal_EggMarketValueDesc".Translate(), 9900);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_EggMarketValueYearly".Translate(), EggMarketValuePerYear(d).ToStringMoney(), "Stat_Animal_EggMarketValueYearlyDesc".Translate(), 9890);
		}
		CompProperties_Milkable milkable = d.GetCompProperties<CompProperties_Milkable>();
		if (milkable != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkType".Translate(), milkable.milkDef.LabelCap, "Stat_Animal_MilkTypeDesc".Translate(), 9880, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(milkable.milkDef)));
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkAmount".Translate(), milkable.milkAmount.ToString(), "Stat_Animal_MilkAmountDesc".Translate(), 9870);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkGrowthTime".Translate(), "PeriodDays".Translate(milkable.milkIntervalDays), "Stat_Animal_MilkGrowthTimeDesc".Translate(), 9860);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkPerYear".Translate(), MilkPerYear(d).ToString("F0"), "Stat_Animal_MilkPerYearDesc".Translate(), 9850);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkValue".Translate(), MilkMarketValue(d).ToStringMoney(), "Stat_Animal_MilkValueDesc".Translate(), 9840);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_MilkValuePerYear".Translate(), MilkMarketValuePerYear(d).ToStringMoney(), "Stat_Animal_MilkValuePerYearDesc".Translate(), 9830);
		}
		CompProperties_Shearable shearable = d.GetCompProperties<CompProperties_Shearable>();
		if (shearable != null)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolType".Translate(), shearable.woolDef.LabelCap, "Stat_Animal_WoolTypeDesc".Translate(), 9820, null, Gen.YieldSingle(new Dialog_InfoCard.Hyperlink(shearable.woolDef)));
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolAmount".Translate(), shearable.woolAmount.ToString(), "Stat_Animal_WoolAmountDesc".Translate(), 9810);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolGrowthTime".Translate(), "PeriodDays".Translate(shearable.shearIntervalDays), "Stat_Animal_WoolGrowthTimeDesc".Translate(), 9800);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolPerYear".Translate(), WoolPerYear(d).ToString("F0"), "Stat_Animal_WoolPerYearDesc".Translate(), 9790);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolValue".Translate(), WoolMarketValue(d).ToStringMoney(), "Stat_Animal_WoolValueDesc".Translate(), 9780);
			yield return new StatDrawEntry(StatCategoryDefOf.AnimalProductivity, "Stat_Animal_WoolValuePerYear".Translate(), WoolMarketValuePerYear(d).ToStringMoney(), "Stat_Animal_WoolValuePerYearDesc".Translate(), 9770);
		}
	}

	public static float AdultMeatAmount(ThingDef d)
	{
		return d.GetStatValueAbstract(StatDefOf.MeatAmount);
	}

	public static float AdultLeatherAmount(ThingDef d)
	{
		return d.GetStatValueAbstract(StatDefOf.LeatherAmount);
	}

	public static bool Herbivore(ThingDef d)
	{
		return (d.race.foodType & FoodTypeFlags.Plant) != 0;
	}

	public static float NutritionToGestate(ThingDef d)
	{
		LifeStageAge lifeStageAge = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1];
		return 0f + GestationDaysEach(d) * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
	}

	public static float NutritionToAdulthood(ThingDef d)
	{
		float num = 0f;
		num += NutritionToGestate(d);
		for (int i = 1; i < d.race.lifeStageAges.Count; i++)
		{
			LifeStageAge lifeStageAge = d.race.lifeStageAges[i];
			float num2 = (lifeStageAge.minAge - d.race.lifeStageAges[i - 1].minAge) * 60f;
			num += num2 * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
		}
		return num;
	}

	public static float GestationDaysEach(ThingDef d)
	{
		if (d.HasComp(typeof(CompEggLayer)))
		{
			CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
			return compProperties.eggLayIntervalDays / compProperties.eggCountRange.Average;
		}
		return d.race.gestationPeriodDays / ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f);
	}

	public static float GestationDaysLitter(ThingDef d)
	{
		if (d.HasComp(typeof(CompEggLayer)))
		{
			return d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays;
		}
		return d.race.gestationPeriodDays;
	}

	public static IntRange OffspringRange(ThingDef d)
	{
		CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties != null)
		{
			return compProperties.eggCountRange;
		}
		if (d.race.litterSizeCurve != null)
		{
			int min = Mathf.Max(Mathf.RoundToInt(d.race.litterSizeCurve.First().x), 1);
			int max = Mathf.Max(Mathf.RoundToInt(d.race.litterSizeCurve.Last().x), 1);
			return new IntRange(min, max);
		}
		return new IntRange(1, 1);
	}

	public static float GrassNutritionPerDay()
	{
		ThingDef plant_Grass = ThingDefOf.Plant_Grass;
		return plant_Grass.GetStatValueAbstract(StatDefOf.Nutrition) / (plant_Grass.plant.growDays / 0.5f);
	}

	public static float GrassToMaintain(ThingDef d)
	{
		if (!Herbivore(d))
		{
			return -1f;
		}
		return 2.6666667E-05f * d.race.baseHungerRate * 60000f / GrassNutritionPerDay();
	}

	public static float EggsPerYear(ThingDef d)
	{
		CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties == null)
		{
			return 0f;
		}
		return 60f / compProperties.eggLayIntervalDays * compProperties.eggCountRange.Average;
	}

	public static float EggNutritionPerYear(ThingDef d)
	{
		return EggsPerYear(d) * EggNutrition(d);
	}

	public static float EggNutrition(ThingDef d)
	{
		CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties == null)
		{
			return -1f;
		}
		return (compProperties.eggUnfertilizedDef ?? compProperties.eggFertilizedDef).GetStatValueAbstract(StatDefOf.Nutrition);
	}

	public static float EggMarketValue(ThingDef d)
	{
		CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
		if (compProperties == null)
		{
			return -1f;
		}
		return (compProperties.eggUnfertilizedDef ?? compProperties.eggFertilizedDef).BaseMarketValue;
	}

	public static float EggMarketValuePerYear(ThingDef d)
	{
		return EggsPerYear(d) * EggMarketValue(d);
	}

	public static float DaysToAdulthood(ThingDef d)
	{
		return d.race.lifeStageAges.Last().minAge * 60f;
	}

	public static float MeatPerDayDuringGrowth(ThingDef d)
	{
		float num = AdultMeatAmount(d);
		float num2 = DaysToAdulthood(d);
		return num / num2;
	}

	public static float MilkPerYear(ThingDef d)
	{
		CompProperties_Milkable compProperties = d.GetCompProperties<CompProperties_Milkable>();
		if (compProperties == null)
		{
			return 0f;
		}
		return 60f / (float)compProperties.milkIntervalDays * (float)compProperties.milkAmount;
	}

	public static float MilkMarketValue(ThingDef d)
	{
		return d.GetCompProperties<CompProperties_Milkable>()?.milkDef.BaseMarketValue ?? (-1f);
	}

	public static float MilkMarketValuePerYear(ThingDef d)
	{
		return MilkPerYear(d) * MilkMarketValue(d);
	}

	public static float MilkNutrition(ThingDef d)
	{
		CompProperties_Milkable compProperties = d.GetCompProperties<CompProperties_Milkable>();
		if (compProperties == null)
		{
			return 0f;
		}
		if (!compProperties.milkDef.IsIngestible)
		{
			return 0f;
		}
		return compProperties.milkDef.ingestible.CachedNutrition;
	}

	public static float MilkNutritionPerYear(ThingDef d)
	{
		return MilkPerYear(d) * MilkNutrition(d);
	}

	public static float WoolPerYear(ThingDef d)
	{
		CompProperties_Shearable compProperties = d.GetCompProperties<CompProperties_Shearable>();
		if (compProperties == null)
		{
			return 0f;
		}
		return 60f / (float)compProperties.shearIntervalDays * (float)compProperties.woolAmount;
	}

	public static float WoolMarketValue(ThingDef d)
	{
		return d.GetCompProperties<CompProperties_Shearable>()?.woolDef.BaseMarketValue ?? 0f;
	}

	public static float WoolMarketValuePerYear(ThingDef d)
	{
		return WoolPerYear(d) * WoolMarketValue(d);
	}
}
