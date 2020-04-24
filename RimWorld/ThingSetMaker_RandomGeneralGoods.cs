using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_RandomGeneralGoods : ThingSetMaker
	{
		private enum GoodsType
		{
			None,
			Meals,
			RawFood,
			Medicine,
			Drugs,
			Resources
		}

		private static Pair<GoodsType, float>[] GoodsWeights = new Pair<GoodsType, float>[5]
		{
			new Pair<GoodsType, float>(GoodsType.Meals, 1f),
			new Pair<GoodsType, float>(GoodsType.RawFood, 0.75f),
			new Pair<GoodsType, float>(GoodsType.Medicine, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Drugs, 0.234f),
			new Pair<GoodsType, float>(GoodsType.Resources, 0.234f)
		};

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IntRange intRange = parms.countRange ?? new IntRange(10, 20);
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			int num = Mathf.Max(intRange.RandomInRange, 1);
			for (int i = 0; i < num; i++)
			{
				outThings.Add(GenerateSingle(techLevel));
			}
		}

		private Thing GenerateSingle(TechLevel techLevel)
		{
			Thing thing = null;
			int num = 0;
			while (thing == null && num < 50)
			{
				switch (GoodsWeights.RandomElementByWeight((Pair<GoodsType, float> x) => x.Second).First)
				{
				case GoodsType.Meals:
					thing = RandomMeals(techLevel);
					break;
				case GoodsType.RawFood:
					thing = RandomRawFood(techLevel);
					break;
				case GoodsType.Medicine:
					thing = RandomMedicine(techLevel);
					break;
				case GoodsType.Drugs:
					thing = RandomDrugs(techLevel);
					break;
				case GoodsType.Resources:
					thing = RandomResources(techLevel);
					break;
				default:
					throw new Exception();
				}
				num++;
			}
			return thing;
		}

		private Thing RandomMeals(TechLevel techLevel)
		{
			ThingDef thingDef;
			if (techLevel.IsNeolithicOrWorse())
			{
				thingDef = ThingDefOf.Pemmican;
			}
			else
			{
				float value = Rand.Value;
				thingDef = ((value < 0.5f) ? ThingDefOf.MealSimple : ((!((double)value < 0.75)) ? ThingDefOf.MealSurvivalPack : ThingDefOf.MealFine));
			}
			Thing thing = ThingMaker.MakeThing(thingDef);
			int num = Mathf.Min(thingDef.stackLimit, 10);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		private Thing RandomRawFood(TechLevel techLevel)
		{
			if (!PossibleRawFood(techLevel).TryRandomElement(out ThingDef result))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private IEnumerable<ThingDef> PossibleRawFood(TechLevel techLevel)
		{
			return ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.HasComp(typeof(CompHatcher)) && (int)x.techLevel <= (int)techLevel && (int)x.ingestible.preferability < 6);
		}

		private Thing RandomMedicine(TechLevel techLevel)
		{
			ThingDef result;
			if (Rand.Value < 0.75f && (int)techLevel >= (int)ThingDefOf.MedicineHerbal.techLevel)
			{
				result = ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsMedicine && (int)x.techLevel <= (int)techLevel).MaxBy((ThingDef x) => x.GetStatValueAbstract(StatDefOf.MedicalPotency));
			}
			else if (!ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsMedicine).TryRandomElement(out result))
			{
				return null;
			}
			if (techLevel.IsNeolithicOrWorse())
			{
				result = ThingDefOf.MedicineHerbal;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 20);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomDrugs(TechLevel techLevel)
		{
			if (!ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsDrug && (int)x.techLevel <= (int)techLevel).TryRandomElement(out ThingDef result))
			{
				return null;
			}
			Thing thing = ThingMaker.MakeThing(result);
			int max = Mathf.Min(result.stackLimit, 25);
			thing.stackCount = Rand.RangeInclusive(1, max);
			return thing;
		}

		private Thing RandomResources(TechLevel techLevel)
		{
			ThingDef thingDef = BaseGenUtility.RandomCheapWallStuff(techLevel);
			Thing thing = ThingMaker.MakeThing(thingDef);
			int num = Mathf.Min(thingDef.stackLimit, 75);
			thing.stackCount = Rand.RangeInclusive(num / 2, num);
			return thing;
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			TechLevel techLevel = parms.techLevel ?? TechLevel.Undefined;
			if (techLevel.IsNeolithicOrWorse())
			{
				yield return ThingDefOf.Pemmican;
			}
			else
			{
				yield return ThingDefOf.MealSimple;
				yield return ThingDefOf.MealFine;
				yield return ThingDefOf.MealSurvivalPack;
			}
			foreach (ThingDef item in PossibleRawFood(techLevel))
			{
				yield return item;
			}
			foreach (ThingDef item2 in ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsMedicine))
			{
				yield return item2;
			}
			foreach (ThingDef item3 in ThingSetMakerUtility.allGeneratableItems.Where((ThingDef x) => x.IsDrug && (int)x.techLevel <= (int)techLevel))
			{
				yield return item3;
			}
			if (techLevel.IsNeolithicOrWorse())
			{
				yield return ThingDefOf.WoodLog;
			}
			else
			{
				foreach (ThingDef item4 in DefDatabase<ThingDef>.AllDefsListForReading.Where((ThingDef d) => BaseGenUtility.IsCheapWallStuff(d)))
				{
					yield return item4;
				}
			}
		}
	}
}
