using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class CommonRitualCellPredicates
{
	public static readonly Predicate<CellRect, IntVec3> InsideRect = (CellRect rect, IntVec3 cell) => rect.Contains(cell);

	public static readonly Predicate<Pawn, IntVec3> Reachable = (Pawn pawn, IntVec3 cell) => pawn.CanReserveAndReach(cell, PathEndMode.OnCell, pawn.NormalMaxDanger());

	public static readonly Predicate<Map, IntVec3> Standable = (Map map, IntVec3 cell) => cell.Standable(map);

	public static readonly Predicate<Map, IntVec3> OnBed = delegate(Map map, IntVec3 cell)
	{
		Building building = map.edificeGrid[cell];
		return building != null && building.def?.IsBed == true;
	};

	public static readonly Predicate<Map, IntVec3> NotOnBed = delegate(Map map, IntVec3 cell)
	{
		Building building = map.edificeGrid[cell];
		return building == null || building.def?.IsBed != true;
	};

	public static readonly Predicate<Map, IntVec3> InDoor = (Map map, IntVec3 cell) => cell.GetDoor(map) != null;

	public static readonly Predicate<Map, IntVec3> NotInDoor = (Map map, IntVec3 cell) => cell.GetDoor(map) == null;

	public static readonly Predicate<(Map, IntVec3), IntVec3> InSameRoomAsSpot = _InSameRoomAsSpot;

	public static bool RemoveLeastDesirableRitualCells(List<IntVec3> cells, IntVec3 spot, Map map, Pawn pawn, CellRect rect)
	{
		cells.RemoveAll(rect, InsideRect);
		cells.RemoveAll(map, Standable, negatePredicate: true);
		cells.RemoveAll((map, spot), InSameRoomAsSpot, negatePredicate: true);
		cells.RemoveAll(pawn, Reachable, negatePredicate: true);
		if (cells.RemoveAll_IfNotAll(map, NotOnBed))
		{
			return cells.RemoveAll_IfNotAll(map, NotInDoor);
		}
		return false;
	}

	public static Func<IntVec3, bool> DefaultValidator(IntVec3 spot, Map map, Pawn pawn, CellRect rect)
	{
		return (IntVec3 cell) => !InsideRect(rect, cell) && Standable(map, cell) && InSameRoomAsSpot((map, spot), cell) && Reachable(pawn, cell);
	}

	private static bool _InSameRoomAsSpot((Map, IntVec3) context, IntVec3 c)
	{
		var (map, locB) = context;
		return WanderUtility.InSameRoom(c, locB, map);
	}
}
