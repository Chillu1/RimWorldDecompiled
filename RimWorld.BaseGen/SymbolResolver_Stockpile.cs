using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_Stockpile : SymbolResolver
{
	private List<IntVec3> cells = new List<IntVec3>();

	private const float FreeCellsFraction = 0.45f;

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		if (rp.stockpileConcreteContents != null)
		{
			CalculateFreeCells(rp.rect, 0f);
			int num = 0;
			int num2 = rp.stockpileConcreteContents.Count - 1;
			while (num2 >= 0 && num < cells.Count)
			{
				Thing thing = GenSpawn.Spawn(rp.stockpileConcreteContents[num2], cells[num], map);
				if (thing != null && thing.def.category == ThingCategory.Item)
				{
					thing.SetForbidden(value: true, warnOnFail: false);
				}
				num++;
				num2--;
			}
			for (int num3 = rp.stockpileConcreteContents.Count - 1; num3 >= 0; num3--)
			{
				if (!rp.stockpileConcreteContents[num3].Spawned)
				{
					rp.stockpileConcreteContents[num3].Destroy();
				}
			}
			rp.stockpileConcreteContents.Clear();
			return;
		}
		CalculateFreeCells(rp.rect, 0.45f);
		ThingSetMakerDef thingSetMakerDef = rp.thingSetMakerDef ?? ThingSetMakerDefOf.MapGen_DefaultStockpile;
		ThingSetMakerParams value;
		if (rp.thingSetMakerParams.HasValue)
		{
			value = rp.thingSetMakerParams.Value;
		}
		else
		{
			value = new ThingSetMakerParams
			{
				techLevel = ((rp.faction != null) ? rp.faction.def.techLevel : TechLevel.Undefined),
				makingFaction = rp.faction,
				validator = (ThingDef x) => (rp.faction == null || (int)x.techLevel >= (int)rp.faction.def.techLevel || !x.IsWeapon || !(x.GetStatValueAbstract(StatDefOf.MarketValue, GenStuff.DefaultStuffFor(x)) < 100f)) ? true : false
			};
			float num4 = rp.stockpileMarketValue ?? Mathf.Min((float)cells.Count * 130f, 1800f);
			value.totalMarketValueRange = new FloatRange(num4, num4);
		}
		if (!value.countRange.HasValue)
		{
			value.countRange = new IntRange(cells.Count, cells.Count);
		}
		ResolveParams resolveParams = rp;
		resolveParams.thingSetMakerDef = thingSetMakerDef;
		resolveParams.thingSetMakerParams = value;
		BaseGen.symbolStack.Push("thingSet", resolveParams);
	}

	private void CalculateFreeCells(CellRect rect, float freeCellsFraction)
	{
		Map map = BaseGen.globalSettings.map;
		cells.Clear();
		foreach (IntVec3 item in rect)
		{
			if (item.Standable(map) && item.GetFirstItem(map) == null)
			{
				cells.Add(item);
			}
		}
		int num = (int)(freeCellsFraction * (float)cells.Count);
		for (int i = 0; i < num; i++)
		{
			cells.RemoveAt(Rand.Range(0, cells.Count));
		}
		cells.Shuffle();
	}
}
