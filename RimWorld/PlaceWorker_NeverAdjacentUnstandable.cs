using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_NeverAdjacentUnstandable : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			GenDraw.DrawFieldEdges(GenAdj.OccupiedRect(center, rot, def.size).ExpandedBy(1).Cells.ToList(), Color.white);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			foreach (IntVec3 item in GenAdj.OccupiedRect(center, rot, def.Size).ExpandedBy(1))
			{
				List<Thing> list = map.thingGrid.ThingsListAt(item);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] != thingToIgnore && list[i].def.passability != 0)
					{
						return "MustPlaceAdjacentStandable".Translate();
					}
				}
			}
			return true;
		}
	}
}
