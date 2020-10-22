using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class RecipeDefGenerator
	{
		public static IEnumerable<RecipeDef> ImpliedRecipeDefs()
		{
			foreach (RecipeDef item in DefsFromRecipeMakers().Concat(DrugAdministerDefs()))
			{
				yield return item;
			}
		}

		private static IEnumerable<RecipeDef> DefsFromRecipeMakers()
		{
			foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.recipeMaker != null))
			{
				yield return CreateRecipeDefFromMaker(def);
				if (def.recipeMaker.bulkRecipeCount > 0)
				{
					yield return CreateRecipeDefFromMaker(def, def.recipeMaker.bulkRecipeCount);
				}
			}
		}

		private static RecipeDef CreateRecipeDefFromMaker(ThingDef def, int adjustedCount = 1)
		{
			RecipeMakerProperties recipeMaker = def.recipeMaker;
			RecipeDef recipeDef = new RecipeDef();
			recipeDef.defName = "Make_" + def.defName;
			if (adjustedCount != 1)
			{
				recipeDef.defName += "Bulk";
			}
			string text = def.label;
			if (adjustedCount != 1)
			{
				text = text + " x" + adjustedCount;
			}
			recipeDef.label = "RecipeMake".Translate(text);
			recipeDef.jobString = "RecipeMakeJobString".Translate(text);
			recipeDef.modContentPack = def.modContentPack;
			recipeDef.workAmount = recipeMaker.workAmount * adjustedCount;
			recipeDef.workSpeedStat = recipeMaker.workSpeedStat;
			recipeDef.efficiencyStat = recipeMaker.efficiencyStat;
			if (def.MadeFromStuff)
			{
				IngredientCount ingredientCount = new IngredientCount();
				ingredientCount.SetBaseCount(def.costStuffCount * adjustedCount);
				ingredientCount.filter.SetAllowAllWhoCanMake(def);
				recipeDef.ingredients.Add(ingredientCount);
				recipeDef.fixedIngredientFilter.SetAllowAllWhoCanMake(def);
				recipeDef.productHasIngredientStuff = true;
			}
			recipeDef.useIngredientsForColor = recipeMaker.useIngredientsForColor;
			if (def.costList != null)
			{
				foreach (ThingDefCountClass cost in def.costList)
				{
					IngredientCount ingredientCount2 = new IngredientCount();
					ingredientCount2.SetBaseCount(cost.count * adjustedCount);
					ingredientCount2.filter.SetAllow(cost.thingDef, allow: true);
					recipeDef.ingredients.Add(ingredientCount2);
				}
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
			recipeDef.effectWorking = recipeMaker.effectWorking;
			recipeDef.soundWorking = recipeMaker.soundWorking;
			recipeDef.researchPrerequisite = recipeMaker.researchPrerequisite;
			recipeDef.researchPrerequisites = recipeMaker.researchPrerequisites;
			recipeDef.factionPrerequisiteTags = recipeMaker.factionPrerequisiteTags;
			string[] items = recipeDef.products.Select((ThingDefCountClass p) => (p.count != 1) ? p.Label : Find.ActiveLanguageWorker.WithIndefiniteArticle(p.thingDef.label)).ToArray();
			recipeDef.description = "RecipeMakeDescription".Translate(items.ToCommaList(useAnd: true));
			recipeDef.descriptionHyperlinks = recipeDef.products.Select((ThingDefCountClass p) => new DefHyperlink(p.thingDef)).ToList();
			if (adjustedCount != 1 && recipeDef.workAmount < 0f)
			{
				recipeDef.workAmount = recipeDef.WorkAmountTotal(null) * (float)adjustedCount;
			}
			return recipeDef;
		}

		private static IEnumerable<RecipeDef> DrugAdministerDefs()
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.IsDrug))
			{
				RecipeDef recipeDef = new RecipeDef();
				recipeDef.defName = "Administer_" + item.defName;
				recipeDef.label = "RecipeAdminister".Translate(item.label);
				recipeDef.jobString = "RecipeAdministerJobString".Translate(item.label);
				recipeDef.workerClass = typeof(Recipe_AdministerIngestible);
				recipeDef.targetsBodyPart = false;
				recipeDef.anesthetize = false;
				recipeDef.surgerySuccessChanceFactor = 99999f;
				recipeDef.modContentPack = item.modContentPack;
				recipeDef.workAmount = item.ingestible.baseIngestTicks;
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
}
