using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_LowFood : Alert
	{
		private const float NutritionThresholdPerColonist = 4f;

		public Alert_LowFood()
		{
			defaultLabel = "LowFood".Translate();
			defaultPriority = AlertPriority.High;
		}

		public override TaggedString GetExplanation()
		{
			Map map = MapWithLowFood();
			if (map == null)
			{
				return "";
			}
			float totalHumanEdibleNutrition = map.resourceCounter.TotalHumanEdibleNutrition;
			int num = map.mapPawns.FreeColonistsSpawnedCount + map.mapPawns.PrisonersOfColonyCount;
			int num2 = Mathf.FloorToInt(totalHumanEdibleNutrition / (float)num);
			return "LowFoodDesc".Translate(totalHumanEdibleNutrition.ToString("F0"), num.ToStringCached(), num2.ToStringCached());
		}

		public override AlertReport GetReport()
		{
			if (Find.TickManager.TicksGame < 150000)
			{
				return false;
			}
			return MapWithLowFood() != null;
		}

		private Map MapWithLowFood()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome && map.mapPawns.AnyColonistSpawned)
				{
					int freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
					if (map.resourceCounter.TotalHumanEdibleNutrition < 4f * (float)freeColonistsSpawnedCount)
					{
						return map;
					}
				}
			}
			return null;
		}
	}
}
