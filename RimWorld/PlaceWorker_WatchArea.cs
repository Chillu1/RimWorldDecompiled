using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_WatchArea : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			GenDraw.DrawFieldEdges(WatchBuildingUtility.CalculateWatchCells(def, center, rot, currentMap).ToList());
		}
	}
}
