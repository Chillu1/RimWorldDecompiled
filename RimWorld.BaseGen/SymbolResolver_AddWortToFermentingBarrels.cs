using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AddWortToFermentingBarrels : SymbolResolver
	{
		private static List<Building_FermentingBarrel> barrels = new List<Building_FermentingBarrel>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			barrels.Clear();
			foreach (IntVec3 item in rp.rect)
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Building_FermentingBarrel building_FermentingBarrel = thingList[i] as Building_FermentingBarrel;
					if (building_FermentingBarrel != null && !barrels.Contains(building_FermentingBarrel))
					{
						barrels.Add(building_FermentingBarrel);
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
}
