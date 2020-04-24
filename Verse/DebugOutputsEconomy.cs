using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class DebugOutputsEconomy
	{
		[DebugOutput("Economy", false)]
		public static void RecipeSkills()
		{
			DebugTables.MakeTablesDialog(DefDatabase<RecipeDef>.AllDefs, new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName), new TableDataGetter<RecipeDef>("workSkill", (RecipeDef d) => (d.workSkill != null) ? d.workSkill.defName : ""), new TableDataGetter<RecipeDef>("workSpeedStat", (RecipeDef d) => (d.workSpeedStat != null) ? d.workSpeedStat.defName : ""), new TableDataGetter<RecipeDef>("workSpeedStat's skillNeedFactors", (RecipeDef d) => (d.workSpeedStat != null) ? ((!d.workSpeedStat.skillNeedFactors.NullOrEmpty()) ? d.workSpeedStat.skillNeedFactors.Select((SkillNeed fac) => fac.skill.defName).ToCommaList() : "") : ""), new TableDataGetter<RecipeDef>("workSkillLearnFactor", (RecipeDef d) => d.workSkillLearnFactor));
		}

		[DebugOutput("Economy", false)]
		public static void Drugs()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsWithinCategory(ThingCategoryDefOf.Medicine) || d.IsWithinCategory(ThingCategoryDefOf.Drugs)), new TableDataGetter<ThingDef>("name", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("ingredients", (ThingDef d) => CostListString(d, divideByVolume: true, starIfOnlyBuyable: true)), new TableDataGetter<ThingDef>("work amount", (ThingDef d) => WorkToProduceBest(d).ToString("F0")), new TableDataGetter<ThingDef>("real ingredient cost", (ThingDef d) => realIngredientCost(d).ToString("F1")), new TableDataGetter<ThingDef>("real sell price", (ThingDef d) => realSellPrice(d).ToString("F1")), new TableDataGetter<ThingDef>("real profit per item", (ThingDef d) => (realSellPrice(d) - realIngredientCost(d)).ToString("F1")), new TableDataGetter<ThingDef>("real profit per day's work", (ThingDef d) => ((realSellPrice(d) - realIngredientCost(d)) / WorkToProduceBest(d) * 30000f).ToString("F1")), new TableDataGetter<ThingDef>("real buy price", (ThingDef d) => realBuyPrice(d).ToString("F1")));
			float realBuyPrice(ThingDef d)
			{
				return d.BaseMarketValue * 1.4f;
			}
			float realIngredientCost(ThingDef d)
			{
				return CostToMake(d, real: true);
			}
			float realSellPrice(ThingDef d)
			{
				return d.BaseMarketValue * 0.6f;
			}
		}

		[DebugOutput("Economy", false)]
		public static void Wool()
		{
			DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn && d.race.IsFlesh && d.GetCompProperties<CompProperties_Shearable>() != null), new TableDataGetter<ThingDef>("animal", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("woolDef", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.defName), new TableDataGetter<ThingDef>("woolAmount", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolAmount.ToString()), new TableDataGetter<ThingDef>("woolValue", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("shear interval", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().shearIntervalDays.ToString("F1")), new TableDataGetter<ThingDef>("value per year", delegate(ThingDef d)
			{
				CompProperties_Shearable compProperties = d.GetCompProperties<CompProperties_Shearable>();
				return (compProperties.woolDef.BaseMarketValue * (float)compProperties.woolAmount * (60f / (float)compProperties.shearIntervalDays)).ToString("F0");
			}));
		}

		[DebugOutput("Economy", false)]
		public static void AnimalGrowth()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Pawn && d.race.IsFlesh
				orderby bestMeatPerInput(d) descending
				select d, new TableDataGetter<ThingDef>("", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("hungerRate", (ThingDef d) => d.race.baseHungerRate.ToString("F2")), new TableDataGetter<ThingDef>("gestDaysEach", (ThingDef d) => gestDaysEach(d).ToString("F2")), new TableDataGetter<ThingDef>("herbiv", (ThingDef d) => ((d.race.foodType & FoodTypeFlags.Plant) == 0) ? "" : "Y"), new TableDataGetter<ThingDef>("|", (ThingDef d) => "|"), new TableDataGetter<ThingDef>("bodySize", (ThingDef d) => d.race.baseBodySize.ToString("F2")), new TableDataGetter<ThingDef>("age Adult", (ThingDef d) => d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge.ToString("F2")), new TableDataGetter<ThingDef>("nutrition to adulthood", (ThingDef d) => nutritionToAdulthood(d).ToString("F2")), new TableDataGetter<ThingDef>("adult meat-nut", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.MeatAmount) * 0.05f).ToString("F2")), new TableDataGetter<ThingDef>("adult meat-nut / input-nut", (ThingDef d) => adultMeatNutPerInput(d).ToString("F3")), new TableDataGetter<ThingDef>("|", (ThingDef d) => "|"), new TableDataGetter<ThingDef>("baby size", (ThingDef d) => (d.race.lifeStageAges[0].def.bodySizeFactor * d.race.baseBodySize).ToString("F2")), new TableDataGetter<ThingDef>("nutrition to gestate", (ThingDef d) => nutritionToGestate(d).ToString("F2")), new TableDataGetter<ThingDef>("egg nut", (ThingDef d) => eggNut(d)), new TableDataGetter<ThingDef>("baby meat-nut", (ThingDef d) => babyMeatNut(d).ToString("F2")), new TableDataGetter<ThingDef>("baby meat-nut / input-nut", (ThingDef d) => babyMeatNutPerInput(d).ToString("F2")), new TableDataGetter<ThingDef>("baby wins", (ThingDef d) => (!(babyMeatNutPerInput(d) > adultMeatNutPerInput(d))) ? "" : "B"));
			float adultMeatNutPerInput(ThingDef d)
			{
				return d.GetStatValueAbstract(StatDefOf.MeatAmount) * 0.05f / nutritionToAdulthood(d);
			}
			float babyMeatNut(ThingDef d)
			{
				LifeStageAge lifeStageAge3 = d.race.lifeStageAges[0];
				return d.GetStatValueAbstract(StatDefOf.MeatAmount) * 0.05f * lifeStageAge3.def.bodySizeFactor;
			}
			float babyMeatNutPerInput(ThingDef d)
			{
				return babyMeatNut(d) / nutritionToGestate(d);
			}
			float bestMeatPerInput(ThingDef d)
			{
				float a = babyMeatNutPerInput(d);
				float b = adultMeatNutPerInput(d);
				return Mathf.Max(a, b);
			}
			string eggNut(ThingDef d)
			{
				CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
				if (compProperties == null)
				{
					return "";
				}
				return compProperties.eggFertilizedDef.GetStatValueAbstract(StatDefOf.Nutrition).ToString("F2");
			}
			float gestDaysEach(ThingDef d)
			{
				return GestationDaysEach(d);
			}
			float nutritionToAdulthood(ThingDef d)
			{
				float num = 0f;
				num += nutritionToGestate(d);
				for (int i = 1; i < d.race.lifeStageAges.Count; i++)
				{
					LifeStageAge lifeStageAge = d.race.lifeStageAges[i];
					float num2 = (lifeStageAge.minAge - d.race.lifeStageAges[i - 1].minAge) * 60f;
					num += num2 * lifeStageAge.def.hungerRateFactor * d.race.baseHungerRate;
				}
				return num;
			}
			float nutritionToGestate(ThingDef d)
			{
				LifeStageAge lifeStageAge2 = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1];
				return 0f + gestDaysEach(d) * lifeStageAge2.def.hungerRateFactor * d.race.baseHungerRate;
			}
		}

		[DebugOutput("Economy", false)]
		public static void AnimalBreeding()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Pawn && d.race.IsFlesh
				orderby GestationDaysEach(d) descending
				select d, new TableDataGetter<ThingDef>("", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("gestDaysEach", (ThingDef d) => GestationDaysEach(d).ToString("F2")), new TableDataGetter<ThingDef>("avgOffspring", (ThingDef d) => (!d.HasComp(typeof(CompEggLayer))) ? ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f).ToString("F2") : d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average.ToString("F2")), new TableDataGetter<ThingDef>("gestDaysRaw", (ThingDef d) => (!d.HasComp(typeof(CompEggLayer))) ? d.race.gestationPeriodDays.ToString("F1") : d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays.ToString("F1")), new TableDataGetter<ThingDef>("growth per 30d", delegate(ThingDef d)
			{
				float f2 = 1f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average : ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f));
				float num2 = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays : d.race.gestationPeriodDays);
				float p2 = 30f / num2;
				return Mathf.Pow(f2, p2).ToString("F2");
			}), new TableDataGetter<ThingDef>("growth per 60d", delegate(ThingDef d)
			{
				float f = 1f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average : ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f));
				float num = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays : d.race.gestationPeriodDays);
				float p = 60f / num;
				return Mathf.Pow(f, p).ToString("F2");
			}));
		}

		private static float GestationDaysEach(ThingDef d)
		{
			if (d.HasComp(typeof(CompEggLayer)))
			{
				CompProperties_EggLayer compProperties = d.GetCompProperties<CompProperties_EggLayer>();
				return compProperties.eggLayIntervalDays / compProperties.eggCountRange.Average;
			}
			return d.race.gestationPeriodDays / ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f);
		}

		[DebugOutput("Economy", false)]
		public static void BuildingSkills()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
				where d.BuildableByPlayer
				select d, new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("construction skill prerequisite", (BuildableDef d) => d.constructionSkillPrerequisite), new TableDataGetter<BuildableDef>("artistic skill prerequisite", (BuildableDef d) => d.artisticSkillPrerequisite));
		}

		[DebugOutput("Economy", false)]
		public static void Crops()
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where d.category == ThingCategory.Plant && d.plant.Harvestable && d.plant.Sowable
				orderby d.plant.IsTree
				select d, new TableDataGetter<ThingDef>("plant", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("product", (ThingDef d) => d.plant.harvestedThingDef.defName), new TableDataGetter<ThingDef>("grow time", (ThingDef d) => d.plant.growDays.ToString("F1")), new TableDataGetter<ThingDef>("work", (ThingDef d) => (d.plant.sowWork + d.plant.harvestWork).ToString("F0")), new TableDataGetter<ThingDef>("harvestCount", (ThingDef d) => d.plant.harvestYield.ToString("F1")), new TableDataGetter<ThingDef>("work-cost per cycle", (ThingDef d) => workCost(d).ToString("F2")), new TableDataGetter<ThingDef>("work-cost per harvestCount", (ThingDef d) => (workCost(d) / d.plant.harvestYield).ToString("F2")), new TableDataGetter<ThingDef>("value each", (ThingDef d) => d.plant.harvestedThingDef.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("harvestValueTotal", (ThingDef d) => (d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue).ToString("F2")), new TableDataGetter<ThingDef>("profit per growDay", (ThingDef d) => ((d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue - workCost(d)) / d.plant.growDays).ToString("F2")), new TableDataGetter<ThingDef>("nutrition per growDay", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? "" : (d.plant.harvestYield * d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition) / d.plant.growDays).ToString("F2")), new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? "" : d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition).ToString("F2")), new TableDataGetter<ThingDef>("fertMin", (ThingDef d) => d.plant.fertilityMin.ToStringPercent()), new TableDataGetter<ThingDef>("fertSensitivity", (ThingDef d) => d.plant.fertilitySensitivity.ToStringPercent()));
			float workCost(ThingDef d)
			{
				return 1.1f + d.plant.growDays * 1f + (d.plant.sowWork + d.plant.harvestWork) * 0.00612f;
			}
		}

		[DebugOutput("Economy", false)]
		public static void ItemAndBuildingAcquisition()
		{
			Func<ThingDef, string> calculatedMarketValue = delegate(ThingDef d)
			{
				if (!Producible(d))
				{
					return "not producible";
				}
				if (!d.StatBaseDefined(StatDefOf.MarketValue))
				{
					return "used";
				}
				string text = StatWorker_MarketValue.CalculatedBaseMarketValue(d, null).ToString("F1");
				return (StatWorker_MarketValue.CalculableRecipe(d) != null) ? (text + " (recipe)") : text;
			};
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.01f) || (d.category == ThingCategory.Building && (d.BuildableByPlayer || d.Minifiable))
				orderby d.BaseMarketValue
				select d, new TableDataGetter<ThingDef>("cat.", (ThingDef d) => d.category.ToString().Substring(0, 1).CapitalizeFirst()), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("mobile", (ThingDef d) => (d.category == ThingCategory.Item || d.Minifiable).ToStringCheckBlank()), new TableDataGetter<ThingDef>("base\nmarket value", (ThingDef d) => d.BaseMarketValue.ToString("F1")), new TableDataGetter<ThingDef>("calculated\nmarket value", (ThingDef d) => calculatedMarketValue(d)), new TableDataGetter<ThingDef>("cost to make", (ThingDef d) => CostToMakeString(d)), new TableDataGetter<ThingDef>("work to produce", (ThingDef d) => (!(WorkToProduceBest(d) > 0f)) ? "-" : WorkToProduceBest(d).ToString("F1")), new TableDataGetter<ThingDef>("profit", (ThingDef d) => (d.BaseMarketValue - CostToMake(d)).ToString("F1")), new TableDataGetter<ThingDef>("profit\nrate", (ThingDef d) => (d.recipeMaker == null) ? "-" : ((d.BaseMarketValue - CostToMake(d)) / WorkToProduceBest(d) * 10000f).ToString("F0")), new TableDataGetter<ThingDef>("market value\ndefined", (ThingDef d) => d.statBases.Any((StatModifier st) => st.stat == StatDefOf.MarketValue).ToStringCheckBlank()), new TableDataGetter<ThingDef>("producible", (ThingDef d) => Producible(d).ToStringCheckBlank()), new TableDataGetter<ThingDef>("thing set\nmaker tags", (ThingDef d) => (!d.thingSetMakerTags.NullOrEmpty()) ? d.thingSetMakerTags.ToCommaList() : ""), new TableDataGetter<ThingDef>("made\nfrom\nstuff", (ThingDef d) => d.MadeFromStuff.ToStringCheckBlank()), new TableDataGetter<ThingDef>("cost list", (ThingDef d) => CostListString(d, divideByVolume: false, starIfOnlyBuyable: false)), new TableDataGetter<ThingDef>("recipes", (ThingDef d) => recipes(d)), new TableDataGetter<ThingDef>("work amount\nsources", (ThingDef d) => workAmountSources(d)));
			string recipes(ThingDef d)
			{
				List<string> list2 = new List<string>();
				foreach (RecipeDef allDef in DefDatabase<RecipeDef>.AllDefs)
				{
					if (!allDef.products.NullOrEmpty())
					{
						for (int j = 0; j < allDef.products.Count; j++)
						{
							if (allDef.products[j].thingDef == d)
							{
								list2.Add(allDef.defName);
							}
						}
					}
				}
				if (list2.Count == 0)
				{
					return "";
				}
				return list2.ToCommaList();
			}
			string workAmountSources(ThingDef d)
			{
				List<string> list = new List<string>();
				if (d.StatBaseDefined(StatDefOf.WorkToMake))
				{
					list.Add("WorkToMake(" + d.GetStatValueAbstract(StatDefOf.WorkToMake) + ")");
				}
				if (d.StatBaseDefined(StatDefOf.WorkToBuild))
				{
					list.Add("WorkToBuild(" + d.GetStatValueAbstract(StatDefOf.WorkToBuild) + ")");
				}
				foreach (RecipeDef allDef2 in DefDatabase<RecipeDef>.AllDefs)
				{
					if (allDef2.workAmount > 0f && !allDef2.products.NullOrEmpty())
					{
						for (int i = 0; i < allDef2.products.Count; i++)
						{
							if (allDef2.products[i].thingDef == d)
							{
								list.Add("RecipeDef-" + allDef2.defName + "(" + allDef2.workAmount + ")");
							}
						}
					}
				}
				if (list.Count == 0)
				{
					return "";
				}
				return list.ToCommaList();
			}
		}

		[DebugOutput("Economy", false)]
		public static void ItemAccessibility()
		{
			DebugTables.MakeTablesDialog(ThingSetMakerUtility.allGeneratableItems.OrderBy((ThingDef x) => x.defName), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("1", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1, Find.CurrentMap)) ? "" : "✓"), new TableDataGetter<ThingDef>("10", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10, Find.CurrentMap)) ? "" : "✓"), new TableDataGetter<ThingDef>("100", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 100, Find.CurrentMap)) ? "" : "✓"), new TableDataGetter<ThingDef>("1000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 1000, Find.CurrentMap)) ? "" : "✓"), new TableDataGetter<ThingDef>("10000", (ThingDef d) => (!PlayerItemAccessibilityUtility.PossiblyAccessible(d, 10000, Find.CurrentMap)) ? "" : "✓"));
		}

		[DebugOutput("Economy", false)]
		public static void ThingSetMakerTags()
		{
			List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>
			{
				new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName),
				new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToString("F1"))
			};
			foreach (string uniqueTag in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.thingSetMakerTags != null).SelectMany((ThingDef d) => d.thingSetMakerTags).Distinct())
			{
				list.Add(new TableDataGetter<ThingDef>(uniqueTag, (ThingDef d) => (d.thingSetMakerTags != null && d.thingSetMakerTags.Contains(uniqueTag)).ToStringCheckBlank()));
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
				where (d.category == ThingCategory.Item && d.BaseMarketValue > 0.01f) || (d.category == ThingCategory.Building && d.Minifiable)
				orderby d.BaseMarketValue
				select d, list.ToArray());
			string text = "";
			string[] array = new string[3]
			{
				"RewardStandardHighFreq",
				"RewardStandardMidFreq",
				"RewardStandardLowFreq"
			};
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.thingSetMakerTags != null)
				{
					int num = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (allDef.thingSetMakerTags.Contains(array[i]))
						{
							num++;
						}
					}
					if (num > 1)
					{
						text = text + allDef.defName + ": " + num + " reward tags\n";
					}
				}
			}
			if (text.Length > 0)
			{
				Log.Warning(text);
			}
		}

		[DebugOutput("Economy", false)]
		public static void ThingSmeltProducts()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				Thing thing = ThingMaker.MakeThing(allDef, GenStuff.DefaultStuffFor(allDef));
				if (thing.SmeltProducts(1f).Any())
				{
					stringBuilder.Append(thing.LabelCap + ": ");
					foreach (Thing item in thing.SmeltProducts(1f))
					{
						stringBuilder.Append(" " + item.Label);
					}
					stringBuilder.AppendLine();
				}
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput("Economy", false)]
		public static void Recipes()
		{
			DebugTables.MakeTablesDialog(DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef d) => !d.products.NullOrEmpty() && !d.ingredients.NullOrEmpty()), new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName), new TableDataGetter<RecipeDef>("work /w carry", (RecipeDef d) => TrueWorkWithCarryTime(d).ToString("F0")), new TableDataGetter<RecipeDef>("work seconds", (RecipeDef d) => (TrueWorkWithCarryTime(d) / 60f).ToString("F0")), new TableDataGetter<RecipeDef>("cheapest products value", (RecipeDef d) => CheapestProductsValue(d).ToString("F1")), new TableDataGetter<RecipeDef>("cheapest ingredients value", (RecipeDef d) => CheapestIngredientValue(d).ToString("F1")), new TableDataGetter<RecipeDef>("work value", (RecipeDef d) => WorkValueEstimate(d).ToString("F1")), new TableDataGetter<RecipeDef>("profit raw", (RecipeDef d) => (CheapestProductsValue(d) - CheapestIngredientValue(d)).ToString("F1")), new TableDataGetter<RecipeDef>("profit with work", (RecipeDef d) => (CheapestProductsValue(d) - WorkValueEstimate(d) - CheapestIngredientValue(d)).ToString("F1")), new TableDataGetter<RecipeDef>("profit per work day", (RecipeDef d) => ((CheapestProductsValue(d) - CheapestIngredientValue(d)) * 60000f / TrueWorkWithCarryTime(d)).ToString("F0")), new TableDataGetter<RecipeDef>("min skill", (RecipeDef d) => (!d.skillRequirements.NullOrEmpty()) ? d.skillRequirements[0].Summary : ""), new TableDataGetter<RecipeDef>("cheapest stuff", (RecipeDef d) => (CheapestNonDerpStuff(d) == null) ? "" : CheapestNonDerpStuff(d).defName), new TableDataGetter<RecipeDef>("cheapest ingredients", (RecipeDef d) => (from pa in CheapestIngredients(d)
				select pa.First.defName + " x" + pa.Second).ToCommaList()));
		}

		[DebugOutput("Economy", false)]
		public static void Floors()
		{
			DebugTables.MakeTablesDialog(DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef d) => d.designationCategory == DesignationCategoryDefOf.Floors || d == TerrainDefOf.Soil).Concat(TerrainDefGenerator_Stone.ImpliedTerrainDefs()), new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName), new TableDataGetter<TerrainDef>("stuff cost", (TerrainDef d) => d.costList.NullOrEmpty() ? "" : d.costList.First().Label), new TableDataGetter<TerrainDef>("work to build", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild)), new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty)), new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness)));
		}

		private static bool Producible(BuildableDef b)
		{
			ThingDef d = b as ThingDef;
			TerrainDef terrainDef = b as TerrainDef;
			if (d != null)
			{
				if (DefDatabase<RecipeDef>.AllDefs.Any((RecipeDef r) => r.products.Any((ThingDefCountClass pr) => pr.thingDef == d)))
				{
					return true;
				}
				if (d.category == ThingCategory.Building && d.BuildableByPlayer)
				{
					return true;
				}
			}
			else if (terrainDef != null)
			{
				return terrainDef.BuildableByPlayer;
			}
			return false;
		}

		public static string CostListString(BuildableDef d, bool divideByVolume, bool starIfOnlyBuyable)
		{
			if (!Producible(d))
			{
				return "";
			}
			List<string> list = new List<string>();
			if (d.costList != null)
			{
				foreach (ThingDefCountClass cost in d.costList)
				{
					float num = cost.count;
					if (divideByVolume)
					{
						num /= cost.thingDef.VolumePerUnit;
					}
					string text = cost.thingDef + " x" + num;
					if (starIfOnlyBuyable && RequiresBuying(cost.thingDef))
					{
						text += "*";
					}
					list.Add(text);
				}
			}
			if (d.MadeFromStuff)
			{
				list.Add("stuff x" + d.costStuffCount);
			}
			return list.ToCommaList();
		}

		private static float TrueWorkWithCarryTime(RecipeDef d)
		{
			ThingDef stuffDef = CheapestNonDerpStuff(d);
			return (float)d.ingredients.Count * 90f + d.WorkAmountTotal(stuffDef) + 90f;
		}

		private static float CheapestIngredientValue(RecipeDef d)
		{
			float num = 0f;
			foreach (Pair<ThingDef, float> item in CheapestIngredients(d))
			{
				num += item.First.BaseMarketValue * item.Second;
			}
			return num;
		}

		private static IEnumerable<Pair<ThingDef, float>> CheapestIngredients(RecipeDef d)
		{
			foreach (IngredientCount ingredient in d.ingredients)
			{
				ThingDef thingDef = ingredient.filter.AllowedThingDefs.Where((ThingDef td) => td != ThingDefOf.Meat_Human).MinBy((ThingDef td) => td.BaseMarketValue / td.VolumePerUnit);
				yield return new Pair<ThingDef, float>(thingDef, ingredient.GetBaseCount() / d.IngredientValueGetter.ValuePerUnitOf(thingDef));
			}
		}

		private static float WorkValueEstimate(RecipeDef d)
		{
			return TrueWorkWithCarryTime(d) * 0.01f;
		}

		private static ThingDef CheapestNonDerpStuff(RecipeDef d)
		{
			ThingDef productDef = d.products[0].thingDef;
			if (!productDef.MadeFromStuff)
			{
				return null;
			}
			return d.ingredients.First().filter.AllowedThingDefs.Where((ThingDef td) => !productDef.IsWeapon || !PawnWeaponGenerator.IsDerpWeapon(productDef, td)).MinBy((ThingDef td) => td.BaseMarketValue / td.VolumePerUnit);
		}

		private static float CheapestProductsValue(RecipeDef d)
		{
			float num = 0f;
			foreach (ThingDefCountClass product in d.products)
			{
				num += product.thingDef.GetStatValueAbstract(StatDefOf.MarketValue, CheapestNonDerpStuff(d)) * (float)product.count;
			}
			return num;
		}

		private static string CostToMakeString(ThingDef d, bool real = false)
		{
			if (d.recipeMaker == null)
			{
				return "-";
			}
			return CostToMake(d, real).ToString("F1");
		}

		private static float CostToMake(ThingDef d, bool real = false)
		{
			if (d.recipeMaker == null)
			{
				return d.BaseMarketValue;
			}
			float num = 0f;
			if (d.costList != null)
			{
				foreach (ThingDefCountClass cost in d.costList)
				{
					float num2 = 1f;
					if (real)
					{
						num2 = (RequiresBuying(cost.thingDef) ? 1.4f : 0.6f);
					}
					num += (float)cost.count * CostToMake(cost.thingDef, real: true) * num2;
				}
			}
			if (d.costStuffCount > 0)
			{
				ThingDef thingDef = GenStuff.DefaultStuffFor(d);
				num += (float)d.costStuffCount * thingDef.BaseMarketValue;
			}
			return num;
		}

		private static bool RequiresBuying(ThingDef def)
		{
			if (def.costList != null)
			{
				foreach (ThingDefCountClass cost in def.costList)
				{
					if (RequiresBuying(cost.thingDef))
					{
						return true;
					}
				}
				return false;
			}
			return !DefDatabase<ThingDef>.AllDefs.Any((ThingDef d) => d.plant != null && d.plant.harvestedThingDef == def && d.plant.Sowable);
		}

		public static float WorkToProduceBest(BuildableDef d)
		{
			float num = float.MaxValue;
			if (d.StatBaseDefined(StatDefOf.WorkToMake))
			{
				num = d.GetStatValueAbstract(StatDefOf.WorkToMake);
			}
			if (d.StatBaseDefined(StatDefOf.WorkToBuild) && d.GetStatValueAbstract(StatDefOf.WorkToBuild) < num)
			{
				num = d.GetStatValueAbstract(StatDefOf.WorkToBuild);
			}
			foreach (RecipeDef allDef in DefDatabase<RecipeDef>.AllDefs)
			{
				if (allDef.workAmount > 0f && !allDef.products.NullOrEmpty())
				{
					for (int i = 0; i < allDef.products.Count; i++)
					{
						if (allDef.products[i].thingDef == d && allDef.workAmount < num)
						{
							num = allDef.workAmount;
						}
					}
				}
			}
			if (num != float.MaxValue)
			{
				return num;
			}
			return -1f;
		}

		[DebugOutput]
		public static void HediffsPriceImpact()
		{
			DebugTables.MakeTablesDialog(DefDatabase<HediffDef>.AllDefs, new List<TableDataGetter<HediffDef>>
			{
				new TableDataGetter<HediffDef>("defName", (HediffDef h) => h.defName),
				new TableDataGetter<HediffDef>("price impact", (HediffDef h) => h.priceImpact.ToStringCheckBlank()),
				new TableDataGetter<HediffDef>("price offset", delegate(HediffDef h)
				{
					if (h.priceOffset != 0f)
					{
						return h.priceOffset.ToStringMoney();
					}
					return (h.spawnThingOnRemoved != null) ? h.spawnThingOnRemoved.BaseMarketValue.ToStringMoney() : "";
				})
			}.ToArray());
		}
	}
}
