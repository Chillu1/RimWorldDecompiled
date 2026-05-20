using System.Collections.Generic;
using Verse;

namespace RimWorld;

public class PlaceWorker_PreventInteractionSpotOverlap : PlaceWorker
{
	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thingToPlace = null)
	{
		if (!(checkingDef is ThingDef { HasSingleOrMultipleInteractionCells: not false } thingDef))
		{
			return true;
		}
		List<IntVec3> list = new List<IntVec3>();
		List<IntVec3> list2 = new List<IntVec3>();
		ThingUtility.InteractionCellsWhenAt(list, thingDef, loc, rot, map);
		for (int i = -1; i <= 1; i++)
		{
			for (int j = -1; j <= 1; j++)
			{
				foreach (IntVec3 item in list)
				{
					IntVec3 c = item;
					c.x += i;
					c.z += j;
					if (!c.InBounds(map))
					{
						continue;
					}
					foreach (Thing item2 in map.thingGrid.ThingsListAtFast(c))
					{
						if (item2 == thingToIgnore)
						{
							continue;
						}
						ThingDef thingDef2 = item2.def;
						if (item2.def.entityDefToBuild != null)
						{
							thingDef2 = item2.def.entityDefToBuild as ThingDef;
						}
						if (thingDef2 == null || !thingDef2.HasSingleOrMultipleInteractionCells)
						{
							continue;
						}
						ThingUtility.InteractionCellsWhenAt(list2, thingDef2, item2.Position, item2.Rotation, item2.Map);
						foreach (IntVec3 item3 in list2)
						{
							if (item3 == item)
							{
								return new AcceptanceReport(((item2.def.entityDefToBuild == null) ? "InteractionSpotOverlaps" : "InteractionSpotWillOverlap").Translate(item2.LabelNoCount, item2));
							}
						}
					}
				}
			}
		}
		return true;
	}
}
