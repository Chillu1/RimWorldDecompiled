using System.Collections.Generic;
using Verse;

namespace RimWorld;

public static class MonumentMarkerUtility
{
	public static Building GetFirstAdjacentBuilding(SketchEntity entity, IntVec3 offset, List<Thing> monumentThings, Map map)
	{
		if (entity.IsSameSpawnedOrBlueprintOrFrame(entity.pos + offset, map))
		{
			return null;
		}
		foreach (IntVec3 edgeCell in entity.OccupiedRect.MovedBy(offset).ExpandedBy(1).EdgeCells)
		{
			if (edgeCell.InBounds(map))
			{
				Building firstBuilding = edgeCell.GetFirstBuilding(map);
				if (firstBuilding != null && !monumentThings.Contains(firstBuilding) && (firstBuilding.Faction == null || firstBuilding.Faction == Faction.OfPlayer))
				{
					return firstBuilding;
				}
			}
		}
		return null;
	}
}
