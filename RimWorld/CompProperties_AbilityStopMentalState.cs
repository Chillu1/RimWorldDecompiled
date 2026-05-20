using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld;

public class CompProperties_AbilityStopMentalState : CompProperties_AbilityEffect
{
	public List<MentalStateDef> exceptions;

	public float psyfocusCostForMinor = -1f;

	public float psyfocusCostForMajor = -1f;

	public float psyfocusCostForExtreme = -1f;

	public override bool OverridesPsyfocusCost => true;

	public override string PsyfocusCostExplanation
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder("PsyfocusCostPerMentalBreakIntensity".Translate() + ":");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("  - " + "MentalBreakIntensityMinor".Translate().CapitalizeFirst() + ": " + psyfocusCostForMinor.ToStringPercent());
			stringBuilder.AppendLine("  - " + "MentalBreakIntensityMajor".Translate().CapitalizeFirst() + ": " + psyfocusCostForMajor.ToStringPercent());
			stringBuilder.AppendLine("  - " + "MentalBreakIntensityExtreme".Translate().CapitalizeFirst() + ": " + psyfocusCostForExtreme.ToStringPercent());
			return stringBuilder.ToString();
		}
	}

	public override FloatRange PsyfocusCostRange => new FloatRange(psyfocusCostForMinor, psyfocusCostForExtreme);

	public CompProperties_AbilityStopMentalState()
	{
		compClass = typeof(CompAbilityEffect_StopMentalState);
	}

	public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (psyfocusCostForMinor < 0f)
		{
			yield return "psyfocusCostForMinor must be defined ";
		}
		if (psyfocusCostForMajor < 0f)
		{
			yield return "psyfocusCostForMajor must be defined ";
		}
		if (psyfocusCostForExtreme < 0f)
		{
			yield return "psyfocusCostForExtreme must be defined ";
		}
	}
}
