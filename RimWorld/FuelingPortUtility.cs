using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class FuelingPortUtility
	{
		public static IntVec3 GetFuelingPortCell(Building podLauncher)
		{
			return GetFuelingPortCell(podLauncher.Position, podLauncher.Rotation);
		}

		public static IntVec3 GetFuelingPortCell(IntVec3 center, Rot4 rot)
		{
			rot.Rotate(RotationDirection.Clockwise);
			return center + rot.FacingCell;
		}

		public static bool AnyFuelingPortGiverAt(IntVec3 c, Map map)
		{
			return FuelingPortGiverAt(c, map) != null;
		}

		public static Building FuelingPortGiverAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Building building = thingList[i] as Building;
				if (building != null && building.def.building.hasFuelingPort)
				{
					return building;
				}
			}
			return null;
		}

		public static Building FuelingPortGiverAtFuelingPortCell(IntVec3 c, Map map)
		{
			for (int i = 0; i < 4; i++)
			{
				IntVec3 c2 = c + GenAdj.CardinalDirections[i];
				if (!c2.InBounds(map))
				{
					continue;
				}
				List<Thing> thingList = c2.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					Building building = thingList[j] as Building;
					if (building != null && building.def.building.hasFuelingPort && GetFuelingPortCell(building) == c)
					{
						return building;
					}
				}
			}
			return null;
		}

		public static CompLaunchable LaunchableAt(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				CompLaunchable compLaunchable = thingList[i].TryGetComp<CompLaunchable>();
				if (compLaunchable != null)
				{
					return compLaunchable;
				}
			}
			return null;
		}
	}
}
