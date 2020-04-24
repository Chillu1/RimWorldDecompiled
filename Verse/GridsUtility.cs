using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class GridsUtility
	{
		public static float GetTemperature(this IntVec3 loc, Map map)
		{
			return GenTemperature.GetTemperatureForCell(loc, map);
		}

		public static Region GetRegion(this IntVec3 loc, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			return RegionAndRoomQuery.RegionAt(loc, map, allowedRegionTypes);
		}

		public static Room GetRoom(this IntVec3 loc, Map map, RegionType allowedRegionTypes = RegionType.Set_Passable)
		{
			return RegionAndRoomQuery.RoomAt(loc, map, allowedRegionTypes);
		}

		public static RoomGroup GetRoomGroup(this IntVec3 loc, Map map)
		{
			return RegionAndRoomQuery.RoomGroupAt(loc, map);
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

		public static bool Fogged(this Thing t)
		{
			return t.Map.fogGrid.IsFogged(t.Position);
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
			return map.terrainGrid.TerrainAt(c);
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
				T val = thingList[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return null;
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

		public static Building GetFirstBuilding(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Building building = list[i] as Building;
				if (building != null)
				{
					return building;
				}
			}
			return null;
		}

		public static Pawn GetFirstPawn(this IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Pawn pawn = thingList[i] as Pawn;
				if (pawn != null)
				{
					return pawn;
				}
			}
			return null;
		}

		public static Mineable GetFirstMineable(this IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Mineable mineable = thingList[i] as Mineable;
				if (mineable != null)
				{
					return mineable;
				}
			}
			return null;
		}

		public static Blight GetFirstBlight(this IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Blight blight = thingList[i] as Blight;
				if (blight != null)
				{
					return blight;
				}
			}
			return null;
		}

		public static Skyfaller GetFirstSkyfaller(this IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Skyfaller skyfaller = thingList[i] as Skyfaller;
				if (skyfaller != null)
				{
					return skyfaller;
				}
			}
			return null;
		}

		public static IPlantToGrowSettable GetPlantToGrowSettable(this IntVec3 c, Map map)
		{
			IPlantToGrowSettable plantToGrowSettable = c.GetEdifice(map) as IPlantToGrowSettable;
			if (plantToGrowSettable == null)
			{
				plantToGrowSettable = (c.GetZone(map) as IPlantToGrowSettable);
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
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Building_Door building_Door = list[i] as Building_Door;
				if (building_Door != null)
				{
					return building_Door;
				}
			}
			return null;
		}

		public static Building GetEdifice(this IntVec3 c, Map map)
		{
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
			Room roomOrAdjacent = c.GetRoomOrAdjacent(map);
			if (roomOrAdjacent != null)
			{
				return roomOrAdjacent.isPrisonCell;
			}
			Log.Error("Checking prison cell status of " + c + " which is not in or adjacent to a room.");
			return false;
		}

		public static bool UsesOutdoorTemperature(this IntVec3 c, Map map)
		{
			Room room = c.GetRoom(map, RegionType.Set_All);
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
						room = array[i].GetRoom(map, RegionType.Set_All);
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
	}
}
