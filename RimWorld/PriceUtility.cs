using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PriceUtility
{
	private const float MinFactor = 0.1f;

	private const float SummaryHealthImpact = 0.8f;

	private const float CapacityImpact = 0.5f;

	private const float MissingCapacityFactor = 0.6f;

	private static readonly SimpleCurve AverageSkillCurve = new SimpleCurve
	{
		new CurvePoint(0f, 0.2f),
		new CurvePoint(5.5f, 1f),
		new CurvePoint(20f, 3f)
	};

	public static float PawnQualityPriceFactor(Pawn pawn, StringBuilder explanation = null)
	{
		float num = 1f;
		num *= Mathf.Lerp(0.19999999f, 1f, pawn.health.summaryHealth.SummaryHealthPercent);
		List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
		for (int i = 0; i < allDefsListForReading.Count; i++)
		{
			if (!pawn.health.capacities.CapableOf(allDefsListForReading[i]))
			{
				num *= 0.6f;
				continue;
			}
			float t = PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, allDefsListForReading[i], null, forTradePrice: true);
			num *= Mathf.Lerp(0.5f, 1f, t);
		}
		if (pawn.skills != null)
		{
			num *= AverageSkillCurve.Evaluate(((IEnumerable<SkillRecord>)pawn.skills.skills).Average((Func<SkillRecord, float>)((SkillRecord sk) => sk.Level)));
		}
		num *= pawn.ageTracker.CurLifeStage.marketValueFactor;
		if (pawn.story != null && pawn.story.traits != null)
		{
			for (int num2 = 0; num2 < pawn.story.traits.allTraits.Count; num2++)
			{
				Trait trait = pawn.story.traits.allTraits[num2];
				if (!trait.Suppressed)
				{
					num += trait.CurrentData.marketValueFactorOffset;
				}
			}
		}
		num += pawn.GetStatValue(StatDefOf.PawnBeauty) * 0.2f;
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		explanation?.AppendLine("StatsReport_CharacterQuality".Translate() + ": x" + num.ToStringPercent());
		return num;
	}

	public static float PawnQualityPriceOffset(Pawn pawn, StringBuilder explanation = null)
	{
		float num = 0f;
		List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
		for (int i = 0; i < hediffs.Count; i++)
		{
			if (hediffs[i].def.priceImpact || hediffs[i].def.priceOffset != 0f)
			{
				float num2 = hediffs[i].def.priceOffset;
				if (num2 == 0f && hediffs[i].def.spawnThingOnRemoved != null)
				{
					num2 = hediffs[i].def.spawnThingOnRemoved.BaseMarketValue;
				}
				if (!(num2 < 1f) || !(num2 > -1f))
				{
					num += num2;
					explanation?.AppendLine(hediffs[i].LabelBaseCap + ": " + num2.ToStringMoneyOffset());
				}
			}
		}
		return num;
	}
}
