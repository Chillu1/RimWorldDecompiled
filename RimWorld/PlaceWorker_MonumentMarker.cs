using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld;

public class PlaceWorker_MonumentMarker : PlaceWorker
{
	private static List<Thing> tmpMonumentThings = new List<Thing>();

	public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
	{
		if (thing is MonumentMarker monumentMarker)
		{
			monumentMarker.DrawGhost(center, placingMode: true, rot);
		}
	}

	public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
	{
		if (thing is MonumentMarker monumentMarker)
		{
			CellRect rect = monumentMarker.sketch.OccupiedRect.MovedBy(loc);
			Blueprint_Install thingToIgnore2 = monumentMarker.FindMyBlueprint(rect, map);
			foreach (SketchEntity entity in monumentMarker.sketch.Entities)
			{
				CellRect cellRect = entity.OccupiedRect.MovedBy(loc);
				if (!cellRect.InBounds(map))
				{
					return false;
				}
				if (cellRect.InNoBuildEdgeArea(map))
				{
					return "TooCloseToMapEdge".Translate();
				}
				foreach (IntVec3 item in cellRect)
				{
					if (!entity.CanBuildOnTerrain(item, map))
					{
						TerrainDef terrain = item.GetTerrain(map);
						return "CannotPlaceMonumentOnTerrain".Translate(terrain.LabelCap);
					}
				}
			}
			tmpMonumentThings.Clear();
			foreach (SketchBuildable buildable in monumentMarker.sketch.Buildables)
			{
				Thing spawnedBlueprintOrFrame = buildable.GetSpawnedBlueprintOrFrame(loc + buildable.pos, map);
				if (spawnedBlueprintOrFrame != null)
				{
					tmpMonumentThings.Add(spawnedBlueprintOrFrame);
				}
				else if (buildable is SketchThing sketchThing)
				{
					Thing sameSpawned = sketchThing.GetSameSpawned(loc + sketchThing.pos, map);
					if (sameSpawned != null)
					{
						tmpMonumentThings.Add(sameSpawned);
					}
				}
			}
			foreach (SketchEntity entity2 in monumentMarker.sketch.Entities)
			{
				if (entity2.IsSameSpawnedOrBlueprintOrFrame(loc + entity2.pos, map))
				{
					continue;
				}
				foreach (IntVec3 item2 in entity2.OccupiedRect.MovedBy(loc))
				{
					if (item2.InBounds(map))
					{
						Building firstBuilding = item2.GetFirstBuilding(map);
						if ((firstBuilding == null || firstBuilding.def.building?.isPowerConduit != true) && firstBuilding != null && !tmpMonumentThings.Contains(firstBuilding))
						{
							tmpMonumentThings.Clear();
							return "CannotPlaceMonumentOver".Translate(firstBuilding.LabelCap);
						}
					}
				}
				if (entity2 is SketchBuildable sketchBuildable)
				{
					Thing thing2 = sketchBuildable.FirstPermanentBlockerAt(loc + entity2.pos, map);
					if (thing2 != null && !tmpMonumentThings.Contains(thing2))
					{
						tmpMonumentThings.Clear();
						return "CannotPlaceMonumentOver".Translate(thing2.LabelCap);
					}
				}
			}
			foreach (SketchEntity entity3 in monumentMarker.sketch.Entities)
			{
				Building firstAdjacentBuilding = MonumentMarkerUtility.GetFirstAdjacentBuilding(entity3, loc, tmpMonumentThings, map);
				if (firstAdjacentBuilding != null)
				{
					BuildingProperties building = firstAdjacentBuilding.def.building;
					if (building != null && !building.isPowerConduit)
					{
						return "MonumentAdjacentToBuilding".Translate(firstAdjacentBuilding.LabelCap);
					}
				}
				if (entity3.IsSpawningBlockedPermanently(loc + entity3.pos, map, thingToIgnore2))
				{
					tmpMonumentThings.Clear();
					return "MonumentBlockedPermanently".Translate();
				}
			}
			tmpMonumentThings.Clear();
		}
		return true;
	}
}
