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
			(thing as MonumentMarker)?.DrawGhost(center, placingMode: true);
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
					if (entity.IsSpawningBlockedPermanently(entity.pos + loc, map, thingToIgnore2))
					{
						return false;
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
					if (!entity2.IsSameSpawnedOrBlueprintOrFrame(loc + entity2.pos, map))
					{
						foreach (IntVec3 item in entity2.OccupiedRect.MovedBy(loc).ExpandedBy(1))
						{
							if (item.InBounds(map))
							{
								Building edifice = item.GetEdifice(map);
								if (edifice != null && !tmpMonumentThings.Contains(edifice))
								{
									tmpMonumentThings.Clear();
									return "MonumentAdjacentToBuilding".Translate();
								}
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
