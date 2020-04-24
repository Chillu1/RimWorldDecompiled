using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_Heater : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			RoomGroup roomGroup = center.GetRoomGroup(currentMap);
			if (roomGroup != null && !roomGroup.UsesOutdoorTemperature)
			{
				GenDraw.DrawFieldEdges(roomGroup.Cells.ToList(), GenTemperature.ColorRoomHot);
			}
		}
	}
}
