using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Interior_GravCore : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		ResolveParams resolveParams = rp;
		if (rp.sitePart != null)
		{
			resolveParams.singleThingToSpawn = rp.sitePart.things.FirstOrDefault((Thing t) => t.def == ThingDefOf.Gravcore);
		}
		if (resolveParams.singleThingToSpawn == null)
		{
			resolveParams.singleThingToSpawn = ThingMaker.MakeThing(ThingDefOf.Gravcore);
		}
		resolveParams.rect = CellRect.CenteredOn(rp.rect.CenterCell, 1, 1);
		BaseGen.symbolStack.Push("thing", resolveParams);
		if (rp.sitePart == null)
		{
			return;
		}
		foreach (Thing item in (IEnumerable<Thing>)rp.sitePart.things)
		{
			if (item.def == ThingDefOf.GravlitePanel)
			{
				ResolveParams resolveParams2 = rp;
				resolveParams2.singleThingToSpawn = item;
				resolveParams2.rect = rp.rect;
				BaseGen.symbolStack.Push("thing", resolveParams2);
			}
		}
	}
}
