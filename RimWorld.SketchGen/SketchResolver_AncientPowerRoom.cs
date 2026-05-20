using System.Linq;
using Verse;

namespace RimWorld.SketchGen;

public class SketchResolver_AncientPowerRoom : SketchResolver
{
	private const float SkipChance = 0.85f;

	private const float OilSmearChance = 0.45f;

	protected override bool CanResolveInt(SketchResolveParams parms)
	{
		if (parms.rect.HasValue)
		{
			return parms.sketch != null;
		}
		return false;
	}

	protected override void ResolveInt(SketchResolveParams parms)
	{
		if (!ModLister.CheckIdeology("Ancient power room"))
		{
			return;
		}
		CellRect value = parms.rect.Value;
		ThingDef ancientGenerator = ThingDefOf.AncientGenerator;
		foreach (IntVec3 cell in value.Cells)
		{
			if (Rand.Chance(0.85f))
			{
				continue;
			}
			CellRect cellRect = GenAdj.OccupiedRect(cell, ancientGenerator.defaultPlacingRot, ancientGenerator.size);
			bool flag = true;
			foreach (IntVec3 item in cellRect.ExpandedBy(1))
			{
				if (parms.sketch.ThingsAt(item).Any())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				parms.sketch.AddThing(ancientGenerator, cell, ancientGenerator.defaultPlacingRot);
				ScatterDebrisUtility.ScatterAround(cell, ancientGenerator.size, Rot4.North, parms.sketch, ThingDefOf.Filth_OilSmear, 0.45f);
			}
		}
		SketchResolveParams parms2 = parms;
		parms2.thingCentral = ancientGenerator;
		SketchResolverDefOf.AddThingsCentral.Resolve(parms2);
	}
}
