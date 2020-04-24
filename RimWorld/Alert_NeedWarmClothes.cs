using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Alert_NeedWarmClothes : Alert
	{
		private static List<Thing> jackets = new List<Thing>();

		private static List<Thing> shirts = new List<Thing>();

		private static List<Thing> pants = new List<Thing>();

		private const float MedicinePerColonistThreshold = 2f;

		private const int CheckNextTwelfthsCount = 3;

		private const float CanShowAlertOnlyIfTempBelow = 5f;

		private static Comparison<Thing> SortByInsulationCold = (Thing a, Thing b) => a.GetStatValue(StatDefOf.Insulation_Cold).CompareTo(b.GetStatValue(StatDefOf.Insulation_Cold));

		private List<Pawn> colonistsWithoutWarmClothes = new List<Pawn>();

		public Alert_NeedWarmClothes()
		{
			defaultLabel = "NeedWarmClothes".Translate();
			defaultPriority = AlertPriority.High;
		}

		private int NeededWarmClothesCount(Map map)
		{
			return map.mapPawns.FreeColonistsSpawnedCount;
		}

		private int ColonistsWithWarmClothesCount(Map map)
		{
			float num = LowestTemperatureComing(map);
			int num2 = 0;
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.GetStatValue(StatDefOf.ComfyTemperatureMin) <= num)
				{
					num2++;
				}
			}
			return num2;
		}

		private void GetColonistsWithoutWarmClothes(Map map, List<Pawn> outResult)
		{
			outResult.Clear();
			float num = LowestTemperatureComing(map);
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				if (item.GetStatValue(StatDefOf.ComfyTemperatureMin) > num)
				{
					outResult.Add(item);
				}
			}
		}

		private int FreeWarmClothesSetsCount(Map map)
		{
			jackets.Clear();
			shirts.Clear();
			pants.Clear();
			List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.Apparel);
			for (int i = 0; i < list.Count; i++)
			{
				if (!list[i].IsInAnyStorage() || list[i].GetStatValue(StatDefOf.Insulation_Cold) <= 0f)
				{
					continue;
				}
				if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Torso))
				{
					if (list[i].def.apparel.layers.Contains(ApparelLayerDefOf.OnSkin))
					{
						shirts.Add(list[i]);
					}
					else
					{
						jackets.Add(list[i]);
					}
				}
				if (list[i].def.apparel.bodyPartGroups.Contains(BodyPartGroupDefOf.Legs))
				{
					pants.Add(list[i]);
				}
			}
			jackets.Sort(SortByInsulationCold);
			shirts.Sort(SortByInsulationCold);
			pants.Sort(SortByInsulationCold);
			float num = ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin) - LowestTemperatureComing(map);
			if (num <= 0f)
			{
				return GenMath.Max(jackets.Count, shirts.Count, pants.Count);
			}
			int num2 = 0;
			while (jackets.Any() || shirts.Any() || pants.Any())
			{
				float num3 = 0f;
				if (jackets.Any())
				{
					Thing thing = jackets[jackets.Count - 1];
					jackets.RemoveLast();
					num3 += thing.GetStatValue(StatDefOf.Insulation_Cold);
				}
				if (num3 < num && shirts.Any())
				{
					Thing thing2 = shirts[shirts.Count - 1];
					shirts.RemoveLast();
					num3 += thing2.GetStatValue(StatDefOf.Insulation_Cold);
				}
				if (num3 < num && pants.Any())
				{
					for (int j = 0; j < pants.Count; j++)
					{
						float statValue = pants[j].GetStatValue(StatDefOf.Insulation_Cold);
						if (statValue + num3 >= num)
						{
							num3 += statValue;
							pants.RemoveAt(j);
							break;
						}
					}
				}
				if (!(num3 >= num))
				{
					break;
				}
				num2++;
			}
			jackets.Clear();
			shirts.Clear();
			pants.Clear();
			return num2;
		}

		private int MissingWarmClothesCount(Map map)
		{
			if (LowestTemperatureComing(map) >= ThingDefOf.Human.GetStatValueAbstract(StatDefOf.ComfyTemperatureMin))
			{
				return 0;
			}
			return Mathf.Max(NeededWarmClothesCount(map) - ColonistsWithWarmClothesCount(map) - FreeWarmClothesSetsCount(map), 0);
		}

		private float LowestTemperatureComing(Map map)
		{
			Twelfth twelfth = GenLocalDate.Twelfth(map);
			float a = GetTemperature(twelfth, map);
			for (int i = 0; i < 3; i++)
			{
				twelfth = twelfth.NextTwelfth();
				a = Mathf.Min(a, GetTemperature(twelfth, map));
			}
			return Mathf.Min(a, map.mapTemperature.OutdoorTemp);
		}

		public override TaggedString GetExplanation()
		{
			Map map = MapWithMissingWarmClothes();
			if (map == null)
			{
				return "";
			}
			int num = MissingWarmClothesCount(map);
			if (num == NeededWarmClothesCount(map))
			{
				return "NeedWarmClothesDesc1All".Translate() + "\n\n" + "NeedWarmClothesDesc2".Translate(LowestTemperatureComing(map).ToStringTemperature("F0"));
			}
			return "NeedWarmClothesDesc1".Translate(num) + "\n\n" + "NeedWarmClothesDesc2".Translate(LowestTemperatureComing(map).ToStringTemperature("F0"));
		}

		public override AlertReport GetReport()
		{
			Map map = MapWithMissingWarmClothes();
			if (map == null)
			{
				return false;
			}
			colonistsWithoutWarmClothes.Clear();
			GetColonistsWithoutWarmClothes(map, colonistsWithoutWarmClothes);
			return AlertReport.CulpritsAre(colonistsWithoutWarmClothes);
		}

		private Map MapWithMissingWarmClothes()
		{
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				Map map = maps[i];
				if (map.IsPlayerHome && !(LowestTemperatureComing(map) >= 5f) && MissingWarmClothesCount(map) > 0)
				{
					return map;
				}
			}
			return null;
		}

		private float GetTemperature(Twelfth twelfth, Map map)
		{
			return GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth);
		}
	}
}
