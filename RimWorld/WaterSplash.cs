using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class WaterSplash : Projectile
	{
		protected override void Impact(Thing hitThing)
		{
			base.Impact(hitThing);
			List<Thing> list = new List<Thing>();
			foreach (Thing item in base.Map.thingGrid.ThingsAt(base.Position))
			{
				if (item.def == ThingDefOf.Fire)
				{
					list.Add(item);
				}
			}
			foreach (Thing item2 in list)
			{
				item2.Destroy();
			}
		}
	}
}
