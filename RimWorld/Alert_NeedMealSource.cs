using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedMealSource : Alert
	{
		public Alert_NeedMealSource()
		{
			defaultLabel = "NeedMealSource".Translate();
			defaultExplanation = "NeedMealSourceDesc".Translate();
		}

		public override AlertReport GetReport()
		{
			if (GenDate.DaysPassed < 2)
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (NeedMealSource(maps[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool NeedMealSource(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			if (!map.mapPawns.AnyColonistSpawned)
			{
				return false;
			}
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				if (allBuildingsColonist[i].def.building.isMealSource)
				{
					return false;
				}
			}
			return true;
		}
	}
}
