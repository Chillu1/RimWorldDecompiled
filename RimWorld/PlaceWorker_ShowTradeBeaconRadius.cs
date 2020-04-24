using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowTradeBeaconRadius : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			GenDraw.DrawFieldEdges(Building_OrbitalTradeBeacon.TradeableCellsAround(center, currentMap));
		}
	}
}
