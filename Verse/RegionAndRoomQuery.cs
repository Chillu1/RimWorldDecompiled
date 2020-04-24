namespace Verse
{
	public static class RegionAndRoomQuery
	{
		public static Region RegionAt(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			if (!c.InBounds(map))
			{
				return null;
			}
			Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
			if (validRegionAt != null && (validRegionAt.type & allowedRegionTypes) != 0)
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

		public static Room RoomAt(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			return RegionAt(c, map, allowedRegionTypes)?.Room;
		}

		public static RoomGroup RoomGroupAt(IntVec3 c, Map map)
		{
			return RoomAt(c, map, RegionType.Set_All)?.Group;
		}

		public static Room GetRoom(this Thing thing, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			if (!thing.Spawned)
			{
				return null;
			}
			return RoomAt(thing.Position, thing.Map, allowedRegionTypes);
		}

		public static RoomGroup GetRoomGroup(this Thing thing)
		{
			return thing.GetRoom(RegionType.Set_All)?.Group;
		}

		public static Room RoomAtFast(IntVec3 c, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
			if (validRegionAt != null && (validRegionAt.type & allowedRegionTypes) != 0)
			{
				return validRegionAt.Room;
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
}
