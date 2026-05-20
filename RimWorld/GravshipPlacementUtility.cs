using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;

namespace RimWorld;

public static class GravshipPlacementUtility
{
	public enum ClearMode
	{
		All,
		AllButNonTreePlants,
		BlockingBuildingsOnly,
		NaturalRockOnly
	}

	public static bool placingGravship;

	public static void PlaceGravshipInMap(Gravship gravship, IntVec3 root, Map map, out List<Thing> spawned)
	{
		spawned = null;
		if (ModsConfig.OdysseyActive)
		{
			new ProfilerBlock("GravshipPlacementUtility.PlaceGravshipInMap");
			placingGravship = true;
			spawned = gravship.ThingPlacements.Select((KeyValuePair<Thing, PositionData.Data> x) => x.Key).ToList();
			spawned.SortBy(GravshipUtility.ThingSpawnPriority);
			map.regionAndRoomUpdater.Enabled = true;
			SpawnFoundations(gravship, map, root);
			SpawnTerrain(gravship, map, root);
			SpawnGas(gravship, map, root);
			CopyZonesIntoMap(gravship, map, root);
			CopyDesignations(gravship, map, root);
			SpawnNonPawnThings(gravship, map, spawned, root);
			SpawnPawns(gravship, map, root);
			SpawnRoofs(gravship, map, root);
			CopyAreasIntoMap(gravship, map, root);
			GravshipUtility.UpdateBillDestinations(map);
			PowerBuildings(gravship);
			gravship.StoryState.CopyTo(map.storyState);
			map.autoSlaughterManager.configs = gravship.autoSlaughterConfigs;
			map.autoSlaughterManager.Notify_ConfigChanged();
			map.substructureGrid.MarkDirty();
			placingGravship = false;
		}
	}

	private static void SpawnFoundations(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Substructure Foundation"))
		{
			SpawnTerrain(map, root, gravship.Foundations, clear: true);
		}
	}

	private static void SpawnTerrain(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Substructure Terrain"))
		{
			SpawnTerrain(map, root, gravship.Terrains);
			foreach (var terrainColor in gravship.TerrainColors)
			{
				IntVec3 item = terrainColor.Item1;
				ColorDef item2 = terrainColor.Item2;
				IntVec3 c = root + item;
				if (c.InBounds(map))
				{
					map.terrainGrid.SetTerrainColor(c, item2);
				}
			}
		}
	}

	private static void SpawnPawns(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Pawns"))
		{
			foreach (var (pawn2, data2) in gravship.PawnPlacements)
			{
				if (pawn2.Discarded || pawn2.Destroyed)
				{
					continue;
				}
				GenSpawn.Spawn(pawn2, root + data2.local, map, data2.rotation);
				if (pawn2.drafter != null)
				{
					pawn2.drafter.Drafted = data2.drafted;
				}
				if (pawn2.Downed || pawn2.Deathresting)
				{
					Building_Bed building_Bed = map.thingGrid.ThingAt<Building_Bed>(pawn2.Position);
					if (building_Bed != null)
					{
						RestUtility.TuckIntoBed(building_Bed, pawn2, pawn2, rescued: false);
					}
				}
			}
		}
	}

	private static void SpawnNonPawnThings(Gravship gravship, Map map, List<Thing> gravshipThings, IntVec3 root)
	{
		using (new ProfilerBlock("Things"))
		{
			foreach (Thing gravshipThing in gravshipThings)
			{
				PositionData.Data data = gravship.ThingPlacements[gravshipThing];
				if (!gravshipThing.Destroyed)
				{
					if (gravshipThing is PawnFlyer pawnFlyer)
					{
						pawnFlyer.Notify_TransportedOnGravship(gravship);
					}
					GenSpawn.Spawn(gravshipThing, root + data.local, map, data.rotation);
				}
			}
		}
	}

	private static void SpawnRoofs(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Roofs"))
		{
			foreach (var (intVec, def) in gravship.Roofs)
			{
				map.roofGrid.SetRoof(root + intVec, def);
			}
		}
	}

	private static void SpawnGas(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Gas"))
		{
			foreach (var gase in gravship.Gases)
			{
				IntVec3 item = gase.Item1;
				uint item2 = gase.Item2;
				IntVec3 intVec = root + item;
				map.gasGrid.SetDirect(intVec, item2);
				map.mapDrawer.MapMeshDirty(intVec, MapMeshFlagDefOf.Gas);
			}
		}
	}

	private static void PowerBuildings(Gravship gravship)
	{
		using (new ProfilerBlock("Power"))
		{
			Thing value;
			foreach (KeyValuePair<Thing, Thing> connectParent in gravship.ConnectParents)
			{
				connectParent.Deconstruct(out var key, out value);
				Thing thing = key;
				Thing thing2 = value;
				if (thing2 != null && thing2.Spawned)
				{
					thing.TryGetComp<CompPower>().ConnectToTransmitter(thing2.TryGetComp<CompPower>());
				}
			}
			foreach (KeyValuePair<Thing, bool> item in gravship.PoweredOn)
			{
				item.Deconstruct(out value, out var value2);
				Thing thing3 = value;
				bool powerOn = value2;
				thing3.TryGetComp<CompPowerTrader>().PowerOn = powerOn;
			}
		}
	}

	internal static void PostSwapMap(Gravship gravship, List<Thing> gravshipThings)
	{
		foreach (Thing gravshipThing in gravshipThings)
		{
			if (gravshipThing != null && !gravshipThing.Destroyed)
			{
				gravshipThing.PostSwapMap();
			}
		}
		foreach (Pawn pawn in gravship.Pawns)
		{
			if (pawn != null && !pawn.Destroyed)
			{
				pawn.PostSwapMap();
			}
		}
	}

	private static void SpawnTerrain(Map map, IntVec3 root, Dictionary<IntVec3, TerrainDef> data, bool clear = false)
	{
		foreach (KeyValuePair<IntVec3, TerrainDef> datum in data)
		{
			datum.Deconstruct(out var key, out var value);
			IntVec3 intVec = key;
			TerrainDef terrainDef = value;
			IntVec3 intVec2 = root + intVec;
			if (!intVec2.InBounds(map))
			{
				continue;
			}
			if (clear)
			{
				ClearThingsAt(intVec2, map, ClearMode.All);
			}
			map.snowGrid.SetDepth(intVec2, 0f);
			map.sandGrid.SetDepth(intVec2, 0f);
			if (!terrainDef.isFoundation && !intVec2.SupportsStructureType(map, terrainDef.terrainAffordanceNeeded))
			{
				map.terrainGrid.SetTerrain(intVec2, map.Biome.TerrainForAffordance(terrainDef.terrainAffordanceNeeded));
			}
			if (terrainDef.isFoundation)
			{
				if (map.terrainGrid.CanRemoveTopLayerAt(intVec2))
				{
					map.terrainGrid.RemoveTopLayer(intVec2, doLeavings: false);
				}
				map.terrainGrid.SetFoundation(intVec2, terrainDef);
			}
			else
			{
				map.terrainGrid.SetTerrain(intVec2, terrainDef);
			}
			map.fogGrid.FloodUnfogAdjacent(intVec2);
		}
	}

	public static void ClearAreaForGravship(Map map, IntVec3 root, HashSet<IntVec3> clearCells)
	{
		ClearArea(map, root, clearCells, ClearMode.AllButNonTreePlants);
	}

	public static void ClearArea(Map map, IntVec3 root, HashSet<IntVec3> clearCells, ClearMode mode)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return;
		}
		MapGenerator.TryGetVar<bool>("DontGenerateClearedGravShipTerrain", out var var);
		if (var)
		{
			return;
		}
		foreach (IntVec3 clearCell in clearCells)
		{
			IntVec3 intVec = root + clearCell;
			if (!intVec.InBounds(map))
			{
				continue;
			}
			if (mode == ClearMode.All || mode == ClearMode.AllButNonTreePlants)
			{
				if (ModsConfig.BiotechActive)
				{
					map.pollutionGrid.SetPolluted(intVec, isPolluted: false, silent: true);
				}
				if (!intVec.SupportsStructureType(map, TerrainAffordanceDefOf.Walkable))
				{
					TerrainDef replacementTerrain = GetReplacementTerrain(intVec.GetTerrain(map), map);
					if (replacementTerrain != null)
					{
						map.terrainGrid.SetTerrain(intVec, replacementTerrain);
					}
				}
			}
			ClearThingsAt(intVec, map, mode);
			if (ShouldRemoveRoof(map, intVec, mode))
			{
				map.roofGrid.SetRoof(intVec, null);
			}
			if (map.roofGrid.RoofAt(intVec) == null)
			{
				map.fogGrid.FloodUnfogAdjacent(intVec);
			}
		}
	}

	private static bool ShouldRemoveRoof(Map map, IntVec3 cell, ClearMode mode)
	{
		if (mode != ClearMode.NaturalRockOnly)
		{
			return true;
		}
		Room room = cell.GetRoom(map);
		if (room != null && room.ProperRoom)
		{
			return false;
		}
		Building edifice = cell.GetEdifice(map);
		if (edifice != null && !edifice.def.building.isNaturalRock)
		{
			return false;
		}
		return map.roofGrid.RoofAt(cell) != RoofDefOf.RoofConstructed;
	}

	private static TerrainDef GetReplacementTerrain(TerrainDef original, Map map)
	{
		if (original == TerrainDefOf.Space)
		{
			return null;
		}
		if (original.gravshipReplacementTerrain != null)
		{
			return original.gravshipReplacementTerrain;
		}
		if (original.passability == Traversability.Impassable || original.dangerous)
		{
			return map.Biome.TerrainForAffordance(TerrainAffordanceDefOf.Medium);
		}
		return null;
	}

	private static void ClearThingsAt(IntVec3 cell, Map map, ClearMode clearMode)
	{
		map.zoneManager.ZoneAt(cell)?.RemoveCell(cell);
		map.areaManager.BuildRoof[cell] = false;
		map.areaManager.NoRoof[cell] = false;
		List<Thing> list = map.thingGrid.ThingsListAt(cell);
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Thing thing = list[num];
			if (thing.def.destroyable && !thing.Destroyed)
			{
				switch (clearMode)
				{
				case ClearMode.All:
					thing.Destroy();
					break;
				case ClearMode.AllButNonTreePlants:
					if (thing.def.IsPlant)
					{
						PlantProperties plant = thing.def.plant;
						if (plant == null || !plant.IsTree)
						{
							break;
						}
					}
					thing.Destroy();
					break;
				case ClearMode.NaturalRockOnly:
				{
					BuildingProperties building = thing.def.building;
					if (building != null && building.isNaturalRock)
					{
						thing.Destroy();
					}
					break;
				}
				case ClearMode.BlockingBuildingsOnly:
				{
					BuildingProperties building = thing.def.building;
					if (building != null && !building.canLandGravshipOn)
					{
						thing.Destroy();
					}
					break;
				}
				}
			}
		}
	}

	private static void CopyZonesIntoMap(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Zones"))
		{
			foreach (MoveableStockpile stockpile in gravship.areas.stockpiles)
			{
				stockpile?.TryCreateStockpile(map.zoneManager, root);
			}
			foreach (MoveableGrowZone growZone in gravship.areas.growZones)
			{
				growZone?.TryCreateGrowZone(map.zoneManager, root);
			}
		}
	}

	private static void CopyAreasIntoMap(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Areas"))
		{
			foreach (MoveableArea_Allowed allowedArea in gravship.areas.allowedAreas)
			{
				allowedArea.TryCreateArea(map.areaManager, root);
			}
			if (gravship.areas.homeArea != null)
			{
				foreach (IntVec3 relativeCell in gravship.areas.homeArea.RelativeCells)
				{
					map.areaManager.Home[root + relativeCell] = true;
				}
				foreach (Pawn assignedPawn in gravship.areas.homeArea.assignedPawns)
				{
					assignedPawn.playerSettings.AreaRestrictionInPawnCurrentMap = map.areaManager.Home;
				}
			}
			if (gravship.areas.buildRoofArea != null)
			{
				foreach (IntVec3 relativeCell2 in gravship.areas.buildRoofArea.RelativeCells)
				{
					map.areaManager.BuildRoof[root + relativeCell2] = true;
				}
			}
			if (gravship.areas.noRoofArea != null)
			{
				foreach (IntVec3 relativeCell3 in gravship.areas.noRoofArea.RelativeCells)
				{
					map.areaManager.NoRoof[root + relativeCell3] = true;
				}
			}
			if (gravship.areas.snowClearArea != null)
			{
				foreach (IntVec3 relativeCell4 in gravship.areas.snowClearArea.RelativeCells)
				{
					map.areaManager.SnowOrSandClear[root + relativeCell4] = true;
				}
			}
			if (ModsConfig.BiotechActive && gravship.areas.pollutionClearArea != null)
			{
				foreach (IntVec3 relativeCell5 in gravship.areas.pollutionClearArea.RelativeCells)
				{
					map.areaManager.PollutionClear[root + relativeCell5] = true;
				}
			}
			foreach (MoveableStorageGroup storageGroup in gravship.areas.storageGroups)
			{
				storageGroup.TryCreateStorageGroup(map);
			}
		}
	}

	private static void CopyDesignations(Gravship gravship, Map map, IntVec3 root)
	{
		using (new ProfilerBlock("Designations"))
		{
			foreach (var (intVec, designation) in gravship.TerrainDesignations)
			{
				if (designation != null)
				{
					designation.target = root + intVec;
					map.designationManager.AddDesignation(designation);
				}
			}
		}
	}

	public static void ApplyTemperatureVacuumFromBase(Gravship gravship, IntVec3 root, Map map)
	{
		if (!ModsConfig.OdysseyActive || gravship == null)
		{
			return;
		}
		foreach (RoomTemperatureVacuum.Data roomTemperature in gravship.RoomTemperatures)
		{
			Room room = (root + roomTemperature.local).GetRoom(map);
			if (room != null && !room.UsesOutdoorTemperature)
			{
				room.Temperature = roomTemperature.temperature;
				room.Vacuum = (room.ExposedToSpace ? 1f : roomTemperature.vacuum);
			}
		}
	}

	public static HashSet<IntVec3> GetCellsAdjacentToSubstructure(IEnumerable<CellRect> occupiedRects, int expansion = 1)
	{
		if (!ModsConfig.OdysseyActive)
		{
			return null;
		}
		HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
		foreach (CellRect occupiedRect in occupiedRects)
		{
			CellRect cellRect = occupiedRect.ExpandedBy(expansion);
			for (int i = cellRect.minX; i <= cellRect.maxX; i++)
			{
				for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
				{
					hashSet.Add(new IntVec3(i, 0, j));
				}
			}
		}
		return hashSet;
	}
}
