using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class Need_Learning : Need
{
	private static string learningActivitiesLineList;

	public const float BaseFallPerTick = 3E-06f;

	public const float BaseFallPerInterval = 0.00045000002f;

	public const float ThresholdEmpty = 0.01f;

	public const float ThresholdLow = 0.15f;

	public const float ThresholdSatisfied = 0.3f;

	public const float ThresholdHigh = 0.7f;

	public const float ThresholdVeryHigh = 0.85f;

	public const float IconSize = 30f;

	public const float IconPad = 5f;

	protected override bool IsFrozen
	{
		get
		{
			if (!base.IsFrozen)
			{
				return pawn.Deathresting;
			}
			return true;
		}
	}

	public bool Suspended => IsFrozen;

	public LearningCategory CurCategory
	{
		get
		{
			if (CurLevel < 0.01f)
			{
				return LearningCategory.Empty;
			}
			if (CurLevel < 0.15f)
			{
				return LearningCategory.VeryLow;
			}
			if (CurLevel < 0.3f)
			{
				return LearningCategory.Low;
			}
			if (CurLevel < 0.7f)
			{
				return LearningCategory.Satisfied;
			}
			if (CurLevel < 0.85f)
			{
				return LearningCategory.High;
			}
			return LearningCategory.Extreme;
		}
	}

	public Need_Learning(Pawn pawn)
		: base(pawn)
	{
		threshPercents = new List<float> { 0.15f, 0.3f, 0.7f, 0.85f };
	}

	public void Learn(float amount)
	{
		if (!(amount <= 0f))
		{
			amount = Mathf.Min(amount, 1f - CurLevel);
			curLevelInt += amount;
		}
	}

	public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
	{
		if (pawn.learning == null)
		{
			return;
		}
		List<LearningDesireDef> activeLearningDesires = pawn.learning.ActiveLearningDesires;
		float num = (float)activeLearningDesires.Count * 30f;
		float num2 = rect.xMax - num;
		for (int i = 0; i < activeLearningDesires.Count; i++)
		{
			Rect rect2 = new Rect(num2 + (float)i * 30f, rect.y, 30f, 30f);
			LearningDesireDef learningDesire = activeLearningDesires[i];
			GUI.DrawTexture(rect2.ContractedBy(5f), activeLearningDesires[i].Icon);
			if (Mouse.IsOver(rect2))
			{
				Widgets.DrawHighlight(rect2);
				TooltipHandler.TipRegion(rect2, () => (learningDesire.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + learningDesire.description + "\n\n" + "LearningDesireTooltip".Translate().Colorize(ColoredText.SubtleGrayColor)).ResolveTags(), learningDesire.GetHashCode());
				doTooltip = false;
			}
		}
		base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip, drawLabel);
	}

	public override void NeedInterval()
	{
		if (!IsFrozen)
		{
			CurLevel -= 0.00045000002f;
		}
		if (pawn.ageTracker.canGainGrowthPoints)
		{
			pawn.ageTracker.growthPoints += pawn.ageTracker.GrowthPointsPerDay * 0.0025f;
		}
	}

	public override string GetTipString()
	{
		if (learningActivitiesLineList == null)
		{
			learningActivitiesLineList = DefDatabase<LearningDesireDef>.AllDefsListForReading.Select((LearningDesireDef d) => d.label).ToList().ToLineList("  - ", capitalizeItems: true);
		}
		return (base.LabelCap + ": " + base.CurLevelPercentage.ToStringPercent()).Colorize(ColoredText.TipSectionTitleColor) + "\n" + def.description.Formatted(learningActivitiesLineList.Named("ACTIVITIES"), pawn.Named("PAWN")).Resolve();
	}
}
