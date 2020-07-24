using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class PlantUtility
	{
		public static bool GrowthSeasonNow(IntVec3 c, Map map, bool forSowing = false)
		{
			Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
			if (roomOrAdjacent == null)
			{
				return false;
			}
			if (roomOrAdjacent.UsesOutdoorTemperature)
			{
				if (forSowing)
				{
					return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNowForSowing;
				}
				return map.weatherManager.growthSeasonMemory.GrowthSeasonOutdoorsNow;
			}
			float temperature = c.GetTemperature(map);
			if (temperature > 0f)
			{
				return temperature < 58f;
			}
			return false;
		}

		public static bool SnowAllowsPlanting(IntVec3 c, Map map)
		{
			return c.GetSnowDepth(map) < 0.2f;
		}

		public static bool CanEverPlantAt(this ThingDef plantDef, IntVec3 c, Map map)
		{
			return plantDef.CanEverPlantAt_NewTemp(c, map);
		}

		public static bool CanEverPlantAt_NewTemp(this ThingDef plantDef, IntVec3 c, Map map, bool canWipePlantsExceptTree = false)
		{
			if (plantDef.category != ThingCategory.Plant)
			{
				Log.Error(string.Concat("Checking CanGrowAt with ", plantDef, " which is not a plant."));
			}
			if (!c.InBounds(map))
			{
				return false;
			}
			if (map.fertilityGrid.FertilityAt(c) < plantDef.plant.fertilityMin)
			{
				return false;
			}
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Thing thing = list[i];
				if (thing.def.BlocksPlanting(canWipePlantsExceptTree))
				{
					return false;
				}
				if (plantDef.passability == Traversability.Impassable)
				{
					if (thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building)
					{
						return false;
					}
					if (thing.def.category == ThingCategory.Plant && canWipePlantsExceptTree && thing.def.plant.IsTree)
					{
						return false;
					}
				}
			}
			if (plantDef.passability == Traversability.Impassable)
			{
				for (int j = 0; j < 4; j++)
				{
					IntVec3 c2 = c + GenAdj.CardinalDirections[j];
					if (c2.InBounds(map))
					{
						Building edifice = c2.GetEdifice(map);
						if (edifice != null && edifice.def.IsDoor)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		public static void LogPlantProportions()
		{
			Dictionary<ThingDef, float> dictionary = new Dictionary<ThingDef, float>();
			foreach (ThingDef allWildPlant in Find.CurrentMap.Biome.AllWildPlants)
			{
				dictionary.Add(allWildPlant, 0f);
			}
			float num = 0f;
			foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
			{
				Plant plant = allCell.GetPlant(Find.CurrentMap);
				if (plant != null && dictionary.ContainsKey(plant.def))
				{
					dictionary[plant.def]++;
					num += 1f;
				}
			}
			foreach (ThingDef allWildPlant2 in Find.CurrentMap.Biome.AllWildPlants)
			{
				dictionary[allWildPlant2] /= num;
			}
			Dictionary<ThingDef, float> dictionary2 = CalculateDesiredPlantProportions(Find.CurrentMap.Biome);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("PLANT           EXPECTED             FOUND");
			foreach (ThingDef allWildPlant3 in Find.CurrentMap.Biome.AllWildPlants)
			{
				stringBuilder.AppendLine(allWildPlant3.LabelCap + "       " + dictionary2[allWildPlant3].ToStringPercent() + "        " + dictionary[allWildPlant3].ToStringPercent());
			}
			Log.Message(stringBuilder.ToString());
		}

		private static Dictionary<ThingDef, float> CalculateDesiredPlantProportions(BiomeDef biome)
		{
			Dictionary<ThingDef, float> dictionary = new Dictionary<ThingDef, float>();
			float num = 0f;
			foreach (ThingDef allDef in DefDatabase<ThingDef>.AllDefs)
			{
				if (allDef.plant != null)
				{
					float num2 = biome.CommonalityOfPlant(allDef);
					dictionary.Add(allDef, num2);
					num += num2;
				}
			}
			foreach (ThingDef allWildPlant in biome.AllWildPlants)
			{
				dictionary[allWildPlant] /= num;
			}
			return dictionary;
		}

		public static IEnumerable<ThingDef> ValidPlantTypesForGrowers(List<IPlantToGrowSettable> sel)
		{
			foreach (ThingDef plantDef in DefDatabase<ThingDef>.AllDefs.Where((ThingDef def) => def.category == ThingCategory.Plant))
			{
				if (sel.TrueForAll((IPlantToGrowSettable x) => CanSowOnGrower(plantDef, x)))
				{
					yield return plantDef;
				}
			}
		}

		public static bool CanSowOnGrower(ThingDef plantDef, object obj)
		{
			if (obj is Zone)
			{
				return plantDef.plant.sowTags.Contains("Ground");
			}
			Thing thing = obj as Thing;
			if (thing != null && thing.def.building != null)
			{
				return plantDef.plant.sowTags.Contains(thing.def.building.sowTag);
			}
			return false;
		}

		public static Thing AdjacentSowBlocker(ThingDef plantDef, IntVec3 c, Map map)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (c2.InBounds(map))
				{
					Plant plant = c2.GetPlant(map);
					if (plant != null && (plant.def.plant.blockAdjacentSow || (plantDef.plant.blockAdjacentSow && plant.sown)))
					{
						return plant;
					}
				}
			}
			return null;
		}

		public static byte GetWindExposure(Plant plant)
		{
			return (byte)Mathf.Min(255f * plant.def.plant.topWindExposure, 255f);
		}

		public static void SetWindExposureColors(Color32[] colors, Plant plant)
		{
			colors[1].a = (colors[2].a = GetWindExposure(plant));
			colors[0].a = (colors[3].a = 0);
		}

		public static void LogFallColorForYear()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Fall color amounts for each latitude and each day of the year");
			stringBuilder.AppendLine("---------------------------------------");
			stringBuilder.Append("Lat".PadRight(6));
			for (int i = -90; i <= 90; i += 10)
			{
				stringBuilder.Append((i + "d").PadRight(6));
			}
			stringBuilder.AppendLine();
			for (int j = 0; j < 60; j += 5)
			{
				stringBuilder.Append(j.ToString().PadRight(6));
				for (int k = -90; k <= 90; k += 10)
				{
					stringBuilder.Append(PlantFallColors.GetFallColorFactor(k, j).ToString("F3").PadRight(6));
				}
				stringBuilder.AppendLine();
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
