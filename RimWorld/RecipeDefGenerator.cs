using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class RecipeDefGenerator
{
	public static IEnumerable<RecipeDef> ImpliedRecipeDefs(bool hotReload = false)
	{
		foreach (RecipeDef item in DefsFromRecipeMakers(hotReload).Concat(DrugAdministerDefs(hotReload)))
		{
			yield return item;
		}
	}

	private static IEnumerable<RecipeDef> DefsFromRecipeMakers(bool hotReload = false)
	{
		foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.recipeMaker != null))
		{
			yield return CreateRecipeDefFromMaker(def, 1, hotReload);
			if (def.recipeMaker.bulkRecipeCount > 0)
			{
				yield return CreateRecipeDefFromMaker(def, def.recipeMaker.bulkRecipeCount, hotReload);
			}
		}
	}

	private static RecipeDef CreateRecipeDefFromMaker(ThingDef def, int adjustedCount = 1, bool hotReload = false)
	{
		RecipeMakerProperties recipeMaker = def.recipeMaker;
		string text = "Make_" + def.defName;
		if (adjustedCount != 1)
		{
			text += "Bulk";
		}
		RecipeDef recipeDef = (hotReload ? (DefDatabase<RecipeDef>.GetNamed(text, errorOnFail: false) ?? new RecipeDef()) : new RecipeDef());
		recipeDef.defName = text;
		string text2 = def.label;
		if (adjustedCount != 1)
		{
			text2 = text2 + " x" + adjustedCount;
		}
		if (string.IsNullOrEmpty(recipeMaker.label))
		{
			recipeDef.label = "RecipeMake".Translate(text2);
		}
		else
		{
			recipeDef.label = recipeMaker.label;
		}
		recipeDef.jobString = "RecipeMakeJobString".Translate(text2);
		recipeDef.modContentPack = def.modContentPack;
		recipeDef.displayPriority = recipeMaker.displayPriority + adjustedCount - 1;
		recipeDef.workAmount = recipeMaker.workAmount * adjustedCount;
		recipeDef.workSpeedStat = recipeMaker.workSpeedStat;
		recipeDef.efficiencyStat = recipeMaker.efficiencyStat;
		SetIngredients(recipeDef, def, adjustedCount);
		recipeDef.useIngredientsForColor = recipeMaker.useIngredientsForColor;
		if (def.costListForDifficulty != null)
		{
			recipeDef.regenerateOnDifficultyChange = true;
		}
		recipeDef.defaultIngredientFilter = recipeMaker.defaultIngredientFilter;
		recipeDef.products.Add(new ThingDefCountClass(def, recipeMaker.productCount * adjustedCount));
		recipeDef.targetCountAdjustment = recipeMaker.targetCountAdjustment * adjustedCount;
		recipeDef.skillRequirements = recipeMaker.skillRequirements.ListFullCopyOrNull();
		recipeDef.workSkill = recipeMaker.workSkill;
		recipeDef.workSkillLearnFactor = recipeMaker.workSkillLearnPerTick;
		recipeDef.requiredGiverWorkType = recipeMaker.requiredGiverWorkType;
		recipeDef.unfinishedThingDef = recipeMaker.unfinishedThingDef;
		recipeDef.recipeUsers = recipeMaker.recipeUsers.ListFullCopyOrNull();
		recipeDef.mechanitorOnlyRecipe = recipeMaker.mechanitorOnlyRecipe;
		recipeDef.effectWorking = recipeMaker.effectWorking;
		recipeDef.soundWorking = recipeMaker.soundWorking;
		recipeDef.researchPrerequisite = recipeMaker.researchPrerequisite;
		recipeDef.memePrerequisitesAny = recipeMaker.memePrerequisitesAny;
		recipeDef.researchPrerequisites = recipeMaker.researchPrerequisites;
		recipeDef.factionPrerequisiteTags = recipeMaker.factionPrerequisiteTags;
		recipeDef.fromIdeoBuildingPreceptOnly = recipeMaker.fromIdeoBuildingPreceptOnly;
		string[] items = recipeDef.products.Select((ThingDefCountClass p) => (p.count != 1) ? p.Label : Find.ActiveLanguageWorker.WithIndefiniteArticle(p.thingDef.label)).ToArray();
		recipeDef.description = "RecipeMakeDescription".Translate(items.ToCommaList(useAnd: true));
		recipeDef.descriptionHyperlinks = recipeDef.products.Select((ThingDefCountClass p) => new DefHyperlink(p.thingDef)).ToList();
		if (adjustedCount != 1 && recipeDef.workAmount < 0f)
		{
			recipeDef.workAmount = recipeDef.WorkAmountTotal(null) * (float)adjustedCount;
		}
		return recipeDef;
	}

	public static void SetIngredients(RecipeDef r, ThingDef def, int adjustedCount = 1)
	{
		r.ingredients.Clear();
		r.adjustedCount = adjustedCount;
		if (def.MadeFromStuff)
		{
			IngredientCount ingredientCount = new IngredientCount();
			ingredientCount.SetBaseCount(def.CostStuffCount * adjustedCount);
			ingredientCount.filter.SetAllowAllWhoCanMake(def);
			ingredientCount.filter.customSummary = def.stuffCategorySummary ?? def.stuffCategories.Select((StuffCategoryDef category) => category.noun).ToCommaListOr();
			r.ingredients.Add(ingredientCount);
			r.fixedIngredientFilter.SetAllowAllWhoCanMake(def);
			r.productHasIngredientStuff = true;
		}
		if (def.CostList == null)
		{
			return;
		}
		foreach (ThingDefCountClass cost in def.CostList)
		{
			IngredientCount ingredientCount2 = new IngredientCount();
			ingredientCount2.SetBaseCount(cost.count * adjustedCount);
			ingredientCount2.filter.SetAllow(cost.thingDef, allow: true);
			r.ingredients.Add(ingredientCount2);
		}
	}

	public static void ResetRecipeIngredientsForDifficulty()
	{
		foreach (RecipeDef item in DefDatabase<RecipeDef>.AllDefs.Where((RecipeDef x) => x.regenerateOnDifficultyChange))
		{
			SetIngredients(item, item.products[0].thingDef, item.adjustedCount);
		}
	}

	private static IEnumerable<RecipeDef> DrugAdministerDefs(bool hotReload = false)
	{
		foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsDrug))
		{
			string defName = "Administer_" + item.defName;
			RecipeDef recipeDef = (hotReload ? (DefDatabase<RecipeDef>.GetNamed(defName, errorOnFail: false) ?? new RecipeDef()) : new RecipeDef());
			recipeDef.defName = defName;
			recipeDef.label = "RecipeAdminister".Translate(item.label);
			recipeDef.jobString = "RecipeAdministerJobString".Translate(item.label);
			recipeDef.workerClass = typeof(Recipe_AdministerIngestible);
			recipeDef.targetsBodyPart = false;
			recipeDef.anesthetize = false;
			recipeDef.surgerySuccessChanceFactor = 99999f;
			recipeDef.modContentPack = item.modContentPack;
			recipeDef.workAmount = item.ingestible.baseIngestTicks;
			recipeDef.humanlikeOnly = item.ingestible.humanlikeOnly;
			IngredientCount ingredientCount = new IngredientCount();
			ingredientCount.SetBaseCount(1f);
			ingredientCount.filter.SetAllow(item, allow: true);
			recipeDef.ingredients.Add(ingredientCount);
			recipeDef.fixedIngredientFilter.SetAllow(item, allow: true);
			recipeDef.recipeUsers = new List<ThingDef>();
			foreach (ThingDef item2 in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn && d.race.IsFlesh))
			{
				recipeDef.recipeUsers.Add(item2);
			}
			yield return recipeDef;
		}
	}
}
