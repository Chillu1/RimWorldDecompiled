using System.Collections.Generic;
using UnityEngine;

namespace Verse;

public class HediffGiver_Birthday : HediffGiver
{
	public SimpleCurve ageFractionChanceCurve;

	public float averageSeverityPerDayBeforeGeneration;

	private static List<Hediff> addedHediffs = new List<Hediff>();

	public void TryApplyAndSimulateSeverityChange(Pawn pawn, float gotAtAge, bool tryNotToKillPawn)
	{
		addedHediffs.Clear();
		if (!TryApply(pawn, addedHediffs))
		{
			return;
		}
		if (averageSeverityPerDayBeforeGeneration != 0f)
		{
			float num = (pawn.ageTracker.AgeBiologicalYearsFloat - gotAtAge) * 60f;
			if (num < 0f)
			{
				Log.Error("daysPassed < 0, pawn=" + pawn?.ToString() + ", gotAtAge=" + gotAtAge);
				return;
			}
			for (int i = 0; i < addedHediffs.Count; i++)
			{
				SimulateSeverityChange(pawn, addedHediffs[i], num, tryNotToKillPawn);
			}
		}
		addedHediffs.Clear();
	}

	private void SimulateSeverityChange(Pawn pawn, Hediff hediff, float daysPassed, bool tryNotToKillPawn)
	{
		float num = averageSeverityPerDayBeforeGeneration * daysPassed;
		num *= Rand.Range(0.5f, 1.4f);
		num += hediff.def.initialSeverity;
		if (tryNotToKillPawn)
		{
			AvoidLifeThreateningStages(ref num, hediff.def.stages);
		}
		hediff.Severity = num;
		pawn.health.Notify_HediffChanged(hediff);
	}

	private void AvoidLifeThreateningStages(ref float severity, List<HediffStage> stages)
	{
		if (stages.NullOrEmpty())
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < stages.Count; i++)
		{
			if (stages[i].lifeThreatening)
			{
				num = i;
				break;
			}
		}
		if (num >= 0)
		{
			if (num == 0)
			{
				severity = Mathf.Min(severity, stages[num].minSeverity);
			}
			else
			{
				severity = Mathf.Min(severity, (stages[num].minSeverity + stages[num - 1].minSeverity) / 2f);
			}
		}
	}

	public float DebugChanceToHaveAtAge(Pawn pawn, int age)
	{
		float num = 1f;
		for (int i = 1; i <= age; i++)
		{
			float x = (float)i / pawn.RaceProps.lifeExpectancy;
			num *= 1f - ageFractionChanceCurve.Evaluate(x);
		}
		return 1f - num;
	}
}
