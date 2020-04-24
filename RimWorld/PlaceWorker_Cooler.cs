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
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec
			}, GenTemperature.ColorSpotCold);
			GenDraw.DrawFieldEdges(new List<IntVec3>
			{
				intVec2
			}, GenTemperature.ColorSpotHot);
			RoomGroup roomGroup = intVec2.GetRoomGroup(currentMap);
			RoomGroup roomGroup2 = intVec.GetRoomGroup(currentMap);
			if (roomGroup == null || roomGroup2 == null)
			{
				return;
			}
			if (roomGroup == roomGroup2 && !roomGroup.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), new Color(1f, 0.7f, 0f, 0.5f));
				return;
			}
			if (!roomGroup.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), GenTemperature.ColorRoomHot);
			}
			if (!roomGroup2.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(roomGroup2.Cells.ToList(), GenTemperature.ColorRoomCold);
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
