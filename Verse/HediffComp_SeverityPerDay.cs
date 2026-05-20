using UnityEngine;

namespace Verse;

public class HediffComp_SeverityPerDay : HediffComp_SeverityModifierBase
{
	public float severityPerDay;

	private HediffCompProperties_SeverityPerDay Props => (HediffCompProperties_SeverityPerDay)props;

	public override string CompLabelInBracketsExtra
	{
		get
		{
			if (Props.showHoursToRecover && SeverityChangePerDay() < 0f)
			{
				return Mathf.RoundToInt(parent.Severity / Mathf.Abs(SeverityChangePerDay()) * 24f).ToString() + "LetterHour".Translate();
			}
			return null;
		}
	}

	public override string CompTipStringExtra
	{
		get
		{
			if (Props.showDaysToRecover && SeverityChangePerDay() < 0f)
			{
				return "DaysToRecover".Translate((parent.Severity / Mathf.Abs(SeverityChangePerDay())).ToString("0.0")).Resolve();
			}
			return null;
		}
	}

	public override void CompPostPostAdd(DamageInfo? dinfo)
	{
		base.CompPostPostAdd(dinfo);
		severityPerDay = Props.CalculateSeverityPerDay();
	}

	public override void CompExposeData()
	{
		base.CompExposeData();
		Scribe_Values.Look(ref severityPerDay, "severityPerDay", 0f);
		if (Scribe.mode == LoadSaveMode.PostLoadInit && severityPerDay == 0f && Props.severityPerDay != 0f && Props.severityPerDayRange == FloatRange.Zero)
		{
			severityPerDay = Props.CalculateSeverityPerDay();
			Log.Warning("Hediff " + parent.Label + " had severityPerDay not matching parent.");
		}
	}

	public override float SeverityChangePerDay()
	{
		if (base.Pawn.ageTracker.AgeBiologicalYearsFloat < Props.minAge)
		{
			return 0f;
		}
		float num = severityPerDay * (parent.CurStage?.severityGainFactor ?? 1f);
		if (ModsConfig.BiotechActive && MechanitorUtility.IsMechanitor(base.Pawn))
		{
			num *= Props.mechanitorFactor;
		}
		return num;
	}
}
