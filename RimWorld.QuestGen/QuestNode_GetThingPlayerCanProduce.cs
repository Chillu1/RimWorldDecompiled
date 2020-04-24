using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.QuestGen
{
	public class QuestNode_GetThingPlayerCanProduce : QuestNode
	{
		[NoTranslate]
		public SlateRef<string> storeProductionItemDefAs;

		[NoTranslate]
		public SlateRef<string> storeProductionItemStuffAs;

		[NoTranslate]
		public SlateRef<string> storeProductionItemCountAs;

		[NoTranslate]
		public SlateRef<string> storeProductionItemLabelAs;

		public SlateRef<SimpleCurve> pointsToRequiredWorkCurve;

		public SlateRef<SimpleCurve> pointsToMaxItemMarketValueCurve;

		public SlateRef<float?> maxMarketValueFactor;

		public SlateRef<float> minStuffCommonality;

		public SlateRef<FloatRange?> workAmountRandomFactorRange;

		public SlateRef<FloatRange?> productionItemCountRandomFactorRange;

		private static List<ThingDef> allWorkTables = new List<ThingDef>();

		private static List<Pair<ThingStuffPair, int>> tmpCandidates = new List<Pair<ThingStuffPair, int>>();

		public static void ResetStaticData()
		{
			allWorkTables.Clear();
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].IsWorkTable)
				{
					allWorkTables.Add(allDefsListForReading[i]);
				}
			}
		}

		protected override bool TestRunInt(Slate slate)
		{
			return DoWork(slate);
		}

		protected override void RunInt()
		{
			DoWork(QuestGen.slate);
		}

		private bool DoWork(Slate slate)
		{
			Map map = slate.Get<Map>("map");
			if (map == null)
			{
				return false;
			}
			float x2 = slate.Get("points", 0f);
			SimpleCurve value = pointsToMaxItemMarketValueCurve.GetValue(slate);
			float num = maxMarketValueFactor.GetValue(slate) ?? 1f;
			float maxMarketValue = value.Evaluate(x2) * num;
			SimpleCurve value2 = pointsToRequiredWorkCurve.GetValue(slate);
			float randomInRange = (workAmountRandomFactorRange.GetValue(slate) ?? FloatRange.One).RandomInRange;
			float num2 = value2.Evaluate(x2) * randomInRange;
			tmpCandidates.Clear();
			for (int j = 0; j < allWorkTables.Count; j++)
			{
				if (BuildCopyCommandUtility.FindAllowedDesignator(allWorkTables[j]) == null)
				{
					continue;
				}
				List<RecipeDef> recipes = allWorkTables[j].AllRecipes;
				for (int i = 0; i < recipes.Count; i++)
				{
					if (recipes[i].AvailableNow && recipes[i].products.Any() && !recipes[i].PotentiallyMissingIngredients(null, map).Any())
					{
						foreach (ThingDef stuff in recipes[i].products[0].thingDef.MadeFromStuff ? GenStuff.AllowedStuffsFor(recipes[i].products[0].thingDef) : Gen.YieldSingle<ThingDef>(null))
						{
							if (stuff == null || (map.listerThings.ThingsOfDef(stuff).Any() && !(stuff.stuffProps.commonality < minStuffCommonality.GetValue(slate))))
							{
								int num3 = 0;
								if (stuff != null)
								{
									List<Thing> list = map.listerThings.ThingsOfDef(stuff);
									for (int k = 0; k < list.Count; k++)
									{
										num3 += list[k].stackCount;
									}
								}
								float num4 = recipes[i].WorkAmountTotal(stuff);
								if (num4 > 0f)
								{
									int num5 = Mathf.FloorToInt(num2 / num4);
									if (stuff != null)
									{
										IngredientCount ingredientCount = recipes[i].ingredients.Where((IngredientCount x) => x.filter.Allows(stuff)).MaxByWithFallback((IngredientCount x) => x.CountRequiredOfFor(stuff, recipes[i]));
										num5 = Mathf.Min(num5, Mathf.FloorToInt((float)num3 / (float)ingredientCount.CountRequiredOfFor(stuff, recipes[i])));
									}
									if (num5 > 0)
									{
										tmpCandidates.Add(new Pair<ThingStuffPair, int>(new ThingStuffPair(recipes[i].products[0].thingDef, stuff), recipes[i].products[0].count * num5));
									}
								}
							}
						}
					}
				}
			}
			tmpCandidates.RemoveAll((Pair<ThingStuffPair, int> x) => x.Second <= 0);
			tmpCandidates.RemoveAll((Pair<ThingStuffPair, int> x) => StatDefOf.MarketValue.Worker.GetValueAbstract(x.First.thing, x.First.stuff) > maxMarketValue);
			if (!tmpCandidates.Any())
			{
				return false;
			}
			Pair<ThingStuffPair, int> pair = tmpCandidates.RandomElement();
			int num6 = Mathf.Min(Mathf.RoundToInt(maxMarketValue / StatDefOf.MarketValue.Worker.GetValueAbstract(pair.First.thing, pair.First.stuff)), pair.Second);
			float randomInRange2 = (productionItemCountRandomFactorRange.GetValue(slate) ?? FloatRange.One).RandomInRange;
			num6 = Mathf.RoundToInt((float)num6 * randomInRange2);
			num6 = Mathf.Max(num6, 1);
			slate.Set(storeProductionItemDefAs.GetValue(slate), pair.First.thing);
			slate.Set(storeProductionItemStuffAs.GetValue(slate), pair.First.stuff);
			slate.Set(storeProductionItemCountAs.GetValue(slate), num6);
			string value3 = storeProductionItemLabelAs.GetValue(slate);
			if (!string.IsNullOrEmpty(value3))
			{
				slate.Set(value3, GenLabel.ThingLabel(pair.First.thing, pair.First.stuff, num6));
			}
			return true;
		}
	}
}
