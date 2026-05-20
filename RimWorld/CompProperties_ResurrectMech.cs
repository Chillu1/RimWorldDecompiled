using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_ResurrectMech : CompProperties_AbilityEffect
{
	public int maxCorpseAgeTicks = int.MaxValue;

	public List<MechChargeCosts> costs = new List<MechChargeCosts>();

	public EffecterDef appliedEffecterDef;

	public EffecterDef centerEffecterDef;

	public CompProperties_ResurrectMech()
	{
		compClass = typeof(CompAbilityEffect_ResurrectMech);
	}

	public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (costs.NullOrEmpty())
		{
			yield return "costs list is null";
			yield break;
		}
		foreach (MechChargeCosts cost in costs)
		{
			if (cost.weightClass == null)
			{
				yield return $"costs list contains null weight class with cost {cost.cost}";
			}
		}
	}
}
