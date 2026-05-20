using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Need_Food : Need
{
	public int lastNonStarvingTick = -99999;

	public const float BaseFoodFallPerTick = 2.6666667E-05f;

	private const float BaseMalnutritionSeverityPerDay = 0.453f;

	private const float BaseMalnutritionSeverityPerInterval = 0.0011325f;

	private CompHoldingPlatformTarget platformComp;

	public bool Starving => CurCategory == HungerCategory.Starving;

	public float PercentageThreshUrgentlyHungry => pawn.RaceProps.FoodLevelPercentageWantEat * 0.4f;

	public float PercentageThreshHungry => pawn.RaceProps.FoodLevelPercentageWantEat * 0.8f;

	public float NutritionBetweenHungryAndFed => (1f - PercentageThreshHungry) * MaxLevel;

	private CompHoldingPlatformTarget PlatformTarget => platformComp ?? (platformComp = pawn.TryGetComp<CompHoldingPlatformTarget>());

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

	public override int GUIChangeArrow
	{
		get
		{
			if (GainingFood())
			{
				return 1;
			}
			if (!(FoodFallPerTickAssumingCategory(HungerCategory.Hungry) > 0f))
			{
				return 0;
			}
			return -1;
		}
	}

	public override float MaxLevel
	{
		get
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return pawn.BodySize * pawn.ageTracker.CurLifeStage.foodMaxFactor;
			}
			return pawn.GetStatValue(StatDefOf.MaxNutrition, applyPostProcess: true, 15);
		}
	}

	public float NutritionWanted => MaxLevel - CurLevel;

	public int TicksStarving => Mathf.Max(0, Find.TickManager.TicksGame - lastNonStarvingTick);

	private float MalnutritionSeverityPerInterval => 0.0011325f * Mathf.Lerp(0.8f, 1.2f, Rand.ValueSeeded(pawn.thingIDNumber ^ 0x26EF7A));

	protected override bool IsFrozen
	{
		get
		{
			if (!base.IsFrozen && !pawn.Deathresting)
			{
				return PlatformTarget?.CurrentlyHeldOnPlatform ?? false;
			}
			return true;
		}
	}

	public Need_Food(Pawn pawn)
		: base(pawn)
	{
	}

	public bool GainingFood()
	{
		if (pawn.jobs?.curDriver is IEatingDriver { GainingNutritionNow: not false })
		{
			return true;
		}
		if (ModsConfig.BiotechActive && ChildcareUtility.CanSuckle(pawn, out var _) && pawn.CarriedBy?.jobs.curDriver is JobDriver_FeedBaby { Feeding: not false })
		{
			return true;
		}
		return false;
	}

	public float FoodFallPerTickAssumingCategory(HungerCategory hunger, bool ignoreMalnutrition = false)
	{
		Building_Bed building_Bed = pawn.CurrentBed();
		float num = BaseHungerRate(pawn.ageTracker.CurLifeStage, pawn.def) * hunger.HungerMultiplier() * pawn.health.hediffSet.GetHungerRateFactor(ignoreMalnutrition ? HediffDefOf.Malnutrition : null) * (pawn.story?.traits?.HungerRateFactor ?? 1f) * (building_Bed?.GetStatValue(StatDefOf.BedHungerRateFactor) ?? 1f);
		if (ModsConfig.BiotechActive)
		{
			Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating);
			if (firstHediffOfDef != null)
			{
				HediffComp_Lactating hediffComp_Lactating = firstHediffOfDef.TryGetComp<HediffComp_Lactating>();
				if (hediffComp_Lactating != null)
				{
					num += hediffComp_Lactating.AddedNutritionPerDay() / 60000f;
				}
			}
		}
		if (ModsConfig.BiotechActive && pawn.genes != null)
		{
			int num2 = 0;
			foreach (Gene item in pawn.genes.GenesListForReading)
			{
				if (!item.Overridden)
				{
					num2 += item.def.biostatMet;
				}
			}
			num *= GeneTuning.MetabolismToFoodConsumptionFactorCurve.Evaluate(num2);
		}
		if (ModsConfig.AnomalyActive)
		{
			CompHoldingPlatformTarget platformTarget = PlatformTarget;
			if (platformTarget != null && platformTarget.CurrentlyHeldOnPlatform)
			{
				num = 0f;
			}
		}
		return num;
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
		if (!IsFrozen || pawn.Deathresting)
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
		StatDefOf.MaxNutrition.Worker.ClearCacheForThing(pawn);
		base.CurLevelPercentage = (pawn.RaceProps.Humanlike ? 0.8f : Rand.Range(0.5f, 0.9f));
		if (Current.ProgramState == ProgramState.Playing)
		{
			lastNonStarvingTick = Find.TickManager.TicksGame;
		}
	}

	public override void OnNeedRemoved()
	{
		if (pawn.health.hediffSet.TryGetHediff(HediffDefOf.Malnutrition, out var hediff))
		{
			pawn.health.RemoveHediff(hediff);
		}
	}

	public override string GetTipString()
	{
		return (base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + " (" + CurLevel.ToString("0.##") + " / " + MaxLevel.ToString("0.##") + ")\n" + def.description;
	}

	public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
	{
		if (threshPercents == null)
		{
			threshPercents = new List<float>();
		}
		threshPercents.Clear();
		threshPercents.Add(PercentageThreshHungry);
		threshPercents.Add(PercentageThreshUrgentlyHungry);
		base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
	}

	public static float BaseHungerRate(LifeStageDef lifeStage, ThingDef pawnDef)
	{
		return lifeStage.hungerRateFactor * pawnDef.race.baseHungerRate * 2.6666667E-05f;
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref lastNonStarvingTick, "lastNonStarvingTick", -99999);
	}
}
