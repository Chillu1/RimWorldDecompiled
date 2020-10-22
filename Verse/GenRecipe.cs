using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse
{
	public static class GenRecipe
	{
		public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
		{
			float efficiency = ((recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f);
			if (recipeDef.workTableEfficiencyStat != null)
			{
				Building_WorkTable building_WorkTable = billGiver as Building_WorkTable;
				if (building_WorkTable != null)
				{
					efficiency *= building_WorkTable.GetStatValue(recipeDef.workTableEfficiencyStat);
				}
			}
			if (recipeDef.products != null)
			{
				for (int j = 0; j < recipeDef.products.Count; j++)
				{
					ThingDefCountClass thingDefCountClass = recipeDef.products[j];
					Thing thing = ThingMaker.MakeThing(stuff: (!thingDefCountClass.thingDef.MadeFromStuff) ? null : dominantIngredient.def, def: thingDefCountClass.thingDef);
					thing.stackCount = Mathf.CeilToInt((float)thingDefCountClass.count * efficiency);
					if (dominantIngredient != null && recipeDef.useIngredientsForColor)
					{
						thing.SetColor(dominantIngredient.DrawColor, reportFailure: false);
					}
					CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
					if (compIngredients != null)
					{
						for (int l = 0; l < ingredients.Count; l++)
						{
							compIngredients.RegisterIngredient(ingredients[l].def);
						}
					}
					CompFoodPoisonable compFoodPoisonable = thing.TryGetComp<CompFoodPoisonable>();
					if (compFoodPoisonable != null)
					{
						if (Rand.Chance(worker.GetRoom()?.GetStat(RoomStatDefOf.FoodPoisonChance) ?? RoomStatDefOf.FoodPoisonChance.roomlessScore))
						{
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.FilthyKitchen);
						}
						else if (Rand.Chance(worker.GetStatValue(StatDefOf.FoodPoisonChance)))
						{
							compFoodPoisonable.SetPoisoned(FoodPoisonCause.IncompetentCook);
						}
					}
					yield return PostProcessProduct(thing, recipeDef, worker);
				}
			}
			if (recipeDef.specialProducts == null)
			{
				yield break;
			}
			for (int j = 0; j < recipeDef.specialProducts.Count; j++)
			{
				for (int k = 0; k < ingredients.Count; k++)
				{
					Thing thing2 = ingredients[k];
					switch (recipeDef.specialProducts[j])
					{
					case SpecialProductType.Butchery:
						foreach (Thing item in thing2.ButcherProducts(worker, efficiency))
						{
							yield return PostProcessProduct(item, recipeDef, worker);
						}
						break;
					case SpecialProductType.Smelted:
						foreach (Thing item2 in thing2.SmeltProducts(efficiency))
						{
							yield return PostProcessProduct(item2, recipeDef, worker);
						}
						break;
					}
				}
			}
		}

		private static Thing PostProcessProduct(Thing product, RecipeDef recipeDef, Pawn worker)
		{
			CompQuality compQuality = product.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				if (recipeDef.workSkill == null)
				{
					Log.Error(string.Concat(recipeDef, " needs workSkill because it creates a product with a quality."));
				}
				QualityCategory q = QualityUtility.GenerateQualityCreatedByPawn(worker, recipeDef.workSkill);
				compQuality.SetQuality(q, ArtGenerationContext.Colony);
				QualityUtility.SendCraftNotification(product, worker);
			}
			CompArt compArt = product.TryGetComp<CompArt>();
			if (compArt != null)
			{
				compArt.JustCreatedBy(worker);
				if (compQuality != null && (int)compQuality.Quality >= 4)
				{
					TaleRecorder.RecordTale(TaleDefOf.CraftedArt, worker, product);
				}
			}
			if (product.def.Minifiable)
			{
				product = product.MakeMinified();
			}
			return product;
		}
	}
}
