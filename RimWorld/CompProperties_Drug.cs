using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_Drug : CompProperties
{
	public ChemicalDef chemical;

	public float addictiveness;

	public float minToleranceToAddict;

	public float existingAddictionSeverityOffset = 0.1f;

	public float needLevelOffset = 1f;

	public FloatRange overdoseSeverityOffset = FloatRange.Zero;

	public float largeOverdoseChance;

	public bool isCombatEnhancingDrug;

	public bool teetotalerCanConsume;

	public float listOrder;

	public bool Addictive => addictiveness > 0f;

	public bool CanCauseOverdose => overdoseSeverityOffset.TrueMax > 0f;

	public CompProperties_Drug()
	{
		compClass = typeof(CompDrug);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
		if (Addictive && chemical == null)
		{
			yield return "addictive but chemical is null";
		}
	}
}
