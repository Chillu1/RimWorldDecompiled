using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_ShipLandingBeacon : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			Map currentMap = Find.CurrentMap;
			if (def.HasComp(typeof(CompShipLandingBeacon)))
			{
				ShipLandingBeaconUtility.DrawLinesToNearbyBeacons(def, center, rot, currentMap, thing);
			}
		}
	}
}
