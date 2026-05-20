using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Cooler : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			IntVec3 intVec = center + IntVec3.South.RotatedBy(rot);
			IntVec3 intVec2 = center + IntVec3.North.RotatedBy(rot);
			GenDraw.DrawFieldEdges(new List<IntVec3> { intVec }, GenTemperature.ColorSpotCold);
			GenDraw.DrawFieldEdges(new List<IntVec3> { intVec2 }, GenTemperature.ColorSpotHot);
			Room room = intVec2.GetRoom(currentMap);
			Room room2 = intVec.GetRoom(currentMap);
			if (room == null || room2 == null)
			{
				return;
			}
			if (room == room2 && !room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), new Color(1f, 0.7f, 0f, 0.5f));
				return;
			}
			if (!room.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room.Cells.ToList(), GenTemperature.ColorRoomHot);
			}
			if (!room2.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(room2.Cells.ToList(), GenTemperature.ColorRoomCold);
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 center, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			IntVec3 c = center + IntVec3.South.RotatedBy(rot);
			IntVec3 c2 = center + IntVec3.North.RotatedBy(rot);
			if (c.Impassable(map) || c2.Impassable(map))
			{
				return "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			Frame firstThing = c.GetFirstThing<Frame>(map);
			Frame firstThing2 = c2.GetFirstThing<Frame>(map);
			if ((firstThing != null && firstThing.def.entityDefToBuild != null && firstThing.def.entityDefToBuild.passability == Traversability.Impassable) || (firstThing2 != null && firstThing2.def.entityDefToBuild != null && firstThing2.def.entityDefToBuild.passability == Traversability.Impassable))
			{
				return "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			Blueprint firstThing3 = c.GetFirstThing<Blueprint>(map);
			Blueprint firstThing4 = c2.GetFirstThing<Blueprint>(map);
			if ((firstThing3 != null && firstThing3.def.entityDefToBuild != null && firstThing3.def.entityDefToBuild.passability == Traversability.Impassable) || (firstThing4 != null && firstThing4.def.entityDefToBuild != null && firstThing4.def.entityDefToBuild.passability == Traversability.Impassable))
			{
				return "MustPlaceCoolerWithFreeSpaces".Translate();
			}
			return true;
		}
	}
}
