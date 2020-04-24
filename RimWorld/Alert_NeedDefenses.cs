using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Alert_NeedDefenses : Alert
	{
		public Alert_NeedDefenses()
		{
			defaultLabel = "NeedDefenses".Translate();
			defaultExplanation = "NeedDefensesDesc".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override AlertReport GetReport()
		{
			if (GenDate.DaysPassed < 2 || GenDate.DaysPassed > 5)
			{
				return false;
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				if (NeedDefenses(maps[i]))
				{
					return true;
				}
			}
			return false;
		}

		private bool NeedDefenses(Map map)
		{
			if (!map.IsPlayerHome)
			{
				return false;
			}
			if (!map.mapPawns.AnyColonistSpawned && !map.listerBuildings.allBuildingsColonist.Any())
			{
				return false;
			}
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if ((building.def.building != null && (building.def.building.IsTurret || building.def.building.isTrap)) || building.def == ThingDefOf.Sandbags || building.def == ThingDefOf.Barricade)
				{
					return false;
				}
			}
			return true;
		}
	}
}
