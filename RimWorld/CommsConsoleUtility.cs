using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class CommsConsoleUtility
	{
		public static bool PlayerHasPoweredCommsConsole(Map map)
		{
			foreach (Building_CommsConsole item in map.listerBuildings.AllBuildingsColonistOfClass<Building_CommsConsole>())
			{
				if (item.Faction == Faction.OfPlayer)
				{
					CompPowerTrader compPowerTrader = item.TryGetComp<CompPowerTrader>();
					if (compPowerTrader == null || compPowerTrader.PowerOn)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static bool PlayerHasPoweredCommsConsole()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (PlayerHasPoweredCommsConsole(maps[i]))
				{
					return true;
				}
			}
			return false;
		}
	}
}
