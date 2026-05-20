using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen;

public class SymbolResolver_AddWortToFermentingBarrels : SymbolResolver
{
	private static List<Building_FermentingBarrel> barrels = new List<Building_FermentingBarrel>();

	public override void Resolve(ResolveParams rp)
	{
		Map map = BaseGen.globalSettings.map;
		barrels.Clear();
		foreach (IntVec3 item2 in rp.rect)
		{
			List<Thing> thingList = item2.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Building_FermentingBarrel item && !barrels.Contains(item))
				{
					barrels.Add(item);
				}
			}
		}
		float progress = Rand.Range(0.1f, 0.9f);
		for (int j = 0; j < barrels.Count; j++)
		{
			if (!barrels[j].Fermented)
			{
				int a = Rand.RangeInclusive(1, 25);
				a = Mathf.Min(a, barrels[j].SpaceLeftForWort);
				if (a > 0)
				{
					barrels[j].AddWort(a);
					barrels[j].Progress = progress;
				}
			}
		}
		barrels.Clear();
	}
}
