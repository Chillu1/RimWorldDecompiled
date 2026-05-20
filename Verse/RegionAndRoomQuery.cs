namespace Verse;

public static class RegionAndRoomQuery
{
	public static Region RegionAt(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		if (!c.InBounds(map))
		{
			return null;
		}
		Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
		if (validRegionAt != null && (validRegionAt.type & allowedRegionTypes) != RegionType.None)
		{
			return validRegionAt;
		}
		return null;
	}

	public static Region GetRegion(this Thing thing, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		if (!thing.Spawned)
		{
			return null;
		}
		return RegionAt(thing.Position, thing.Map, allowedRegionTypes);
	}

	public static Region GetRegionHeld(this Thing thing, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		return RegionAt(thing.PositionHeld, thing.MapHeld, allowedRegionTypes);
	}

	public static District DistrictAt(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		return RegionAt(c, map, allowedRegionTypes)?.District;
	}

	public static Room RoomAt(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_All)
	{
		return DistrictAt(c, map, allowedRegionTypes)?.Room;
	}

	public static District GetDistrict(this Thing thing, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		Thing spawnedParentOrMe = thing.SpawnedParentOrMe;
		if (spawnedParentOrMe != null)
		{
			return DistrictAt(spawnedParentOrMe.Position, spawnedParentOrMe.Map, allowedRegionTypes);
		}
		return null;
	}

	public static Room GetRoom(this Thing thing, RegionType allowedRegionTypes = RegionType.Set_All)
	{
		return thing.GetDistrict(allowedRegionTypes)?.Room;
	}

	public static bool IsOutside(this Thing thing)
	{
		return thing.GetRoom()?.PsychologicallyOutdoors ?? true;
	}

	public static District DistirctAtFast(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
		if (validRegionAt != null && (validRegionAt.type & allowedRegionTypes) != RegionType.None)
		{
			return validRegionAt.District;
		}
		return null;
	}

	public static Room RoomAtOrAdjacent(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		Room room = RoomAt(c, map, allowedRegionTypes);
		if (room != null)
		{
			return room;
		}
		for (int i = 0; i < 8; i++)
		{
			room = RoomAt(c + GenAdj.AdjacentCells[i], map, allowedRegionTypes);
			if (room != null)
			{
				return room;
			}
		}
		return room;
	}
}
