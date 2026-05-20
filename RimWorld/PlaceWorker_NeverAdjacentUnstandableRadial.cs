using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_NeverAdjacentUnstandableRadial : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, (float)def.size.x + 0.9f, useCenter: true).ToList(), Color.white);
	}

	public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(center, (float)def.Size.x + 0.9f, useCenter: true))
		{
			if (!item.InBounds(map))
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(item);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != thingToIgnore && (!item.Walkable(map) || list[i].def.passability != Traversability.Standable))
				{
					return "MustPlaceAdjacentStandable".Translate();
				}
			}
		}
		return true;
	}
}
