using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class CompProperties_AbilityHemogenCost : CompProperties_AbilityEffect
{
	public float hemogenCost;

	public CompProperties_AbilityHemogenCost()
	{
		compClass = typeof(CompAbilityEffect_HemogenCost);
	}

	public override IEnumerable<string> ExtraStatSummary()
	{
		yield return string.Concat("AbilityHemogenCost".Translate() + ": ", Mathf.RoundToInt(hemogenCost * 100f).ToString());
	}
}
