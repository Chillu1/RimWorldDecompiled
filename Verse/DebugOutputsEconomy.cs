using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class DebugOutputsEconomy
{
	[DebugOutput("Economy", false)]
	public static void ApparelByStuff()
	{
		List<FloatMenuOption> list = new List<FloatMenuOption>();
		list.Add(new FloatMenuOption("Stuffless", delegate
		{
			DoTableInternalApparel(null);
		}));
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef td) => td.IsStuff))
		{
			ThingDef localStuff = item;
			list.Add(new FloatMenuOption(localStuff.defName, delegate
			{
				DoTableInternalApparel(localStuff);
			}));
		}
		Find.WindowStack.Add(new FloatMenu(list));
	}

	[DebugOutput("Economy", false)]
	public static void ApparelArmor()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
		list.Add(new TableDataGetter<ThingDef>("stuff", (ThingDef x) => x.MadeFromStuff.ToStringCheckBlank()));
		list.Add(new TableDataGetter<ThingDef>("mass", (ThingDef x) => x.BaseMass));
		list.Add(new TableDataGetter<ThingDef>("mrkt\nvalue", (ThingDef x) => x.BaseMarketValue.ToString("F0")));
		list.Add(new TableDataGetter<ThingDef>("hp", (ThingDef x) => x.BaseMaxHitPoints));
		list.Add(new TableDataGetter<ThingDef>("flama\nbility", (ThingDef x) => x.BaseFlammability));
		list.Add(new TableDataGetter<ThingDef>("recipe\nmin\nskill", (ThingDef x) => (x.recipeMaker == null || x.recipeMaker.skillRequirements.NullOrEmpty()) ? "" : (x.recipeMaker.skillRequirements[0].skill.defName + " " + x.recipeMaker.skillRequirements[0].minLevel)));
		list.Add(new TableDataGetter<ThingDef>("equip\ndelay", (ThingDef x) => x.GetStatValueAbstract(StatDefOf.EquipDelay)));
		list.Add(new TableDataGetter<ThingDef>("none", (ThingDef x) => x.MadeFromStuff ? "" : (x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat).ToStringPercent())));
		list.Add(new TableDataGetter<ThingDef>("verbs", (ThingDef x) => string.Join(",", x.Verbs.Select((VerbProperties v) => v.label))));
		foreach (ThingDef item in new List<ThingDef>
		{
			ThingDefOf.Steel,
			ThingDefOf.Plasteel,
			ThingDefOf.Cloth,
			ThingDef.Named("Leather_Patch"),
			ThingDefOf.Leather_Plain,
			ThingDef.Named("Leather_Heavy"),
			ThingDef.Named("Leather_Thrumbo"),
			ThingDef.Named("Synthread"),
			ThingDef.Named("Hyperweave"),
			ThingDef.Named("DevilstrandCloth"),
			ThingDef.Named("WoolSheep"),
			ThingDef.Named("WoolMegasloth"),
			ThingDefOf.BlocksGranite,
			ThingDefOf.Silver,
			ThingDefOf.Gold
		})
		{
			ThingDef stuffLocal = item;
			if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
			{
				list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), (ThingDef x) => (!stuffLocal.stuffProps.CanMake(x)) ? "" : (x.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuffLocal).ToStringPercent() + " / " + x.GetStatValueAbstract(StatDefOf.ArmorRating_Heat, stuffLocal).ToStringPercent())));
			}
		}
		DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsApparel
			orderby x.BaseMarketValue
			select x, list.ToArray());
	}

	[DebugOutput("Economy", false)]
	public static void ApparelInsulation()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
		list.Add(new TableDataGetter<ThingDef>("none", (ThingDef x) => x.MadeFromStuff ? "" : (x.GetStatValueAbstract(StatDefOf.Insulation_Heat).ToStringTemperature() + " / " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold).ToStringTemperature())));
		foreach (ThingDef item in from x in DefDatabase<ThingDef>.AllDefs
			where x.IsStuff
			orderby x.BaseMarketValue
			select x)
		{
			ThingDef stuffLocal = item;
			if (DefDatabase<ThingDef>.AllDefs.Any((ThingDef x) => x.IsApparel && stuffLocal.stuffProps.CanMake(x)))
			{
				list.Add(new TableDataGetter<ThingDef>(stuffLocal.label.Shorten(), (ThingDef x) => (!stuffLocal.stuffProps.CanMake(x)) ? "" : (x.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuffLocal).ToString("F1") + ", " + x.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuffLocal).ToString("F1"))));
			}
		}
		DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsApparel
			orderby x.BaseMarketValue
			select x, list.ToArray());
	}

	[DebugOutput("Economy", false)]
	public static void ApparelCountsForNudity()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>();
		list.Add(new TableDataGetter<ThingDef>("defName", (ThingDef x) => x.defName));
		list.Add(new TableDataGetter<ThingDef>("label", (ThingDef x) => x.LabelCap));
		list.Add(new TableDataGetter<ThingDef>("countsAsClothingForNudity", (ThingDef x) => x.apparel.countsAsClothingForNudity));
		DebugTables.MakeTablesDialog(from x in DefDatabase<ThingDef>.AllDefs
			where x.IsApparel
			orderby x.BaseMarketValue
			select x, list.ToArray());
	}

	private static void DoTableInternalApparel(ThingDef stuff)
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsApparel && (stuff == null || (d.MadeFromStuff && stuff.stuffProps.CanMake(d)))), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("body\nparts", (ThingDef d) => GenText.ToSpaceList(d.apparel.bodyPartGroups.Select((BodyPartGroupDef bp) => bp.defName))), new TableDataGetter<ThingDef>("layers", (ThingDef d) => GenText.ToSpaceList(d.apparel.layers.Select((ApparelLayerDef l) => l.ToString()))), new TableDataGetter<ThingDef>("tags", (ThingDef d) => GenText.ToSpaceList(d.apparel.tags.Select((string t) => t.ToString()))), new TableDataGetter<ThingDef>("work", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.WorkToMake, stuff).ToString("F0")), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MarketValue, stuff).ToString("F0")), new TableDataGetter<ThingDef>("insul.\ncold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Cold, stuff).ToString("F1")), new TableDataGetter<ThingDef>("insul.\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Insulation_Heat, stuff).ToString("F1")), new TableDataGetter<ThingDef>("armor\nblunt", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt, stuff).ToString("F2")), new TableDataGetter<ThingDef>("armor\nsharp", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp, stuff).ToString("F2")), new TableDataGetter<ThingDef>("StuffEffectMult.\narmor", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierArmor, stuff).ToString("F2")), new TableDataGetter<ThingDef>("StuffEffectMult.\ncold", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Cold, stuff).ToString("F2")), new TableDataGetter<ThingDef>("StuffEffectMult.\nheat", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.StuffEffectMultiplierInsulation_Heat, stuff).ToString("F2")), new TableDataGetter<ThingDef>("equip\ntime", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.EquipDelay, stuff).ToString("F1")), new TableDataGetter<ThingDef>("ingredients", (ThingDef d) => CostListString(d, divideByVolume: false, starIfOnlyBuyable: false)));
	}

	[DebugOutput("Economy", false)]
	public static void RecipeSkills()
	{
		DebugTables.MakeTablesDialog(DefDatabase<RecipeDef>.AllDefs, new TableDataGetter<RecipeDef>("defName", (RecipeDef d) => d.defName), new TableDataGetter<RecipeDef>("workSkill", (RecipeDef d) => (d.workSkill != null) ? d.workSkill.defName : ""), new TableDataGetter<RecipeDef>("workSpeedStat", (RecipeDef d) => (d.workSpeedStat != null) ? d.workSpeedStat.defName : ""), new TableDataGetter<RecipeDef>("workSpeedStat's skillNeedFactors", (RecipeDef d) => (d.workSpeedStat != null) ? ((!d.workSpeedStat.skillNeedFactors.NullOrEmpty()) ? d.workSpeedStat.skillNeedFactors.Select((SkillNeed fac) => fac.skill.defName).ToCommaList() : "") : ""), new TableDataGetter<RecipeDef>("workSkillLearnFactor", (RecipeDef d) => d.workSkillLearnFactor));
	}

	[DebugOutput("Economy", false)]
	public static void Drugs()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsWithinCategory(ThingCategoryDefOf.Medicine) || d.IsWithinCategory(ThingCategoryDefOf.Drugs)), new TableDataGetter<ThingDef>("name", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("market\nvalue", (ThingDef d) => d.BaseMarketValue.ToStringMoney()), new TableDataGetter<ThingDef>("ingredients", (ThingDef d) => CostListString(d, divideByVolume: true, starIfOnlyBuyable: true)), new TableDataGetter<ThingDef>("work\namount", (ThingDef d) => (!(WorkToProduceBest(d) > 0f)) ? "-" : WorkToProduceBest(d).ToString("F0")), new TableDataGetter<ThingDef>("real\ningredient cost", (ThingDef d) => RealIngredientCost(d).ToString("F1")), new TableDataGetter<ThingDef>("real\nsell price", (ThingDef d) => RealSellPrice(d).ToStringMoney()), new TableDataGetter<ThingDef>("real\nprofit\nper item", (ThingDef d) => (RealSellPrice(d) - RealIngredientCost(d)).ToStringMoney()), new TableDataGetter<ThingDef>("real\nprofit\nper day's work", (ThingDef d) => ((RealSellPrice(d) - RealIngredientCost(d)) / WorkToProduceBest(d) * 30000f).ToStringMoney()), new TableDataGetter<ThingDef>("real\nbuy price", (ThingDef d) => RealBuyPrice(d).ToStringMoney()), new TableDataGetter<ThingDef>("for\npleasure", (ThingDef d) => d.IsPleasureDrug.ToStringCheckBlank()), new TableDataGetter<ThingDef>("non\nmedical", (ThingDef d) => d.IsNonMedicalDrug.ToStringCheckBlank()), new TableDataGetter<ThingDef>("joy", (ThingDef d) => (!d.IsPleasureDrug) ? "-" : d.ingestible.joy.ToString()), new TableDataGetter<ThingDef>("high\ngain", delegate(ThingDef d)
		{
			if (DrugStatsUtility.GetDrugHighGiver(d) == null)
			{
				return "-";
			}
			return (!(DrugStatsUtility.GetDrugHighGiver(d).severity > 0f)) ? "-" : DrugStatsUtility.GetDrugHighGiver(d).severity.ToString();
		}), new TableDataGetter<ThingDef>("high\noffset\nper day", (ThingDef d) => (DrugStatsUtility.GetDrugHighGiver(d)?.hediffDef == null) ? "-" : DrugStatsUtility.GetHighOffsetPerDay(d).ToString()), new TableDataGetter<ThingDef>("high\ndays\nper dose", (ThingDef d) => (DrugStatsUtility.GetDrugHighGiver(d)?.hediffDef == null) ? "-" : (DrugStatsUtility.GetDrugHighGiver(d).severity / (0f - DrugStatsUtility.GetHighOffsetPerDay(d))).ToString("F2")), new TableDataGetter<ThingDef>("tolerance\ngain", (ThingDef d) => (!(DrugStatsUtility.GetToleranceGain(d) > 0f)) ? "-" : DrugStatsUtility.GetToleranceGain(d).ToStringPercent()), new TableDataGetter<ThingDef>("tolerance\noffset\nper day", (ThingDef d) => (DrugStatsUtility.GetTolerance(d) == null) ? "-" : DrugStatsUtility.GetToleranceOffsetPerDay(d).ToStringPercent()), new TableDataGetter<ThingDef>("tolerance\ndays\nper dose", (ThingDef d) => (DrugStatsUtility.GetTolerance(d) == null) ? "-" : (DrugStatsUtility.GetToleranceGain(d) / (0f - DrugStatsUtility.GetToleranceOffsetPerDay(d))).ToString("F2")), new TableDataGetter<ThingDef>("addiction\nmin tolerance", (ThingDef d) => (!Addictive(d)) ? "-" : MinToleranceToAddict(d).ToString()), new TableDataGetter<ThingDef>("addiction\nnew chance", (ThingDef d) => (!Addictive(d)) ? "-" : NewAddictionChance(d).ToStringPercent()), new TableDataGetter<ThingDef>("addiction\nnew severity", (ThingDef d) => (!Addictive(d)) ? "-" : NewAddictionSeverity(d).ToString()), new TableDataGetter<ThingDef>("addiction\nold severity gain", (ThingDef d) => (!Addictive(d)) ? "-" : OldAddictionSeverityOffset(d).ToString()), new TableDataGetter<ThingDef>("addiction\noffset\nper day", (ThingDef d) => (Addiction(d) == null) ? "-" : DrugStatsUtility.GetAddictionOffsetPerDay(d).ToString()), new TableDataGetter<ThingDef>("addiction\nrecover\nmin days", (ThingDef d) => (Addiction(d) == null) ? "-" : (NewAddictionSeverity(d) / (0f - DrugStatsUtility.GetAddictionOffsetPerDay(d))).ToString("F2")), new TableDataGetter<ThingDef>("need fall\nper day", (ThingDef d) => (DrugStatsUtility.GetNeed(d) == null) ? "-" : DrugStatsUtility.GetNeed(d).fallPerDay.ToString("F2")), new TableDataGetter<ThingDef>("need cost\nper day", (ThingDef d) => (DrugStatsUtility.GetNeed(d) == null) ? "-" : DrugStatsUtility.GetAddictionNeedCostPerDay(d).ToStringMoney()), new TableDataGetter<ThingDef>("overdose\nseverity gain", (ThingDef d) => (!IsDrug(d)) ? "-" : OverdoseSeverity(d).ToString()), new TableDataGetter<ThingDef>("overdose\nrandom-emerg\nchance", (ThingDef d) => (!IsDrug(d)) ? "-" : LargeOverdoseChance(d).ToStringPercent()), new TableDataGetter<ThingDef>("combat\ndrug", (ThingDef d) => (IsDrug(d) && d.GetCompProperties<CompProperties_Drug>().isCombatEnhancingDrug).ToStringCheckBlank()), new TableDataGetter<ThingDef>("safe dose\ninterval", (ThingDef d) => DrugStatsUtility.GetSafeDoseIntervalReadout(d)));
		static HediffDef Addiction(ThingDef d)
		{
			if (!Addictive(d))
			{
				return null;
			}
			return DrugStatsUtility.GetChemical(d).addictionHediff;
		}
		static bool Addictive(ThingDef d)
		{
			if (!IsDrug(d))
			{
				return false;
			}
			return DrugStatsUtility.GetDrugComp(d).Addictive;
		}
		static bool IsDrug(ThingDef d)
		{
			return d.HasComp(typeof(CompDrug));
		}
		static float LargeOverdoseChance(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetDrugComp(d).largeOverdoseChance;
			}
			return -1f;
		}
		static float MinToleranceToAddict(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetDrugComp(d).minToleranceToAddict;
			}
			return -1f;
		}
		static float NewAddictionChance(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetDrugComp(d).addictiveness;
			}
			return -1f;
		}
		static float NewAddictionSeverity(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetChemical(d).addictionHediff.initialSeverity;
			}
			return -1f;
		}
		static float OldAddictionSeverityOffset(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetDrugComp(d).existingAddictionSeverityOffset;
			}
			return -1f;
		}
		static FloatRange OverdoseSeverity(ThingDef d)
		{
			if (IsDrug(d))
			{
				return DrugStatsUtility.GetDrugComp(d).overdoseSeverityOffset;
			}
			return FloatRange.Zero;
		}
		static float RealBuyPrice(ThingDef d)
		{
			return d.BaseMarketValue * 1.4f;
		}
		static float RealIngredientCost(ThingDef d)
		{
			return CostToMake(d, real: true);
		}
		static float RealSellPrice(ThingDef d)
		{
			return d.BaseMarketValue * 0.6f;
		}
	}

	[DebugOutput("Economy", false)]
	public static void Wool()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn && d.race.IsFlesh && d.GetCompProperties<CompProperties_Shearable>() != null), new TableDataGetter<ThingDef>("animal", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("woolDef", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.defName), new TableDataGetter<ThingDef>("woolAmount", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolAmount.ToString()), new TableDataGetter<ThingDef>("woolValue", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().woolDef.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("shear interval", (ThingDef d) => d.GetCompProperties<CompProperties_Shearable>().shearIntervalDays.ToString("F1")), new TableDataGetter<ThingDef>("value yearly", delegate(ThingDef d)
		{
			CompProperties_Shearable compProperties = d.GetCompProperties<CompProperties_Shearable>();
			return (compProperties.woolDef.BaseMarketValue * (float)compProperties.woolAmount * (60f / (float)compProperties.shearIntervalDays)).ToString("F0");
		}));
	}

	private static float AdultAgeDays(ThingDef d)
	{
		return d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f;
	}

	private static float SlaughterValue(ThingDef d)
	{
		float num = 0f;
		if (d.race.meatDef != null)
		{
			num = AnimalProductionUtility.AdultMeatAmount(d) * d.race.meatDef.BaseMarketValue;
		}
		float num2 = 0f;
		if (d.race.leatherDef != null)
		{
			num2 = AnimalProductionUtility.AdultLeatherAmount(d) * d.race.leatherDef.BaseMarketValue;
		}
		return num + num2;
	}

	private static float SlaughterValuePerGrowthYear(ThingDef d)
	{
		float num = AdultAgeDays(d) / 60f;
		return SlaughterValue(d) / num;
	}

	private static float TotalMarketValueOutputPerYear(ThingDef d)
	{
		return 0f + AnimalProductionUtility.MilkMarketValuePerYear(d) + AnimalProductionUtility.WoolMarketValuePerYear(d) + AnimalProductionUtility.EggMarketValuePerYear(d);
	}

	[DebugOutput("Economy", false)]
	public static void AnimalEconomy()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh && !d.race.Humanlike
			orderby d.devNote
			select d, new TableDataGetter<ThingDef>("devNote", (ThingDef d) => d.devNote), new TableDataGetter<ThingDef>("", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("trainability", (ThingDef d) => (d.race.trainability != null) ? d.race.trainability.defName : ""), new TableDataGetter<ThingDef>("hunger\nrate\nadult", (ThingDef d) => d.race.baseHungerRate.ToString("F2")), new TableDataGetter<ThingDef>("eaten\nnutriton\nyearly", (ThingDef d) => EatenNutritionPerYear(d).ToString("F1")), new TableDataGetter<ThingDef>("gestation\ndays raw", (ThingDef d) => AnimalProductionUtility.GestationDaysLitter(d).ToString("F1")), new TableDataGetter<ThingDef>("litter size\naverage", (ThingDef d) => LitterSizeAverage(d).ToString("F1")), new TableDataGetter<ThingDef>("gestation\ndays each", (ThingDef d) => AnimalProductionUtility.GestationDaysEach(d).ToString("F1")), new TableDataGetter<ThingDef>("herbivore", (ThingDef d) => (!AnimalProductionUtility.Herbivore(d)) ? "" : "He"), new TableDataGetter<ThingDef>("grass to\nmaintain", (ThingDef d) => AnimalProductionUtility.Herbivore(d) ? AnimalProductionUtility.GrassToMaintain(d).ToString("F0") : ""), new TableDataGetter<ThingDef>("value output\nper nutrition", (ThingDef d) => TotalMarketValuePerNutritionEaten(d).ToStringMoney()), new TableDataGetter<ThingDef>("body\nsize", (ThingDef d) => d.race.baseBodySize.ToString("F2")), new TableDataGetter<ThingDef>("filth", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.FilthRate)), new TableDataGetter<ThingDef>("adult age\ndays", (ThingDef d) => AdultAgeDays(d).ToString("F1")), new TableDataGetter<ThingDef>("nutrition to\nadulthood", (ThingDef d) => AnimalProductionUtility.NutritionToAdulthood(d).ToString("F2")), new TableDataGetter<ThingDef>("adult meat\namount", (ThingDef d) => AnimalProductionUtility.AdultMeatAmount(d).ToString("F0")), new TableDataGetter<ThingDef>("adult meat\nnutrition", (ThingDef d) => AdultMeatNutrition(d).ToString("F2")), new TableDataGetter<ThingDef>("adult meat\nnutrition per\ninput nutrition", (ThingDef d) => AdultMeatNutritionPerInput(d).ToString("F3")), new TableDataGetter<ThingDef>("slaughter value", (ThingDef d) => SlaughterValue(d).ToStringMoney()), new TableDataGetter<ThingDef>("slaughter value\n/input nutrition", (ThingDef d) => SlaughterValuePerInputNutrition(d).ToStringMoney()), new TableDataGetter<ThingDef>("slaughter value\n/growth year", (ThingDef d) => SlaughterValuePerGrowthYear(d).ToStringMoney()), new TableDataGetter<ThingDef>("eggs\nyearly", (ThingDef d) => IsEggLayer(d) ? AnimalProductionUtility.EggsPerYear(d).ToString("F1") : ""), new TableDataGetter<ThingDef>("egg\nvalue", (ThingDef d) => IsEggLayer(d) ? AnimalProductionUtility.EggMarketValue(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("egg\nvalue\nyearly", (ThingDef d) => IsEggLayer(d) ? AnimalProductionUtility.EggMarketValuePerYear(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("egg\nnutrition", (ThingDef d) => IsEggLayer(d) ? AnimalProductionUtility.EggNutrition(d).ToString("F1") : ""), new TableDataGetter<ThingDef>("egg\nnutrition\nyearly", (ThingDef d) => IsEggLayer(d) ? AnimalProductionUtility.EggNutritionPerYear(d).ToString("F1") : ""), new TableDataGetter<ThingDef>("milk\nyearly", (ThingDef d) => IsMilkable(d) ? AnimalProductionUtility.MilkPerYear(d).ToString("F1") : ""), new TableDataGetter<ThingDef>("milk\nvalue", (ThingDef d) => IsMilkable(d) ? AnimalProductionUtility.MilkMarketValue(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("milk\nvalue\nyearly", (ThingDef d) => IsMilkable(d) ? AnimalProductionUtility.MilkMarketValuePerYear(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("milk nutrition\nyearly", (ThingDef d) => IsMilkable(d) ? AnimalProductionUtility.MilkNutritionPerYear(d).ToString("F1") : ""), new TableDataGetter<ThingDef>("wool\nyearly", (ThingDef d) => IsShearable(d) ? AnimalProductionUtility.WoolPerYear(d).ToString("F0") : ""), new TableDataGetter<ThingDef>("wool\nvalue", (ThingDef d) => IsShearable(d) ? AnimalProductionUtility.WoolMarketValue(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("wool value\nyearly", (ThingDef d) => IsShearable(d) ? AnimalProductionUtility.WoolMarketValuePerYear(d).ToStringMoney() : ""), new TableDataGetter<ThingDef>("temp\nmin", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin).ToString("F0")), new TableDataGetter<ThingDef>("temp\nmax", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax).ToString("F0")), new TableDataGetter<ThingDef>("temp\nwidth", (ThingDef d) => (d.GetStatValueAbstract(StatDefOf.ComfyTemperatureMax) - d.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin)).ToString("F0")), new TableDataGetter<ThingDef>("move\nspeed", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.MoveSpeed).ToString()), new TableDataGetter<ThingDef>("wildness", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Wildness).ToStringPercent()), new TableDataGetter<ThingDef>("roam\nMTB days", (ThingDef d) => d.race.roamMtbDays.HasValue ? d.race.roamMtbDays.Value.ToString("F1") : ""), new TableDataGetter<ThingDef>("petness", (ThingDef d) => (!(d.race.petness <= 0f)) ? d.race.petness.ToStringPercent() : ""), new TableDataGetter<ThingDef>("nuzzle\nMTB hours", (ThingDef d) => (!(d.race.nuzzleMtbHours < 0f)) ? d.race.nuzzleMtbHours.ToString("F0") : ""), new TableDataGetter<ThingDef>("baby\nsize", (ThingDef d) => (d.race.lifeStageAges[0].def.bodySizeFactor * d.race.baseBodySize).ToString("F2")), new TableDataGetter<ThingDef>("nutrition to\ngestate", (ThingDef d) => AnimalProductionUtility.NutritionToGestate(d).ToString("F2")), new TableDataGetter<ThingDef>("baby meat\nnutrition", (ThingDef d) => BabyMeatNutrition(d).ToString("F2")), new TableDataGetter<ThingDef>("baby meat\nnutrition per\ninput nutrition", (ThingDef d) => BabyMeatNutritionPerInputNutrition(d).ToString("F2")), new TableDataGetter<ThingDef>("should\neat babies", (ThingDef d) => (!(BabyMeatNutritionPerInputNutrition(d) > AdultMeatNutritionPerInput(d))) ? "" : "B"));
		static float AdultMeatNutrition(ThingDef d)
		{
			return AnimalProductionUtility.AdultMeatAmount(d) * 0.05f;
		}
		static float AdultMeatNutritionPerInput(ThingDef d)
		{
			return AdultMeatNutrition(d) / AnimalProductionUtility.NutritionToAdulthood(d);
		}
		static float BabyMeatNutrition(ThingDef d)
		{
			return AdultMeatNutrition(d) * d.race.lifeStageAges[0].def.bodySizeFactor;
		}
		static float BabyMeatNutritionPerInputNutrition(ThingDef d)
		{
			return BabyMeatNutrition(d) / AnimalProductionUtility.NutritionToGestate(d);
		}
		static float EatenNutritionPerYear(ThingDef d)
		{
			return 2.6666667E-05f * d.race.baseHungerRate * 3600000f;
		}
		static bool IsEggLayer(ThingDef d)
		{
			return d.HasComp(typeof(CompEggLayer));
		}
		static bool IsMilkable(ThingDef d)
		{
			return d.HasComp(typeof(CompMilkable));
		}
		static bool IsShearable(ThingDef d)
		{
			return d.HasComp(typeof(CompShearable));
		}
		static float SlaughterValuePerInputNutrition(ThingDef d)
		{
			return SlaughterValue(d) / AnimalProductionUtility.NutritionToAdulthood(d);
		}
		static float TotalMarketValuePerNutritionEaten(ThingDef d)
		{
			return TotalMarketValueOutputPerYear(d) / EatenNutritionPerYear(d);
		}
	}

	[DebugOutput("Economy", false)]
	public static void AnimalBreeding()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.IsFlesh
			orderby AnimalProductionUtility.GestationDaysEach(d) descending
			select d, new TableDataGetter<ThingDef>("", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("gestation\ndays litter", (ThingDef d) => AnimalProductionUtility.GestationDaysEach(d).ToString("F1")), new TableDataGetter<ThingDef>("offspring\ncount range", (ThingDef d) => AnimalProductionUtility.OffspringRange(d).ToString()), new TableDataGetter<ThingDef>("gestation\ndays group", (ThingDef d) => AnimalProductionUtility.GestationDaysLitter(d).ToString("F1")), new TableDataGetter<ThingDef>("growth per 30d", delegate(ThingDef d)
		{
			float f = 1f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average : ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f));
			float num = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays : d.race.gestationPeriodDays);
			float p = 30f / num;
			return Mathf.Pow(f, p).ToString("F2");
		}), new TableDataGetter<ThingDef>("growth per 60d", delegate(ThingDef d)
		{
			float f = 1f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average : ((d.race.litterSizeCurve != null) ? Rand.ByCurveAverage(d.race.litterSizeCurve) : 1f));
			float num = d.race.lifeStageAges[d.race.lifeStageAges.Count - 1].minAge * 60f + (d.HasComp(typeof(CompEggLayer)) ? d.GetCompProperties<CompProperties_EggLayer>().eggLayIntervalDays : d.race.gestationPeriodDays);
			float p = 60f / num;
			return Mathf.Pow(f, p).ToString("F2");
		}));
	}

	private static float LitterSizeAverage(ThingDef d)
	{
		if (d.HasComp(typeof(CompEggLayer)))
		{
			return d.GetCompProperties<CompProperties_EggLayer>().eggCountRange.Average;
		}
		if (d.race.litterSizeCurve == null)
		{
			return 1f;
		}
		return Rand.ByCurveAverage(d.race.litterSizeCurve);
	}

	[DebugOutput("Economy", false)]
	public static void Buildings()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>())
			where d.BuildableByPlayer
			select d, new TableDataGetter<BuildableDef>("defName", (BuildableDef d) => d.defName), new TableDataGetter<BuildableDef>("max HP", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.MaxHitPoints)), new TableDataGetter<BuildableDef>("ingredients", (BuildableDef d) => string.Join(", ", d.CostListAdjusted(null, errorOnNullStuff: false))), new TableDataGetter<BuildableDef>("work to build", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild)), new TableDataGetter<BuildableDef>("cover effectiveness", (BuildableDef d) => (!(d is ThingDef def)) ? "" : def.BaseBlockChance().ToStringPercent()), new TableDataGetter<BuildableDef>("flammability", (BuildableDef d) => d.GetStatValueAbstract(StatDefOf.Flammability)), new TableDataGetter<BuildableDef>("terrain requirement", (BuildableDef d) => (d.terrainAffordanceNeeded == null) ? "" : d.terrainAffordanceNeeded.defName), new TableDataGetter<BuildableDef>("construction skill required", (BuildableDef d) => d.constructionSkillPrerequisite), new TableDataGetter<BuildableDef>("artistic skill required", (BuildableDef d) => d.artisticSkillPrerequisite));
	}

	[DebugOutput("Economy", false)]
	public static void Crops()
	{
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Plant && d.plant.Harvestable && d.plant.Sowable
			orderby d.plant.IsTree
			select d, new TableDataGetter<ThingDef>("plant", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("product", (ThingDef d) => d.plant.harvestedThingDef.defName), new TableDataGetter<ThingDef>("grow\ntime", (ThingDef d) => d.plant.growDays.ToString("F1")), new TableDataGetter<ThingDef>("work\nsow", (ThingDef d) => d.plant.sowWork.ToString("F0")), new TableDataGetter<ThingDef>("work\nharvest", (ThingDef d) => d.plant.harvestWork.ToString("F0")), new TableDataGetter<ThingDef>("work\ntotal", (ThingDef d) => (d.plant.sowWork + d.plant.harvestWork).ToString("F0")), new TableDataGetter<ThingDef>("harvest\nyield", (ThingDef d) => d.plant.harvestYield.ToString("F1")), new TableDataGetter<ThingDef>("work-cost\nper cycle", (ThingDef d) => workCost(d).ToString("F2")), new TableDataGetter<ThingDef>("work-cost\nper harvestCount", (ThingDef d) => (workCost(d) / d.plant.harvestYield).ToString("F2")), new TableDataGetter<ThingDef>("value\neach", (ThingDef d) => d.plant.harvestedThingDef.BaseMarketValue.ToString("F2")), new TableDataGetter<ThingDef>("harvest Value\nTotal", (ThingDef d) => (d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue).ToString("F2")), new TableDataGetter<ThingDef>("profit\nper growDay", (ThingDef d) => ((d.plant.harvestYield * d.plant.harvestedThingDef.BaseMarketValue - workCost(d)) / d.plant.growDays).ToString("F2")), new TableDataGetter<ThingDef>("nutrition\nper growDay", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? "" : (d.plant.harvestYield * d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition) / d.plant.growDays).ToString("F2")), new TableDataGetter<ThingDef>("nutrition", (ThingDef d) => (d.plant.harvestedThingDef.ingestible == null) ? "" : d.plant.harvestedThingDef.GetStatValueAbstract(StatDefOf.Nutrition).ToString("F2")), new TableDataGetter<ThingDef>("fert\nmin", (ThingDef d) => d.plant.fertilityMin.ToStringPercent()), new TableDataGetter<ThingDef>("fert\nsensitivity", (ThingDef d) => d.plant.fertilitySensitivity.ToStringPercent()), new TableDataGetter<ThingDef>("yield per\nharvest work", (ThingDef d) => (d.plant.harvestYield / d.plant.harvestWork).ToString("F3")));
		static float workCost(ThingDef d)
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
		static string recipes(ThingDef d)
		{
			List<string> list = new List<string>();
			foreach (RecipeDef allDef in DefDatabase<RecipeDef>.AllDefs)
			{
				if (!allDef.products.NullOrEmpty())
				{
					for (int i = 0; i < allDef.products.Count; i++)
					{
						if (allDef.products[i].thingDef == d)
						{
							list.Add(allDef.defName);
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
		static string workAmountSources(ThingDef d)
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
		string[] array = new string[3] { "RewardStandardHighFreq", "RewardStandardMidFreq", "RewardStandardLowFreq" };
		foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
		{
			if (allDef.thingSetMakerTags == null)
			{
				continue;
			}
			int num = 0;
			for (int num2 = 0; num2 < array.Length; num2++)
			{
				if (allDef.thingSetMakerTags.Contains(array[num2]))
				{
					num++;
				}
			}
			if (num > 1)
			{
				text = text + allDef.defName + ": " + num + " reward tags\n";
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
			if (!thing.SmeltProducts(1f).Any())
			{
				continue;
			}
			stringBuilder.Append(thing.LabelCap + ": ");
			foreach (Thing item in thing.SmeltProducts(1f))
			{
				stringBuilder.Append(" " + item.Label);
			}
			stringBuilder.AppendLine();
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
		DebugTables.MakeTablesDialog(DefDatabase<TerrainDef>.AllDefs.Where((TerrainDef d) => d.designationCategory == DesignationCategoryDefOf.Floors || d == TerrainDefOf.Soil).Concat(TerrainDefGenerator_Stone.ImpliedTerrainDefs()), new TableDataGetter<TerrainDef>("defName", (TerrainDef d) => d.defName), new TableDataGetter<TerrainDef>("stuff cost", (TerrainDef d) => d.CostList.NullOrEmpty() ? "" : d.CostList.First().Label), new TableDataGetter<TerrainDef>("work to build", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.WorkToBuild)), new TableDataGetter<TerrainDef>("beauty", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Beauty)), new TableDataGetter<TerrainDef>("cleanliness", (TerrainDef d) => d.GetStatValueAbstract(StatDefOf.Cleanliness)));
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
		if (d.CostList != null)
		{
			foreach (ThingDefCountClass cost in d.CostList)
			{
				float num = cost.count;
				if (divideByVolume)
				{
					num /= cost.thingDef.VolumePerUnit;
				}
				string text = cost.thingDef?.ToString() + " x" + num;
				if (starIfOnlyBuyable && RequiresBuying(cost.thingDef))
				{
					text += "*";
				}
				list.Add(text);
			}
		}
		if (d.MadeFromStuff)
		{
			list.Add("stuff x" + d.CostStuffCount);
		}
		return list.ToCommaList();
	}

	private static float TrueWorkWithCarryTime(RecipeDef d)
	{
		ThingDef stuff = CheapestNonDerpStuff(d);
		return (float)d.ingredients.Count * 90f + d.WorkAmountForStuff(stuff) + 90f;
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
		if (d.CostList != null)
		{
			foreach (ThingDefCountClass cost in d.CostList)
			{
				float num2 = 1f;
				if (real)
				{
					num2 = (RequiresBuying(cost.thingDef) ? 1.4f : 0.6f);
				}
				num += (float)cost.count * CostToMake(cost.thingDef, real: true) * num2;
			}
		}
		if (d.CostStuffCount > 0)
		{
			ThingDef thingDef = GenStuff.DefaultStuffFor(d);
			num += (float)d.CostStuffCount * thingDef.BaseMarketValue;
		}
		return num;
	}

	private static bool RequiresBuying(ThingDef def)
	{
		if (def.CostList != null)
		{
			foreach (ThingDefCountClass cost in def.CostList)
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
			if (!(allDef.workAmount > 0f) || allDef.products.NullOrEmpty())
			{
				continue;
			}
			for (int i = 0; i < allDef.products.Count; i++)
			{
				if (allDef.products[i].thingDef == d && allDef.workAmount < num)
				{
					num = allDef.workAmount;
				}
			}
		}
		if (num != float.MaxValue)
		{
			return num;
		}
		return -1f;
	}

	[DebugOutput("Economy", false)]
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

	[DebugOutput("Economy", false)]
	public static void RoamingVsEconomy()
	{
		float cowMarketValuePerRoam = MarketValuePerRoam(ThingDefOf.Cow);
		float cowSlaughterValuePerRoam = SlaughterValuePerRoam(ThingDefOf.Cow);
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Pawn && d.race.Roamer
			orderby d.devNote
			select d, new TableDataGetter<ThingDef>("devNote", (ThingDef d) => d.devNote), new TableDataGetter<ThingDef>("", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("wildness", (ThingDef d) => d.GetStatValueAbstract(StatDefOf.Wildness).ToStringPercent()), new TableDataGetter<ThingDef>("roam\nMTB days", (ThingDef d) => d.race.roamMtbDays.Value.ToString("F1")), new TableDataGetter<ThingDef>("roams\navg per year", (ThingDef d) => RoamsPerYear(d).ToString("F1")), new TableDataGetter<ThingDef>("trainability", (ThingDef d) => (d.race.trainability != null) ? d.race.trainability.defName : ""), new TableDataGetter<ThingDef>("grass to\nmaintain", (ThingDef d) => AnimalProductionUtility.Herbivore(d) ? AnimalProductionUtility.GrassToMaintain(d).ToString("F0") : ""), new TableDataGetter<ThingDef>("yearly market value", (ThingDef d) => TotalMarketValueOutputPerYear(d).ToStringMoney()), new TableDataGetter<ThingDef>("yearly market value\ndollars per roam", (ThingDef d) => MarketValuePerRoam(d).ToStringMoney()), new TableDataGetter<ThingDef>("yearly market value\ndollars per roam\ncow normalized", (ThingDef d) => (MarketValuePerRoam(d) / cowMarketValuePerRoam).ToString("F2")), new TableDataGetter<ThingDef>("yearly slaughter value", (ThingDef d) => SlaughterValuePerGrowthYear(d).ToStringMoney()), new TableDataGetter<ThingDef>("yearly slaughter value\ndollars per roam", (ThingDef d) => SlaughterValuePerRoam(d).ToStringMoney()), new TableDataGetter<ThingDef>("yearly slaughter value\ndollars per roam\ncow normalized", (ThingDef d) => (SlaughterValuePerRoam(d) / cowSlaughterValuePerRoam).ToString("F2")));
		static float MarketValuePerRoam(ThingDef d)
		{
			return TotalMarketValueOutputPerYear(d) / RoamsPerYear(d);
		}
		static float RoamsPerYear(ThingDef d)
		{
			return 60f / d.race.roamMtbDays.Value;
		}
		static float SlaughterValuePerRoam(ThingDef d)
		{
			return SlaughterValuePerGrowthYear(d) / RoamsPerYear(d);
		}
	}

	[DebugOutput("Economy", false)]
	public static void ArchonexusAllowedItems()
	{
		DebugTables.MakeTablesDialog(DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.ArchonexusMaxAllowedCount != 0), new TableDataGetter<ThingDef>("label", (ThingDef d) => d.LabelCap), new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("allowed count", (ThingDef d) => d.ArchonexusMaxAllowedCount), new TableDataGetter<ThingDef>("category", (ThingDef d) => string.Join(",", d.thingCategories)), new TableDataGetter<ThingDef>("sort prio", (ThingDef d) => TransferableUIUtility.DefaultArchonexusItemListOrderPriority(d)), new TableDataGetter<ThingDef>("max value", (ThingDef d) => d.BaseMarketValue * (float)d.ArchonexusMaxAllowedCount), new TableDataGetter<ThingDef>("x1 value", (ThingDef d) => d.BaseMarketValue), new TableDataGetter<ThingDef>("x1 weight", (ThingDef d) => d.BaseMass.ToString("0.###")), new TableDataGetter<ThingDef>("max stack", (ThingDef d) => d.stackLimit), new TableDataGetter<ThingDef>("weight limit", (ThingDef d) => (int)(5f / d.BaseMass)), new TableDataGetter<ThingDef>("value limit", (ThingDef d) => (int)(2000f / d.BaseMarketValue)), new TableDataGetter<ThingDef>("bringable", (ThingDef d) => MoveColonyUtility.IsBringableItem(ThingMaker.MakeThing(d, GenStuff.RandomStuffFor(d)))), new TableDataGetter<ThingDef>("show distinct", (ThingDef d) => MoveColonyUtility.IsDistinctArchonexusItem(d)), new TableDataGetter<ThingDef>("path", (ThingDef d) => d.fileName));
	}

	[DebugOutput("Economy", false)]
	public static void RewardTags()
	{
		List<TableDataGetter<ThingDef>> list = new List<TableDataGetter<ThingDef>>
		{
			new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName)
		};
		foreach (string uniqueTag in (from x in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.thingSetMakerTags != null).SelectMany((ThingDef d) => d.thingSetMakerTags)
			where x.Contains("RewardStandard")
			select x).Distinct())
		{
			list.Add(new TableDataGetter<ThingDef>(uniqueTag, (ThingDef d) => (d.thingSetMakerTags != null && d.thingSetMakerTags.Contains(uniqueTag)).ToStringCheckBlank()));
		}
		DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where d.thingSetMakerTags != null && d.thingSetMakerTags.Any((string x) => x.Contains("RewardStandard"))
			orderby d.BaseMarketValue
			select d, list.ToArray());
	}
}
