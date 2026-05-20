using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class ThingSetMaker_Books : ThingSetMaker
{
	private float marketValueFactor = 1f;

	protected override bool CanGenerateSub(ThingSetMakerParams parms)
	{
		if (parms.countRange.HasValue && parms.countRange.Value.max <= 0)
		{
			return false;
		}
		ThingDef result;
		return TechprintUtility.TryGetTechprintDefToGenerate_NewTemp(parms.makingFaction, out result, null, (parms.totalMarketValueRange?.max * marketValueFactor) ?? float.MaxValue);
	}

	protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
	{
		int num = 1;
		if (parms.countRange.HasValue)
		{
			num = Mathf.Max(parms.countRange.Value.RandomInRange, num);
		}
		for (int i = 0; i < num; i++)
		{
			outThings.Add(BookUtility.MakeBook(ArtGenerationContext.Outsider, parms.qualityGenerator));
		}
	}

	protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
	{
		return DefDatabase<ThingDef>.AllDefs.Where((ThingDef x) => x.HasComp<CompBook>());
	}
}
