using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class ChemicalDef : Def
{
	public HediffDef addictionHediff;

	public HediffDef toleranceHediff;

	public bool canBinge = true;

	public bool canBeAddicted = true;

	public bool generateAddictionGenes = true;

	public float geneOverdoseChanceFactorResist = 1f;

	public float geneOverdoseChanceFactorImmune = 1f;

	public float geneToleranceBuildupFactorResist = 1f;

	public float geneToleranceBuildupFactorImmune = 1f;

	public float onGeneratedAddictedToleranceChance;

	public List<HediffGiver_Event> onGeneratedAddictedEvents;

	public override IEnumerable<string> ConfigErrors()
	{
		foreach (string item in base.ConfigErrors())
		{
			yield return item;
		}
		if (canBeAddicted && addictionHediff == null)
		{
			yield return "addictionHediff is null";
		}
	}
}
