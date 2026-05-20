using RimWorld;
using Verse.AI;

namespace Verse;

public static class PathUtility
{
	private const int Cost_DoorToBash = 300;

	private const int Cost_FenceToBash = 300;

	public static Area GetAllowedArea(Pawn pawn)
	{
		if (pawn != null && pawn.playerSettings != null && !pawn.Drafted && ForbidUtility.CaresAboutForbidden(pawn, cellTarget: true))
		{
			Area area = pawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
			if (area != null && area.TrueCount <= 0)
			{
				area = null;
			}
			return area;
		}
		return null;
	}

	public static bool IsDestroyable(Thing thing)
	{
		if (thing.def.useHitPoints)
		{
			return thing.def.destroyable;
		}
		return false;
	}

	public static ushort GetDoorCost(Building_Door door, TraverseParms traverseParms, Pawn pawn, PathFinderCostTuning? tuningParams)
	{
		PathFinderCostTuning pathFinderCostTuning = tuningParams ?? PathFinderCostTuning.DefaultTuning;
		switch (traverseParms.mode)
		{
		case TraverseMode.NoPassClosedDoors:
		case TraverseMode.NoPassClosedDoorsOrWater:
			if (door.FreePassage)
			{
				return 0;
			}
			return ushort.MaxValue;
		case TraverseMode.PassAllDestroyableThings:
		case TraverseMode.PassAllDestroyablePlayerOwnedThings:
		case TraverseMode.PassAllDestroyableThingsNotWater:
			if (pawn != null && door.PawnCanOpen(pawn) && !door.IsForbiddenToPass(pawn) && !door.FreePassage)
			{
				return (ushort)door.TicksToOpenNow;
			}
			if ((pawn != null && door.CanPhysicallyPass(pawn)) || door.FreePassage)
			{
				return 0;
			}
			if (traverseParms.mode == TraverseMode.PassAllDestroyablePlayerOwnedThings && door.Faction != null && !door.Faction.IsPlayer)
			{
				return ushort.MaxValue;
			}
			return (ushort)((float)pathFinderCostTuning.costBlockedDoor + (float)door.HitPoints * pathFinderCostTuning.costBlockedWallExtraPerHitPoint);
		case TraverseMode.PassDoors:
			if (pawn != null && door.PawnCanOpen(pawn) && !door.IsForbiddenToPass(pawn) && !door.FreePassage)
			{
				return (ushort)door.TicksToOpenNow;
			}
			if ((pawn != null && door.CanPhysicallyPass(pawn)) || door.FreePassage)
			{
				return 0;
			}
			return 150;
		case TraverseMode.ByPawn:
			if (!traverseParms.canBashDoors && door.IsForbiddenToPass(pawn))
			{
				return ushort.MaxValue;
			}
			if (door.PawnCanOpen(pawn) && !door.FreePassage)
			{
				return (ushort)door.TicksToOpenNow;
			}
			if (door.CanPhysicallyPass(pawn))
			{
				return 0;
			}
			if (traverseParms.canBashDoors)
			{
				return 300;
			}
			return ushort.MaxValue;
		default:
			return 0;
		}
	}

	public static ushort GetBuildingCost(Building b, TraverseParms traverseParms, Pawn pawn, PathFinderCostTuning? tuningParms)
	{
		PathFinderCostTuning obj = tuningParms ?? PathFinderCostTuning.DefaultTuning;
		int costBlockedDoor = obj.costBlockedDoor;
		float costBlockedDoorPerHitPoint = obj.costBlockedDoorPerHitPoint;
		if (b is Building_Door door)
		{
			return GetDoorCost(door, traverseParms, pawn, tuningParms);
		}
		if (b.def.IsFence && traverseParms.fenceBlocked)
		{
			switch (traverseParms.mode)
			{
			case TraverseMode.ByPawn:
				if (traverseParms.canBashFences)
				{
					return 300;
				}
				return ushort.MaxValue;
			case TraverseMode.PassAllDestroyableThings:
			case TraverseMode.PassAllDestroyableThingsNotWater:
				return (ushort)(costBlockedDoor + (ushort)((float)b.HitPoints * costBlockedDoorPerHitPoint));
			case TraverseMode.PassAllDestroyablePlayerOwnedThings:
				if (!b.Faction.IsPlayer)
				{
					return ushort.MaxValue;
				}
				return (ushort)(costBlockedDoor + (int)((float)b.HitPoints * costBlockedDoorPerHitPoint));
			case TraverseMode.PassDoors:
			case TraverseMode.NoPassClosedDoors:
			case TraverseMode.NoPassClosedDoorsOrWater:
				return 0;
			}
		}
		else if (pawn != null && b is IPathFindCostProvider pathFindCostProvider)
		{
			return pathFindCostProvider.PathFindCostFor(pawn);
		}
		return 0;
	}

	public static bool BlocksDiagonalMovement(int x, int z, PathingContext pc, bool canBashFences)
	{
		return BlocksDiagonalMovement(pc.map.cellIndices.CellToIndex(x, z), pc, canBashFences);
	}

	public static bool BlocksDiagonalMovement(int index, PathingContext pc, bool canBashFences)
	{
		if (!pc.pathGrid.WalkableFast(index))
		{
			return true;
		}
		Building building = pc.map.edificeGrid[index];
		if (building != null && canBashFences && building.def.IsFence)
		{
			return true;
		}
		return false;
	}
}
