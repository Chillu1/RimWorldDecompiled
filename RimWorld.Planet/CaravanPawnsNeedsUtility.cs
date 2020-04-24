using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanPawnsNeedsUtility
	{
		public static bool CanEatForNutritionEver(ThingDef food, Pawn pawn)
		{
			if (food.IsNutritionGivingIngestible && pawn.WillEat(food, null, careIfNotAcceptableForTitle: false) && (int)food.ingestible.preferability > 1)
			{
				if (food.IsDrug)
				{
					return !pawn.IsTeetotaler();
				}
				return true;
			}
			return false;
		}

		public static bool CanEatForNutritionNow(ThingDef food, Pawn pawn)
		{
			if (!CanEatForNutritionEver(food, pawn))
			{
				return false;
			}
			if (pawn.RaceProps.Humanlike && (int)pawn.needs.food.CurCategory < 3 && (int)food.ingestible.preferability <= 3)
			{
				return false;
			}
			return true;
		}

		public static bool CanEatForNutritionNow(Thing food, Pawn pawn)
		{
			if (!food.IngestibleNow)
			{
				return false;
			}
			if (!CanEatForNutritionNow(food.def, pawn))
			{
				return false;
			}
			return true;
		}

		public static float GetFoodScore(Thing food, Pawn pawn)
		{
			float num = GetFoodScore(food.def, pawn, food.GetStatValue(StatDefOf.Nutrition));
			if (pawn.RaceProps.Humanlike)
			{
				int a = food.TryGetComp<CompRottable>()?.TicksUntilRotAtCurrentTemp ?? int.MaxValue;
				float a2 = 1f - (float)Mathf.Min(a, 3600000) / 3600000f;
				num += Mathf.Min(a2, 0.999f);
			}
			return num;
		}

		public static float GetFoodScore(ThingDef food, Pawn pawn, float singleFoodNutrition)
		{
			if (pawn.RaceProps.Humanlike)
			{
				return (int)food.ingestible.preferability;
			}
			float num = 0f;
			if (food == ThingDefOf.Kibble || food == ThingDefOf.Hay)
			{
				num = 5f;
			}
			else if (food.ingestible.preferability == FoodPreferability.DesperateOnlyForHumanlikes)
			{
				num = 4f;
			}
			else if (food.ingestible.preferability == FoodPreferability.RawBad)
			{
				num = 3f;
			}
			else if (food.ingestible.preferability == FoodPreferability.RawTasty)
			{
				num = 2f;
			}
			else if ((int)food.ingestible.preferability < 6)
			{
				num = 1f;
			}
			return num + Mathf.Min(singleFoodNutrition / 100f, 0.999f);
		}
	}
}
