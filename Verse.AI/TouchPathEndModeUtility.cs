using System;
using System.Collections.Generic;

namespace Verse.AI;

public static class TouchPathEndModeUtility
{
	[Obsolete("Use IsCornerTouchAllowed_NewTemp()")]
	public static bool IsCornerTouchAllowed(IntVec3 corner, IntVec3 adjCardinalX, IntVec3 adjCardinalZ, PathingContext pc, bool forEdifice)
	{
		return IsCornerTouchAllowed_NewTemp(adjCardinalX, adjCardinalZ, pc, forEdifice ? pc.map.edificeGrid[corner] : null);
	}

	public static bool IsCornerTouchAllowed_NewTemp(IntVec3 adjCardinalX, IntVec3 adjCardinalZ, PathingContext pc, Thing target = null)
	{
		if (target != null && (target.IsConsideredEdificeForCornerTouch() || MakesOccupiedCellsAlwaysReachableDiagonally(target.def)))
		{
			return true;
		}
		if ((pc.pathGrid.Walkable(adjCardinalX) && adjCardinalX.GetDoor(pc.map) == null) || (pc.pathGrid.Walkable(adjCardinalZ) && adjCardinalZ.GetDoor(pc.map) == null))
		{
			return true;
		}
		return false;
	}

	public static bool MakesOccupiedCellsAlwaysReachableDiagonally(ThingDef def)
	{
		return (def.IsFrame ? (def.entityDefToBuild as ThingDef) : def)?.CanInteractThroughCorners ?? false;
	}

	public static bool IsAdjacentCornerAndNotAllowed(IntVec3 corner, IntVec3 BL, IntVec3 TL, IntVec3 TR, IntVec3 BR, PathingContext pc, Thing forThing)
	{
		IntVec3 intVec = new IntVec3(1, 0, 0);
		IntVec3 intVec2 = new IntVec3(0, 0, 1);
		if (corner == BL && !IsCornerTouchAllowed_NewTemp(BL + intVec2, BL + intVec, pc, forThing))
		{
			return true;
		}
		if (corner == TL && !IsCornerTouchAllowed_NewTemp(TL - intVec2, TL + intVec, pc, forThing))
		{
			return true;
		}
		if (corner == TR && !IsCornerTouchAllowed_NewTemp(TR - intVec2, TR - intVec, pc, forThing))
		{
			return true;
		}
		if (corner == BR && !IsCornerTouchAllowed_NewTemp(BR + intVec2, BR - intVec, pc, forThing))
		{
			return true;
		}
		return false;
	}

	public static void AddAllowedAdjacentRegions(LocalTargetInfo dest, TraverseParms traverseParams, Map map, List<Region> regions)
	{
		GenAdj.GetAdjacentCorners(dest, out var BL, out var TL, out var TR, out var BR);
		PathingContext pc = map.pathing.For(traverseParams);
		if (!dest.HasThing || (dest.Thing.def.size.x == 1 && dest.Thing.def.size.z == 1))
		{
			IntVec3 cell = dest.Cell;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = GenAdj.AdjacentCells[i] + cell;
				if (intVec.InBounds(map) && !IsAdjacentCornerAndNotAllowed(intVec, BL, TL, TR, BR, pc, dest.Thing))
				{
					Region region = intVec.GetRegion(map);
					if (region != null && region.Allows(traverseParams, isDestination: true))
					{
						regions.Add(region);
					}
				}
			}
			return;
		}
		List<IntVec3> list = GenAdjFast.AdjacentCells8Way(dest);
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].InBounds(map) && !IsAdjacentCornerAndNotAllowed(list[j], BL, TL, TR, BR, pc, dest.Thing))
			{
				Region region2 = list[j].GetRegion(map);
				if (region2 != null && region2.Allows(traverseParams, isDestination: true))
				{
					regions.Add(region2);
				}
			}
		}
	}

	public static bool IsAdjacentOrInsideAndAllowedToTouch(IntVec3 root, LocalTargetInfo target, PathingContext pc)
	{
		GenAdj.GetAdjacentCorners(target, out var BL, out var TL, out var TR, out var BR);
		if (root.AdjacentTo8WayOrInside(target))
		{
			return !IsAdjacentCornerAndNotAllowed(root, BL, TL, TR, BR, pc, target.Thing);
		}
		return false;
	}

	private static bool IsConsideredEdificeForCornerTouch(this Thing thing)
	{
		if (thing == null)
		{
			return false;
		}
		ThingDef thingDef = ((thing.def.IsFrame || thing.def.IsBlueprint) ? (thing.def.entityDefToBuild as ThingDef) : thing.def);
		if (thingDef == null)
		{
			return false;
		}
		if (!thingDef.IsEdifice() || !thingDef.holdsRoof)
		{
			return thingDef.building?.canConstructInCorner ?? false;
		}
		return true;
	}
}
