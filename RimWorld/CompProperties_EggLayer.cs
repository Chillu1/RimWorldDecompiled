using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class CompProperties_EggLayer : CompProperties
{
	public float eggLayIntervalDays = 1f;

	public IntRange eggCountRange = IntRange.One;

	public ThingDef eggUnfertilizedDef;

	public ThingDef eggFertilizedDef;

	public int eggFertilizationCountMax = 1;

	public bool eggLayFemaleOnly = true;

	public float eggProgressUnfertilizedMax = 1f;

	public CompProperties_EggLayer()
	{
		compClass = typeof(CompEggLayer);
	}

	public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
	{
		if (eggFertilizedDef == null)
		{
			yield return "eggFertilizedDef is null";
		}
		else if (eggUnfertilizedDef == null)
		{
			yield return "eggUnfertilizedDef is null";
		}
		foreach (string item in base.ConfigErrors(parentDef))
		{
			yield return item;
		}
	}
}
