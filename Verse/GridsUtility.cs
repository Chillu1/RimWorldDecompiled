using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;

namespace Verse;

public static class GridsUtility
{
	private static readonly HashSet<Region> connected = new HashSet<Region>();

	private static readonly Queue<Region> queue = new Queue<Region>();

	public static float GetTemperature(this IntVec3 loc, Map map)
	{
		return GenTemperature.GetTemperatureForCell(loc, map);
	}

	public static Region GetRegion(this IntVec3 loc, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		return RegionAndRoomQuery.RegionAt(loc, map, allowedRegionTypes);
	}

	public static District GetDistrict(this IntVec3 loc, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		return RegionAndRoomQuery.DistrictAt(loc, map, allowedRegionTypes);
	}

	public static bool ConnectedCellCountExceeds(this IntVec3 cell, Map map, int count, RegionType allowed = RegionType.Set_Passable)
	{
		Region region = cell.GetRegion(map, allowed);
		if (region == null)
		{
			return false;
		}
		int num = region.CellCount;
		connected.Add(region);
		queue.Enqueue(region);
		Region result;
		while (queue.TryDequeue(out result) && num < count)
		{
			num += result.CellCount;
			foreach (RegionLink link in result.links)
			{
				Region otherRegion = link.GetOtherRegion(result);
				if (connected.Add(otherRegion))
				{
					queue.Enqueue(otherRegion);
				}
			}
		}
		connected.Clear();
		queue.Clear();
		return num >= count;
	}

	public static Room GetRoom(this IntVec3 loc, Map map)
	{
		return RegionAndRoomQuery.RoomAt(loc, map);
	}

	public static Room GetRoomOrAdjacent(this IntVec3 loc, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
	{
		return RegionAndRoomQuery.RoomAtOrAdjacent(loc, map, allowedRegionTypes);
	}

	public static List<Thing> GetThingList(this IntVec3 c, Map map)
	{
		return map.thingGrid.ThingsListAt(c);
	}

	public static float GetSnowDepth(this IntVec3 c, Map map)
	{
		return map.snowGrid.GetDepth(c);
	}

	public static float GetSandDepth(this IntVec3 c, Map map)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return 0f;
		}
		return map.sandGrid.GetDepth(c);
	}

	public static bool Fogged(this Thing t)
	{
		return t.MapHeld.fogGrid.IsFogged(t.PositionHeld);
	}

	public static bool Fogged(this IntVec3 c, Map map)
	{
		return map.fogGrid.IsFogged(c);
	}

	public static RoofDef GetRoof(this IntVec3 c, Map map)
	{
		return map.roofGrid.RoofAt(c);
	}

	public static bool Roofed(this IntVec3 c, Map map)
	{
		return map.roofGrid.Roofed(c);
	}

	public static bool Filled(this IntVec3 c, Map map)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null)
		{
			return edifice.def.Fillage == FillCategory.Full;
		}
		return false;
	}

	public static TerrainDef GetTerrain(this IntVec3 c, Map map)
	{
		TerrainDef terrainDef = map.terrainGrid.TerrainAt(c);
		if (terrainDef == null)
		{
			return TerrainDefOf.Soil;
		}
		return terrainDef;
	}

	public static List<TerrainAffordanceDef> GetAffordances(this IntVec3 c, Map map)
	{
		if (map.terrainGrid.FoundationAt(c) != null)
		{
			return map.terrainGrid.FoundationAt(c).affordances;
		}
		return map.terrainGrid.TerrainAt(c).affordances;
	}

	public static WaterBodyType GetWaterBodyType(this IntVec3 c, Map map)
	{
		if (map.terrainGrid.FoundationAt(c) != null)
		{
			return map.terrainGrid.BaseTerrainAt(c).waterBodyType;
		}
		if (map.terrainGrid.TerrainAt(c).temporary)
		{
			return map.terrainGrid.TerrainAtIgnoreTemp(map.cellIndices.CellToIndex(c)).waterBodyType;
		}
		return map.terrainGrid.TerrainAt(c).waterBodyType;
	}

	public static Plan GetPlan(this IntVec3 c, Map map)
	{
		return map.planManager.PlanAt(c);
	}

	public static Zone GetZone(this IntVec3 c, Map map)
	{
		return map.zoneManager.ZoneAt(c);
	}

	public static Plant GetPlant(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.category == ThingCategory.Plant)
			{
				return (Plant)list[i];
			}
		}
		return null;
	}

	public static Thing GetRoofHolderOrImpassable(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def.holdsRoof || thingList[i].def.passability == Traversability.Impassable)
			{
				return thingList[i];
			}
		}
		return null;
	}

	public static Thing GetFirstThing(this IntVec3 c, Map map, ThingDef def)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def == def)
			{
				return thingList[i];
			}
		}
		return null;
	}

	public static ThingWithComps GetFirstThingWithComp<TComp>(this IntVec3 c, Map map) where TComp : ThingComp
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].TryGetComp<TComp>() != null)
			{
				return (ThingWithComps)thingList[i];
			}
		}
		return null;
	}

	public static T GetFirstThing<T>(this IntVec3 c, Map map) where T : Thing
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static bool TryGetFirstThing<T>(this IntVec3 c, Map map, out T thing) where T : Thing
	{
		thing = c.GetFirstThing<T>(map);
		return thing != null;
	}

	public static Thing GetFirstHaulable(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.designateHaulable)
			{
				return list[i];
			}
		}
		return null;
	}

	public static Thing GetFirstItem(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.category == ThingCategory.Item)
			{
				return list[i];
			}
		}
		return null;
	}

	public static IEnumerable<Thing> GetItems(this IntVec3 c, Map map)
	{
		List<Thing> thingList = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def.category == ThingCategory.Item)
			{
				yield return thingList[i];
			}
		}
	}

	public static int GetMaxItemsAllowedInCell(this IntVec3 c, Map map)
	{
		return c.GetEdifice(map)?.MaxItemsInCell ?? 1;
	}

	public static int GetItemCount(this IntVec3 c, Map map)
	{
		int num = 0;
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.category == ThingCategory.Item)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetAllItemsStackCount(this IntVec3 c, Map map, ThingDef itemDef)
	{
		int num = 0;
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def == itemDef)
			{
				num += list[i].stackCount;
			}
		}
		return num;
	}

	public static int GetItemStackSpaceLeftFor(this IntVec3 c, Map map, ThingDef itemDef)
	{
		int num = 0;
		int num2 = 0;
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.category == ThingCategory.Item)
			{
				num2++;
			}
			if (list[i].def == itemDef)
			{
				num += Mathf.Max(list[i].def.stackLimit - list[i].stackCount, 0);
			}
		}
		return num + Mathf.Max(c.GetMaxItemsAllowedInCell(map) - num2, 0) * itemDef.stackLimit;
	}

	public static bool IsBuildingInteractionCell(this IntVec3 cell, Map map)
	{
		for (int i = 0; i < 4; i++)
		{
			IntVec3 c = cell + GenAdj.CardinalDirections[i];
			if (c.InBounds(map))
			{
				Building edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.hasInteractionCell && edifice.InteractionCell == cell)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static Building GetFirstBuilding(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i] is Building result)
			{
				return result;
			}
		}
		return null;
	}

	public static Pawn GetFirstPawn(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Pawn result)
			{
				return result;
			}
		}
		return null;
	}

	public static Mineable GetFirstMineable(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Mineable result)
			{
				return result;
			}
		}
		return null;
	}

	public static Blight GetFirstBlight(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Blight result)
			{
				return result;
			}
		}
		return null;
	}

	public static Skyfaller GetFirstSkyfaller(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i] is Skyfaller result)
			{
				return result;
			}
		}
		return null;
	}

	public static IPlantToGrowSettable GetPlantToGrowSettable(this IntVec3 c, Map map)
	{
		IPlantToGrowSettable plantToGrowSettable = c.GetEdifice(map) as IPlantToGrowSettable;
		if (plantToGrowSettable == null)
		{
			plantToGrowSettable = c.GetZone(map) as IPlantToGrowSettable;
		}
		return plantToGrowSettable;
	}

	public static Building GetTransmitter(this IntVec3 c, Map map)
	{
		List<Thing> list = map.thingGrid.ThingsListAt(c);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].def.EverTransmitsPower)
			{
				return (Building)list[i];
			}
		}
		return null;
	}

	public static Building_Door GetDoor(this IntVec3 c, Map map)
	{
		if (c.GetEdifice(map) is Building_Door result)
		{
			return result;
		}
		return null;
	}

	public static Building GetFence(this IntVec3 c, Map map)
	{
		Building edifice = c.GetEdifice(map);
		if (edifice != null && edifice.def.IsFence)
		{
			return edifice;
		}
		return null;
	}

	public static Building GetEdifice(this IntVec3 c, Map map)
	{
		return map.edificeGrid[c];
	}

	public static Building GetEdificeSafe(this IntVec3 c, Map map)
	{
		if (!c.InBounds(map))
		{
			return null;
		}
		return map.edificeGrid[c];
	}

	public static Thing GetCover(this IntVec3 c, Map map)
	{
		return map.coverGrid[c];
	}

	public static Gas GetGas(this IntVec3 c, Map map)
	{
		List<Thing> thingList = c.GetThingList(map);
		for (int i = 0; i < thingList.Count; i++)
		{
			if (thingList[i].def.category == ThingCategory.Gas)
			{
				return (Gas)thingList[i];
			}
		}
		return null;
	}

	public static bool IsInPrisonCell(this IntVec3 c, Map map)
	{
		Room roomOrAdjacent = c.GetRoomOrAdjacent(map, RegionType.Set_All);
		if (roomOrAdjacent != null)
		{
			return roomOrAdjacent.IsPrisonCell;
		}
		IntVec3 intVec = c;
		Log.Error("Checking prison cell status of " + intVec.ToString() + " which is not in or adjacent to a room.");
		return false;
	}

	public static bool UsesOutdoorTemperature(this IntVec3 c, Map map)
	{
		Room room = c.GetRoom(map);
		if (room != null)
		{
			return room.UsesOutdoorTemperature;
		}
		Building edifice = c.GetEdifice(map);
		if (edifice != null)
		{
			IntVec3[] array = GenAdj.CellsAdjacent8Way(edifice).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].InBounds(map))
				{
					room = array[i].GetRoom(map);
					if (room != null && room.UsesOutdoorTemperature)
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	public static bool IsPolluted(this IntVec3 c, Map map)
	{
		return map.pollutionGrid.IsPolluted(c);
	}

	public static bool CanPollute(this IntVec3 c, Map map)
	{
		return map.pollutionGrid.CanPollute(c);
	}

	public static bool CanUnpollute(this IntVec3 c, Map map)
	{
		return map.pollutionGrid.CanUnpollute(c);
	}

	public static void Pollute(this IntVec3 c, Map map, bool silent = false)
	{
		map.pollutionGrid.SetPolluted(c, isPolluted: true, silent);
	}

	public static void Unpollute(this IntVec3 c, Map map)
	{
		map.pollutionGrid.SetPolluted(c, isPolluted: false);
		if (map.snowGrid.GetDepth(c) > float.Epsilon)
		{
			map.snowGrid.MakeMeshDirty(c);
		}
		if (ModsConfig.OdysseyActive && map.sandGrid.GetDepth(c) > float.Epsilon)
		{
			map.sandGrid.MakeMeshDirty(c);
		}
	}

	public static float GetFertility(this IntVec3 c, Map map)
	{
		return map.fertilityGrid.FertilityAt(c);
	}
}
