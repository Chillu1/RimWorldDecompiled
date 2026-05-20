using System.Collections.Generic;

namespace Verse
{
	public static class RegionTypeUtility
	{
		public static bool IsOneCellRegion(this RegionType regionType)
		{
			return regionType == RegionType.Portal;
		}

		public static bool AllowsMultipleRegionsPerDistrict(this RegionType regionType)
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
			if (c.GetFence(map) != null)
			{
				return RegionType.Fence;
			}
			if (c.WalkableByNormal(map))
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

		public static bool Passable(this RegionType regionType)
		{
			return (regionType & RegionType.Set_Passable) != 0;
		}
	}
}
