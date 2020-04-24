using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Need_Food : Need
	{
		private int lastNonStarvingTick = -99999;

		public const float BaseFoodFallPerTick = 2.66666666E-05f;

		private const float BaseMalnutritionSeverityPerDay = 0.17f;

		private const float BaseMalnutritionSeverityPerInterval = 0.00113333331f;

		public bool Starving => CurCategory == HungerCategory.Starving;

		public float PercentageThreshUrgentlyHungry => pawn.RaceProps.FoodLevelPercentageWantEat * 0.4f;

		public float PercentageThreshHungry => pawn.RaceProps.FoodLevelPercentageWantEat * 0.8f;

		public float NutritionBetweenHungryAndFed => (1f - PercentageThreshHungry) * MaxLevel;

		public HungerCategory CurCategory
		{
			get
			{
				if (base.CurLevelPercentage <= 0f)
				{
					return HungerCategory.Starving;
				}
				if (base.CurLevelPercentage < PercentageThreshUrgentlyHungry)
				{
					return HungerCategory.UrgentlyHungry;
				}
				if (base.CurLevelPercentage < PercentageThreshHungry)
				{
					return HungerCategory.Hungry;
				}
				return HungerCategory.Fed;
			}
		}

		public float FoodFallPerTick => FoodFallPerTickAssumingCategory(CurCategory);

		public int TicksUntilHungryWhenFed => Mathf.CeilToInt(NutritionBetweenHungryAndFed / FoodFallPerTickAssumingCategory(HungerCategory.Fed));

		public int TicksUntilHungryWhenFedIgnoringMalnutrition => Mathf.CeilToInt(NutritionBetweenHungryAndFed / FoodFallPerTickAssumingCategory(HungerCategory.Fed, ignoreMalnutrition: true));

		public override int GUIChangeArrow => -1;

		public override float MaxLevel => pawn.BodySize * pawn.ageTracker.CurLifeStage.foodMaxFactor;

		public float NutritionWanted => MaxLevel - CurLevel;

		private float HungerRate => pawn.ageTracker.CurLifeStage.hungerRateFactor * pawn.RaceProps.baseHungerRate * pawn.health.hediffSet.HungerRateFactor * pawn.GetStatValue(StatDefOf.HungerRateMultiplier);

		private float HungerRateIgnoringMalnutrition => pawn.ageTracker.CurLifeStage.hungerRateFactor * pawn.RaceProps.baseHungerRate * pawn.health.hediffSet.GetHungerRateFactor(HediffDefOf.Malnutrition) * pawn.GetStatValue(StatDefOf.HungerRateMultiplier);

		public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - lastNonStarvingTick);

		private float MalnutritionSeverityPerInterval => 0.00113333331f * Mathf.Lerp(0.8f, 1.2f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x26EF7A));

		public Need_Food(Pawn pawn)
			: base(pawn)
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref lastNonStarvingTick, "lastNonStarvingTick", -99999);
		}

		public float FoodFallPerTickAssumingCategory(HungerCategory cat, bool ignoreMalnutrition = false)
		{
			float num = ignoreMalnutrition ? HungerRateIgnoringMalnutrition : HungerRate;
			switch (cat)
			{
			case HungerCategory.Fed:
				return 2.66666666E-05f * num;
			case HungerCategory.Hungry:
				return 2.66666666E-05f * num * 0.5f;
			case HungerCategory.UrgentlyHungry:
				return 2.66666666E-05f * num * 0.25f;
			case HungerCategory.Starving:
				return 2.66666666E-05f * num * 0.15f;
			default:
				return 999f;
			}
		}

		public override void NeedInterval()
		{
			if (!IsFrozen)
			{
				CurLevel -= FoodFallPerTick * 150f;
			}
			if (!Starving)
			{
				lastNonStarvingTick = Find.TickManager.TicksGame;
			}
			if (!IsFrozen)
			{
				if (Starving)
				{
					HealthUtility.AdjustSeverity(pawn, HediffDefOf.Malnutrition, MalnutritionSeverityPerInterval);
				}
				else
				{
					HealthUtility.AdjustSeverity(pawn, HediffDefOf.Malnutrition, 0f - MalnutritionSeverityPerInterval);
				}
			}
		}

		public override void SetInitialLevel()
		{
			if (pawn.RaceProps.Humanlike)
			{
				base.CurLevelPercentage = 0.8f;
			}
			else
			{
				base.CurLevelPercentage = Rand.Range(0.5f, 0.9f);
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				lastNonStarvingTick = Find.TickManager.TicksGame;
			}
		}

		public override string GetTipString()
		{
			return base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent() + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description;
		}

		public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true)
		{
			if (threshPercents == null)
			{
				threshPercents = new List<float>();
			}
			threshPercents.Clear();
			threshPercents.Add(PercentageThreshHungry);
			threshPercents.Add(PercentageThreshUrgentlyHungry);
			base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
		}
	}
}
