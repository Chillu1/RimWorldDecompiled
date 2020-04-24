using System.Collections.Generic;

namespace Verse
{
	public static class RegionTypeUtility
	{
		public static bool IsOneCellRegion(this RegionType regionType)
		{
			return regionType == RegionType.Portal;
		}

		public static bool AllowsMultipleRegionsPerRoom(this RegionType regionType)
		{
			return regionType != RegionType.Portal;
		}

		public static RegionType GetExpectedRegionType(this IntVec3 c, Map map)
		{
			if (!c.InBounds(map))
			{
				return RegionType.None;
			}
			if (c.GetDoor(map) != null)
			{
				return RegionType.Portal;
			}
			if (c.Walkable(map))
			{
				return RegionType.Normal;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].def.Fillage == FillCategory.Full)
				{
					return RegionType.None;
				}
			}
			return RegionType.ImpassableFreeAirExchange;
		}

		public static RegionType GetRegionType(this IntVec3 c, Map map)
		{
			return c.GetRegion(map, RegionType.Set_All)?.type ?? RegionType.None;
		}

		public static bool Passable(this RegionType regionType)
		{
			return (regionType & RegionType.Set_Passable) != 0;
		}
	}
}
