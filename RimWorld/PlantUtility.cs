using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PlantUtility
{
	public static bool GrowthSeasonNow(Map map, ThingDef plantDef)
	{
		float outdoorTemp = map.mapTemperature.OutdoorTemp;
		if (outdoorTemp > plantDef.plant.minGrowthTemperature)
		{
			return outdoorTemp < plantDef.plant.maxGrowthTemperature;
		}
		return false;
	}

	public static bool GrowthSeasonNow(IntVec3 c, Map map, ThingDef plantDef)
	{
		if (c.GetRoomOrAdjacent(map, RegionType.Set_All) == null)
		{
			return false;
		}
		float temperature = c.GetTemperature(map);
		if (temperature > plantDef.plant.minGrowthTemperature)
		{
			return temperature < plantDef.plant.maxGrowthTemperature;
		}
		return false;
	}

	public static bool SnowAllowsPlanting(IntVec3 c, Map map)
	{
		return c.GetSnowDepth(map) < 0.2f;
	}

	public static bool SandAllowsPlanting(IntVec3 c, Map map)
	{
		return c.GetSandDepth(map) < 0.2f;
	}

	public static bool CanNowPlantAt(this ThingDef plantDef, IntVec3 c, Map map, bool canWipePlantsExceptTree = false)
	{
		if (!plantDef.CanEverPlantAt(c, map, canWipePlantsExceptTree, checkMapTemperature: false))
		{
			return false;
		}
		foreach (Thing thing in c.GetThingList(map))
		{
			if (map.designationManager.DesignationOn(thing, DesignationDefOf.Uninstall) != null)
			{
				return false;
			}
			if (map.designationManager.DesignationOn(thing, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (thing is Building building && map.listerBuildings.TryGetReinstallBlueprint(building, out var _))
			{
				return false;
			}
		}
		return true;
	}

	public static AcceptanceReport CanEverPlantAt(this ThingDef plantDef, IntVec3 c, Map map, out Thing blockingThing, bool canWipePlantsExceptTree = false, bool checkMapTemperature = true, bool writeNoReason = false)
	{
		blockingThing = null;
		if (plantDef.category != ThingCategory.Plant)
		{
			Log.Error("Checking CanGrowAt with " + plantDef?.ToString() + " which is not a plant.");
		}
		if (!c.InBounds(map))
		{
			if (!writeNoReason)
			{
				return "OutOfBounds".Translate();
			}
			return false;
		}
		TerrainDef terrain = c.GetTerrain(map);
		if (!plantDef.plant.completelyIgnoreFertility && map.fertilityGrid.FertilityAt(c) < plantDef.plant.fertilityMin)
		{
			if (!writeNoReason)
			{
				return "MessageWarningNotEnoughFertility".Translate();
			}
			return false;
		}
		if (checkMapTemperature && (map.TileInfo.MinTemperature > plantDef.plant.maxGrowthTemperature || map.TileInfo.MaxTemperature < plantDef.plant.minGrowthTemperature))
		{
			if (!writeNoReason)
			{
				return "CannotPlantExtremeTemp".Translate();
			}
			return false;
		}
		if (c.IsPolluted(map))
		{
			if (plantDef.plant.pollution == Pollution.CleanOnly)
			{
				if (!writeNoReason)
				{
					return "MessageWarningPollutedCell".Translate();
				}
				return false;
			}
		}
		else if (plantDef.plant.pollution == Pollution.PollutedOnly)
		{
			if (!writeNoReason)
			{
				return "MessageWarningNotPollutedCell".Translate();
			}
			return false;
		}
		if (plantDef.plant.terraformable && !CompTerraformer.CanEverConvertCell(c, map))
		{
			if (!writeNoReason)
			{
				return "MessageCannotBePlacedOn".Translate(terrain);
			}
			return false;
		}
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		bool flag = false;
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Building_PlantGrower)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			if (plantDef.plant.WildTerrainTags.Count > 0 && !plantDef.plant.WildTerrainTags.Overlaps(terrain.tags.OrElseEmptyEnumerable()))
			{
				if (!writeNoReason)
				{
					return "CannotPlantMissingTerrainTag".Translate();
				}
				return false;
			}
			if (plantDef.plant.terrainBlacklist != null && plantDef.plant.terrainBlacklist.Contains(terrain))
			{
				if (!writeNoReason)
				{
					return "CannotPlantMissingTerrainTag".Translate(terrain);
				}
				return false;
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Thing thing = list[j];
			if (!flag && thing.def.BlocksPlanting(canWipePlantsExceptTree))
			{
				blockingThing = thing;
				if (!writeNoReason)
				{
					return "BlockedBy".Translate(thing);
				}
				return false;
			}
			if (plantDef.passability != Traversability.Impassable)
			{
				continue;
			}
			if (thing.def.category == ThingCategory.Pawn || thing.def.category == ThingCategory.Item || thing.def.category == ThingCategory.Building)
			{
				blockingThing = thing;
				if (!writeNoReason)
				{
					return "BlockedBy".Translate(thing);
				}
				return false;
			}
			if (thing.def.category == ThingCategory.Plant && canWipePlantsExceptTree && thing.def.plant.IsTree)
			{
				blockingThing = thing;
				if (!writeNoReason)
				{
					return "BlockedBy".Translate(thing);
				}
				return false;
			}
		}
		if (plantDef.passability == Traversability.Impassable)
		{
			for (int k = 0; k < 4; k++)
			{
				IntVec3 c2 = c + GenAdj.CardinalDirections[k];
				if (!c2.InBounds(map))
				{
					continue;
				}
				Building edifice = c2.GetEdifice(map);
				if (edifice != null && edifice.def.IsDoor)
				{
					blockingThing = edifice;
					if (!writeNoReason)
					{
						return "BlockedBy".Translate(edifice);
					}
					return false;
				}
			}
		}
		return true;
	}

	public static bool CanEverPlantAt(this ThingDef plantDef, IntVec3 c, Map map, bool canWipePlantsExceptTree = false, bool checkMapTemperature = true)
	{
		Thing blockingThing;
		return plantDef.CanEverPlantAt(c, map, out blockingThing, canWipePlantsExceptTree, checkMapTemperature, writeNoReason: true).Accepted;
	}

	public static void LogPlantProportions()
	{
		Dictionary<ThingDef, float> dictionary = new Dictionary<ThingDef, float>();
		foreach (ThingDef allWildPlant in Find.CurrentMap.wildPlantSpawner.AllWildPlants)
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
		foreach (ThingDef allWildPlant2 in Find.CurrentMap.wildPlantSpawner.AllWildPlants)
		{
			dictionary[allWildPlant2] /= num;
		}
		Dictionary<ThingDef, float> dictionary2 = CalculateDesiredPlantProportions(Find.CurrentMap.Biome);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("PLANT           EXPECTED             FOUND");
		foreach (ThingDef allWildPlant3 in Find.CurrentMap.wildPlantSpawner.AllWildPlants)
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
		if (obj is IPlantToGrowSettable settable && !PollutionUtility.CanPlantAt(plantDef, settable))
		{
			return false;
		}
		if (obj is Zone)
		{
			return plantDef.plant.sowTags.Contains("Ground");
		}
		if (obj is Thing thing && thing.def.building != null)
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

	public static float GrowthRateFactorFor_Fertility(ThingDef def, float fertilityAtCell)
	{
		if (def.plant.completelyIgnoreFertility)
		{
			return 1f;
		}
		return fertilityAtCell * def.plant.fertilitySensitivity + (1f - def.plant.fertilitySensitivity);
	}

	public static float GrowthRateFactorFor_Light(ThingDef def, float glow)
	{
		if (def.plant.growMinGlow == def.plant.growOptimalGlow && glow == def.plant.growOptimalGlow)
		{
			return 1f;
		}
		return GenMath.InverseLerp(def.plant.growMinGlow, def.plant.growOptimalGlow, glow);
	}

	public static float GrowthRateFactorFor_Temperature(ThingDef plant, float cellTemp)
	{
		if (cellTemp < plant.plant.minOptimalGrowthTemperature)
		{
			return Mathf.InverseLerp(plant.plant.minGrowthTemperature, plant.plant.minOptimalGrowthTemperature, cellTemp);
		}
		if (cellTemp > plant.plant.maxOptimalGrowthTemperature)
		{
			return Mathf.InverseLerp(plant.plant.maxGrowthTemperature, plant.plant.maxOptimalGrowthTemperature, cellTemp);
		}
		return 1f;
	}

	public static float NutritionFactorFromGrowth(ThingDef def, float plantGrowth)
	{
		if (def.plant.Sowable)
		{
			return plantGrowth;
		}
		return Mathf.Lerp(0.5f, 1f, plantGrowth);
	}

	public static bool PawnWillingToCutPlant_Job(Thing plant, Pawn pawn)
	{
		if (plant.def.plant.IsTree && plant.def.plant.treeLoversCareIfChopped)
		{
			return new HistoryEvent(HistoryEventDefOf.CutTree, pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job();
		}
		return true;
	}

	public static bool TreeMarkedForExtraction(Thing plant)
	{
		if (plant.def.plant.IsTree)
		{
			return plant.MapHeld.designationManager.DesignationOn(plant, DesignationDefOf.ExtractTree) != null;
		}
		return false;
	}
}
