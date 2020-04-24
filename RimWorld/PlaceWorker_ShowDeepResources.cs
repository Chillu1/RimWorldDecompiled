using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowDeepResources : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			List<Building> allBuildingsColonist = currentMap.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building thing2 = allBuildingsColonist[i];
				if (thing2.TryGetComp<CompDeepScanner>() != null)
				{
					CompPowerTrader compPowerTrader = thing2.TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						currentMap.deepResourceGrid.MarkForDraw();
					}
				}
			}
		}
	}
}
