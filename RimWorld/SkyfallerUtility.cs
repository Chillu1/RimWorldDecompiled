using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class SkyfallerUtility
{
	public static bool CanPossiblyFallOnColonist(ThingDef skyfaller, IntVec3 c, Map map)
	{
		CellRect cellRect = GenAdj.OccupiedRect(c, Rot4.North, skyfaller.size);
		int dist = Mathf.Max(Mathf.CeilToInt(skyfaller.skyfaller.explosionRadius) + 7, 14);
		foreach (IntVec3 item in cellRect.ExpandedBy(dist))
		{
			if (!item.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = item.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn { IsColonist: not false })
				{
					return true;
				}
			}
		}
		return false;
	}
}
