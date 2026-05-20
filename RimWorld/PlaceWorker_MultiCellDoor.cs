using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_MultiCellDoor : PlaceWorker
{
	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		foreach (IntVec3 item in DoorUtility.WallRequirementCells(def, center, rot))
		{
			GhostDrawer.DrawGhostThing(item, Rot4.South, ThingDefOf.Wall, null, Color.grey, AltitudeLayer.Blueprint, null, drawPlaceWorkers: false);
		}
	}

	public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
	{
		if (!(def is ThingDef def2))
		{
			return;
		}
		foreach (IntVec3 item in DoorUtility.WallRequirementCells(def2, loc, rot))
		{
			if (!DoorUtility.EncapsulatingWallAt(item, map, includeUnbuilt: true))
			{
				Messages.Message("MessageBuildingRequiresAdjacentWalls".Translate(def).CapitalizeFirst(), MessageTypeDefOf.CautionInput, historical: false);
				break;
			}
		}
	}
}
