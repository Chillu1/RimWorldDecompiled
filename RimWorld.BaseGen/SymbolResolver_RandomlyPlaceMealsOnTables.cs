using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_RandomlyPlaceMealsOnTables : SymbolResolver
{
	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		ThingDef thingDef = ((rp.faction == null || !rp.faction.def.techLevel.IsNeolithicOrWorse()) ? ThingDefOf.MealSimple : ThingDefOf.Pemmican);
		foreach (IntVec3 item in rp.rect)
		{
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.IsTable && Rand.Chance(0.15f))
				{
					int value = Mathf.Clamp(Mathf.RoundToInt(ThingDefOf.MealSimple.ingestible.CachedNutrition * Rand.Range(0.9f, 1.1f) / thingDef.ingestible.CachedNutrition), 1, thingDef.stackLimit);
					ResolveParams resolveParams = rp;
					resolveParams.rect = CellRect.SingleCell(item);
					resolveParams.singleThingDef = thingDef;
					resolveParams.singleThingStackCount = value;
					BaseGen.symbolStack.Push("thing", resolveParams);
				}
			}
		}
	}
}
