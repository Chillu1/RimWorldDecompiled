using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_ShowDeepResources : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		Map currentMap = Find.CurrentMap;
		if (currentMap.deepResourceGrid.AnyActiveDeepScannersOnMap())
		{
			currentMap.deepResourceGrid.MarkForDraw();
		}
	}
}
