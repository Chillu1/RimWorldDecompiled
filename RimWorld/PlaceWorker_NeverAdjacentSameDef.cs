using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PlaceWorker_NeverAdjacentSameDef : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(item);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing2 = list[i];
				if (thing2 != thingToIgnore && ((thing2.def.category == ThingCategory.Building && thing2.def == def) || ((thing2.def.IsBlueprint || thing2.def.IsFrame) && thing2.def.entityDefToBuild is ThingDef thingDef && thingDef == def)))
				{
					return "CannotPlaceAdjacentSameDef".Translate();
				}
			}
		}
		return true;
	}
}
