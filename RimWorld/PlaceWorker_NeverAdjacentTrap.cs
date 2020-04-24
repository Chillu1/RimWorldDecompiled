using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeverAdjacentTrap : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
			{
				List<Thing> list = map.thingGrid.ThingsListAt(item);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing2 = list[i];
					if (thing2 != thingToIgnore && ((thing2.def.category == ThingCategory.Building && thing2.def.building.isTrap) || ((thing2.def.IsBlueprint || thing2.def.IsFrame) && thing2.def.entityDefToBuild is ThingDef && ((ThingDef)thing2.def.entityDefToBuild).building.isTrap)))
					{
						return "CannotPlaceAdjacentTrap".Translate();
					}
				}
			}
			return true;
		}
	}
}
