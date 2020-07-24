using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PlaceWorker_MonumentMarker : PlaceWorker
	{
		private static List<Thing> tmpMonumentThings = new List<Thing>();

		public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			(thing as MonumentMarker)?.DrawGhost_NewTmp(center, placingMode: true, rot);
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			MonumentMarker monumentMarker = thing as MonumentMarker;
			if (monumentMarker != null)
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
							return "MonumentBadTerrain".Translate();
						}
					}
					if (entity.IsSpawningBlockedPermanently(loc + entity.pos, map, thingToIgnore2))
					{
						return "MonumentBlockedPermanently".Translate();
					}
				}
				tmpMonumentThings.Clear();
				foreach (SketchBuildable buildable in monumentMarker.sketch.Buildables)
				{
					Thing spawnedBlueprintOrFrame = buildable.GetSpawnedBlueprintOrFrame(loc + buildable.pos, map);
					SketchThing sketchThing;
					if (spawnedBlueprintOrFrame != null)
					{
						tmpMonumentThings.Add(spawnedBlueprintOrFrame);
					}
					else if ((sketchThing = (buildable as SketchThing)) != null)
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
							if (firstBuilding != null && !tmpMonumentThings.Contains(firstBuilding))
							{
								tmpMonumentThings.Clear();
								return "MonumentOverlapsBuilding".Translate();
							}
						}
					}
				}
				foreach (SketchEntity entity3 in monumentMarker.sketch.Entities)
				{
					if (entity3.IsSameSpawnedOrBlueprintOrFrame(loc + entity3.pos, map))
					{
						continue;
					}
					foreach (IntVec3 edgeCell in entity3.OccupiedRect.MovedBy(loc).ExpandedBy(1).EdgeCells)
					{
						if (edgeCell.InBounds(map))
						{
							Building firstBuilding2 = edgeCell.GetFirstBuilding(map);
							if (firstBuilding2 != null && !tmpMonumentThings.Contains(firstBuilding2) && (firstBuilding2.Faction == null || firstBuilding2.Faction == Faction.OfPlayer))
							{
								tmpMonumentThings.Clear();
								return "MonumentAdjacentToBuilding".Translate();
							}
						}
					}
				}
				tmpMonumentThings.Clear();
			}
			return true;
		}
	}
}
