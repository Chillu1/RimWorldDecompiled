using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShowFacilitiesConnections : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			if (def.HasComp(typeof(CompAffectedByFacilities)))
			{
				CompAffectedByFacilities.DrawLinesToPotentialThingsToLinkTo(def, center, rot, currentMap);
			}
			else
			{
				CompFacility.DrawLinesToPotentialThingsToLinkTo(def, center, rot, currentMap);
			}
		}
	}
}
