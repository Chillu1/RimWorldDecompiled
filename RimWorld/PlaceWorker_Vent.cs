using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Vent : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
			IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
			GenDraw.DrawFieldEdges(new List<IntVec3> { intVec }, Color.white);
			GenDraw.DrawFieldEdges(new List<IntVec3> { intVec2 }, Color.white);
			Room room = intVec2.GetRoom(currentMap);
			Room room2 = intVec.GetRoom(currentMap);
			if (room == null || room2 == null)
			{
				return;
			}
			if (room == room2 && !room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), Color.white);
				return;
			}
			if (!room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), Color.white);
			}
			if (!room2.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room2.Cells.ToList(), Color.white);
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			if (c.Impassable(map) || c2.Impassable(map))
			{
				return "MustPlaceVentWithFreeSpaces".Translate();
			}
			return true;
		}
	}
}
