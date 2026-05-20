using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class DropCellFinder
	{
		private static List<Building> tmpColonyBuildings = new List<Building>();

		public static List<ShipLandingArea> tmpShipLandingAreas = new List<ShipLandingArea>();

		public static IntVec3 RandomDropSpot(Map map, bool standableOnly = true)
		{
			IntVec3 result = CellFinderLoose.RandomCellWith((IntVec3 c) => (!standableOnly || c.Standable(map)) && !c.GetTerrain(map).IsWater && !c.Roofed(map) && !c.Fogged(map), map);
			if (result.IsValid)
			{
				return result;
			}
			return CellFinderLoose.RandomCellWith((IntVec3 c) => (!standableOnly || c.Standable(map)) && !c.GetTerrain(map).IsWater && !c.Fogged(map), map);
		}

		public static IntVec3 TradeDropSpot(Map map)
		{
			IEnumerable<Building> collection = map.listerBuildings.allBuildingsColonist.Where((Building b) => b.def.IsCommsConsole);
			IEnumerable<Building> enumerable = map.listerBuildings.allBuildingsColonist.Where((Building b) => b.def.IsOrbitalTradeBeacon);
			Building building = enumerable.FirstOrDefault((Building b) => !map.roofGrid.Roofed(b.Position) && AnyAdjacentGoodDropSpot(b.Position, map, allowFogged: false, canRoofPunch: false));
			if (building != null)
			{
				IntVec3 position = building.Position;
				if (!TryFindDropSpotNear(position, map, out var result, allowFogged: false, canRoofPunch: false))
				{
					IntVec3 intVec = position;
					Log.Error("Could find no good TradeDropSpot near dropCenter " + intVec.ToString() + ". Using a random standable unfogged cell.");
					return CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(map) && !c.Fogged(map), map);
				}
				return result;
			}
			List<Building> list = new List<Building>();
			list.AddRange(enumerable);
			list.AddRange(collection);
			list.RemoveAll(delegate(Building b)
			{
				CompPowerTrader compPowerTrader = b.TryGetComp<CompPowerTrader>();
				return compPowerTrader != null && !compPowerTrader.PowerOn;
			});
			Predicate<IntVec3> validator = (IntVec3 c) => IsGoodDropSpot(c, map, allowFogged: false, canRoofPunch: false);
			if (!list.Any())
			{
				list.AddRange(map.listerBuildings.allBuildingsColonist);
				list.Shuffle();
				if (!list.Any())
				{
					return CellFinderLoose.RandomCellWith(validator, map);
				}
			}
			int num = 8;
			do
			{
				for (int num2 = 0; num2 < list.Count; num2++)
				{
					if (CellFinder.TryFindRandomCellNear(list[num2].Position, map, num, validator, out var position))
					{
						return position;
					}
				}
				num = Mathf.RoundToInt((float)num * 1.1f);
			}
			while (num <= map.Size.x);
			Log.Error("Failed to generate trade drop center. Giving random.");
			return CellFinderLoose.RandomCellWith(validator, map);
		}

		public static IntVec3 TryFindSafeLandingSpotCloseToColony(Map map, IntVec2 size, Faction faction = null, int borderWidth = 2)
		{
			size.x += borderWidth;
			size.z += borderWidth;
			tmpColonyBuildings.Clear();
			tmpColonyBuildings.AddRange(map.listerBuildings.allBuildingsColonist);
			if (!tmpColonyBuildings.Any())
			{
				return CellFinderLoose.RandomCellWith(SpotValidator, map);
			}
			tmpColonyBuildings.Shuffle();
			for (int i = 0; i < tmpColonyBuildings.Count; i++)
			{
				if (TryFindDropSpotNear(tmpColonyBuildings[i].Position, map, out var result, allowFogged: false, canRoofPunch: false, allowIndoors: false, size) && SkyfallerCanLandAt(result, map, size, faction))
				{
					tmpColonyBuildings.Clear();
					return result;
				}
			}
			tmpColonyBuildings.Clear();
			return CellFinderLoose.RandomCellWith(SpotValidator, map);
			bool SpotValidator(IntVec3 c)
			{
				if (!SkyfallerCanLandAt(c, map, size, faction))
				{
					return false;
				}
				if (c.Roofed(map))
				{
					return false;
				}
				if (c.GetTerrain(map).IsWater)
				{
					return false;
				}
				if (ModsConfig.RoyaltyActive)
				{
					List<Thing> list = map.listerThings.ThingsOfDef(ThingDefOf.ActivatorProximity);
					for (int j = 0; j < list.Count; j++)
					{
						if (list[j].Faction != null && list[j].Faction.HostileTo(faction))
						{
							CompSendSignalOnMotion compSendSignalOnMotion = list[j].TryGetComp<CompSendSignalOnMotion>();
							if (compSendSignalOnMotion != null && c.InHorDistOf(list[j].Position, compSendSignalOnMotion.Props.radius + 10f))
							{
								return false;
							}
						}
					}
				}
				return true;
			}
		}

		public static bool SkyfallerCanLandAt(IntVec3 c, Map map, IntVec2 size, Faction faction = null)
		{
			CellRect cellRect = GenAdj.OccupiedRect(c, Rot4.North, size);
			if (!cellRect.InBounds(map))
			{
				return false;
			}
			if (!IsSafeDropSpot(c, map, faction, size, 5))
			{
				return false;
			}
			foreach (IntVec3 item in cellRect)
			{
				List<Thing> thingList = item.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Thing thing = thingList[i];
					if (thing is IActiveTransporter || thing is Skyfaller)
					{
						return false;
					}
					if (thing.def.preventSkyfallersLandingOn)
					{
						return false;
					}
					ThingCategory category = thing.def.category;
					if (category == ThingCategory.Item || category == ThingCategory.Building)
					{
						return false;
					}
				}
			}
			return true;
		}

		public static IntVec3 GetBestShuttleLandingSpot(Map map, Faction factionForFindingSpot)
		{
			Thing firstBlockingThing;
			return GetBestShuttleLandingSpot(map, factionForFindingSpot, out firstBlockingThing);
		}

		public static IntVec3 GetBestShuttleLandingSpot(Map map, Faction factionForFindingSpot, out Thing firstBlockingThing)
		{
			if (!TryFindShipLandingArea(map, out var result, out firstBlockingThing))
			{
				result = TryFindSafeLandingSpotCloseToColony(map, ThingDefOf.Shuttle.Size, factionForFindingSpot);
			}
			if (!result.IsValid && !FindSafeLandingSpot(out result, factionForFindingSpot, map, 35, 15, 25, ThingDefOf.Shuttle.Size + new IntVec2(2, 2)))
			{
				IntVec3 intVec = RandomDropSpot(map);
				if (!intVec.IsValid)
				{
					intVec = RandomDropSpot(map, standableOnly: false);
				}
				if (!TryFindDropSpotNear(intVec, map, out result, allowFogged: false, canRoofPunch: false, allowIndoors: false, ThingDefOf.Shuttle.Size + new IntVec2(2, 2)))
				{
					result = intVec;
				}
			}
			return result;
		}

		public static bool TryFindShipLandingArea(Map map, out IntVec3 result, out Thing firstBlockingThing)
		{
			tmpShipLandingAreas.Clear();
			List<ShipLandingArea> landingZones = ShipLandingBeaconUtility.GetLandingZones(map);
			if (landingZones.Any())
			{
				for (int i = 0; i < landingZones.Count; i++)
				{
					if (landingZones[i].Clear)
					{
						tmpShipLandingAreas.Add(landingZones[i]);
					}
				}
				if (tmpShipLandingAreas.Any())
				{
					result = tmpShipLandingAreas.RandomElement().CenterCell;
					firstBlockingThing = null;
					tmpShipLandingAreas.Clear();
					return true;
				}
				firstBlockingThing = landingZones[0].FirstBlockingThing;
			}
			else
			{
				firstBlockingThing = null;
			}
			result = IntVec3.Invalid;
			tmpShipLandingAreas.Clear();
			return false;
		}

		public static bool TryFindDropSpotNear(IntVec3 center, Map map, out IntVec3 result, bool allowFogged, bool canRoofPunch, bool allowIndoors = true, IntVec2? size = null, bool mustBeReachableFromCenter = true)
		{
			return TryFindDropSpotNear(center, map, out result, allowFogged, canRoofPunch, 16, allowIndoors, size, mustBeReachableFromCenter);
		}

		public static bool TryFindDropSpotNear(IntVec3 center, Map map, out IntVec3 result, bool allowFogged, bool canRoofPunch, int maxRadius, bool allowIndoors = true, IntVec2? size = null, bool mustBeReachableFromCenter = true)
		{
			if (DebugViewSettings.drawDestSearch)
			{
				map.debugDrawer.FlashCell(center, 1f, "center");
			}
			Room centerRoom = center.GetRoom(map);
			if (allowIndoors && canRoofPunch && centerRoom != null && !centerRoom.PsychologicallyOutdoors)
			{
				if (TryFindCell(SameRoomValidator, out result))
				{
					return true;
				}
				if (TryFindCell(AnyRoomValidator, out result))
				{
					return true;
				}
			}
			return TryFindCell(Validator, out result);
			bool AnyRoomValidator(IntVec3 c)
			{
				if (!Validator(c))
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (room != null)
				{
					return !room.PsychologicallyOutdoors;
				}
				return false;
			}
			bool SameRoomValidator(IntVec3 c)
			{
				if (!Validator(c))
				{
					return false;
				}
				return c.GetRoom(map) == centerRoom;
			}
			bool TryFindCell(Predicate<IntVec3> validator, out IntVec3 cell)
			{
				int num = Mathf.Min(maxRadius / 2, 5);
				int num2 = Mathf.Min(maxRadius / 5, 1);
				int num3 = num;
				do
				{
					if (CellFinder.TryFindRandomCellNear(center, map, num3, validator, out cell))
					{
						return true;
					}
					num3 += num2;
				}
				while (num3 <= maxRadius);
				cell = center;
				return false;
			}
			bool Validator(IntVec3 c)
			{
				if (!c.InBounds(map))
				{
					return false;
				}
				if (size.HasValue)
				{
					CellRect cellRect = c.RectAbout(size.Value);
					if (!cellRect.InBounds(map))
					{
						return false;
					}
					foreach (IntVec3 item in cellRect)
					{
						if (!IsGoodDropSpot(item, map, allowFogged, canRoofPunch, allowIndoors))
						{
							return false;
						}
					}
				}
				else if (!IsGoodDropSpot(c, map, allowFogged, canRoofPunch, allowIndoors))
				{
					return false;
				}
				if (mustBeReachableFromCenter && !map.reachability.CanReach(center, c, PathEndMode.OnCell, TraverseMode.PassDoors, Danger.Deadly))
				{
					return false;
				}
				return true;
			}
		}

		public static bool IsGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch, bool allowIndoors = true)
		{
			if (map == null || !c.InBounds(map) || !c.Standable(map))
			{
				return false;
			}
			if (!CanPhysicallyDropInto(c, map, canRoofPunch, allowIndoors))
			{
				if (DebugViewSettings.drawDestSearch)
				{
					map.debugDrawer.FlashCell(c, 0f, "phys");
				}
				return false;
			}
			if (Current.ProgramState == ProgramState.Playing && !allowFogged && c.Fogged(map))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is IActiveTransporter || thing is Skyfaller)
				{
					return false;
				}
				if (!(thing is Building { IsClearableFreeBuilding: not false }))
				{
					if (thing.def.IsEdifice())
					{
						return false;
					}
					if (thing.def.preventSkyfallersLandingOn)
					{
						return false;
					}
					if (thing.def.category != ThingCategory.Plant && GenSpawn.SpawningWipes(ThingDefOf.ActiveDropPod, thing.def))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool AnyAdjacentGoodDropSpot(IntVec3 c, Map map, bool allowFogged, bool canRoofPunch)
		{
			if (!IsGoodDropSpot(c + IntVec3.North, map, allowFogged, canRoofPunch) && !IsGoodDropSpot(c + IntVec3.East, map, allowFogged, canRoofPunch) && !IsGoodDropSpot(c + IntVec3.South, map, allowFogged, canRoofPunch))
			{
				return IsGoodDropSpot(c + IntVec3.West, map, allowFogged, canRoofPunch);
			}
			return true;
		}

		public static IntVec3 FindRaidDropCenterDistant(Map map, bool allowRoofed = false, bool allowWater = true)
		{
			Faction hostFaction = map.ParentFaction ?? Faction.OfPlayer;
			IEnumerable<Thing> first = map.mapPawns.FreeHumanlikesSpawnedOfFaction(hostFaction);
			first = ((hostFaction != Faction.OfPlayer) ? first.Concat(from x in map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
				where x.Faction == hostFaction
				select x) : first.Concat(map.listerBuildings.allBuildingsColonist));
			int num = 0;
			float num2 = 65f;
			foreach (IntVec3 item in map.AllCells.Where(delegate(IntVec3 cell)
			{
				if (!CanPhysicallyDropInto(cell, map, canRoofPunch: true, allowedIndoors: false))
				{
					return false;
				}
				if (cell.Fogged(map))
				{
					return false;
				}
				if (cell.DistanceToEdge(map) < 5)
				{
					return false;
				}
				if (!allowRoofed && cell.Roofed(map))
				{
					return false;
				}
				return (allowWater || !cell.GetTerrain(map).IsWater) ? true : false;
			}).InRandomOrder())
			{
				num++;
				if (num > 300)
				{
					return item;
				}
				num2 -= 0.2f;
				bool flag = false;
				foreach (Thing item2 in first)
				{
					if ((float)(item - item2.Position).LengthHorizontalSquared < num2 * num2)
					{
						flag = true;
						break;
					}
				}
				if (!flag && map.reachability.CanReachFactionBase(item, hostFaction))
				{
					return item;
				}
			}
			return map.AllCells.RandomElement();
		}

		public static bool TryFindRaidDropCenterClose(out IntVec3 spot, Map map, bool canRoofPunch = true, bool allowIndoors = true, bool closeWalk = true, int maxRadius = -1)
		{
			Faction parentFaction = map.ParentFaction;
			if (parentFaction == null)
			{
				return RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CanPhysicallyDropInto(x, map, canRoofPunch, allowIndoors) && !x.Fogged(map) && x.Standable(map), map, out spot);
			}
			int num = 0;
			do
			{
				IntVec3 result = IntVec3.Invalid;
				if (map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).Any())
				{
					result = map.mapPawns.FreeHumanlikesSpawnedOfFaction(parentFaction).RandomElement().Position;
				}
				else
				{
					if (parentFaction == Faction.OfPlayer)
					{
						List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
						for (int num2 = 0; num2 < allBuildingsColonist.Count && !TryFindDropSpotNear(allBuildingsColonist[num2].Position, map, out result, allowFogged: true, canRoofPunch, allowIndoors); num2++)
						{
						}
					}
					else
					{
						List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
						for (int num3 = 0; num3 < list.Count && (list[num3].Faction != parentFaction || !TryFindDropSpotNear(list[num3].Position, map, out result, allowFogged: true, canRoofPunch, allowIndoors)); num3++)
						{
						}
					}
					if (!result.IsValid)
					{
						RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith((IntVec3 x) => CanPhysicallyDropInto(x, map, canRoofPunch, allowIndoors) && !x.Fogged(map) && x.Standable(map), map, out result);
					}
				}
				int num4 = ((maxRadius >= 0) ? maxRadius : 10);
				if (!closeWalk)
				{
					CellFinder.TryFindRandomCellNear(result, map, num4 * num4, null, out spot, 50);
				}
				else
				{
					spot = CellFinder.RandomClosewalkCellNear(result, map, num4);
				}
				if (CanPhysicallyDropInto(spot, map, canRoofPunch, allowIndoors) && !spot.Fogged(map))
				{
					return true;
				}
				num++;
			}
			while (num <= 300);
			spot = CellFinderLoose.RandomCellWith((IntVec3 c) => CanPhysicallyDropInto(c, map, canRoofPunch, allowIndoors), map);
			return false;
		}

		public static bool FindSafeLandingSpot(out IntVec3 spot, Faction faction, Map map, int distToHostiles = 35, int distToFires = 15, int distToEdge = 25, IntVec2? size = null, IntVec3? interactionCell = null)
		{
			spot = IntVec3.Invalid;
			int num = 200;
			while (num-- > 0)
			{
				IntVec3 intVec = RandomDropSpot(map);
				if (IsSafeDropSpot(intVec, map, faction, size, distToEdge, distToHostiles, distToFires, interactionCell))
				{
					spot = intVec;
					return true;
				}
			}
			return false;
		}

		public static bool FindSafeLandingSpotNearAvoidingHostiles(Thing thing, Map map, out IntVec3 spot, int distToHostiles = 35, int distToFires = 15, int distToEdge = 25, IntVec2? size = null, IntVec3? interactionCell = null)
		{
			return RCellFinder.TryFindRandomSpotNearAvoidingHostilePawns(thing, map, (IntVec3 cell) => IsSafeDropSpot(cell, map, thing.Faction, size, distToEdge, distToHostiles, distToFires, interactionCell), out spot);
		}

		public static bool CanPhysicallyDropInto(IntVec3 c, Map map, bool canRoofPunch, bool allowedIndoors = true)
		{
			if (!c.Walkable(map))
			{
				return false;
			}
			if (c.GetTerrain(map).exposesToVacuum)
			{
				return false;
			}
			if (c.GetTerrain(map).IsWater)
			{
				return false;
			}
			RoofDef roof = c.GetRoof(map);
			if (roof != null)
			{
				if (!canRoofPunch)
				{
					return false;
				}
				if (roof.isThickRoof)
				{
					return false;
				}
			}
			if (!allowedIndoors)
			{
				Room room = c.GetRoom(map);
				if (room != null && !room.PsychologicallyOutdoors)
				{
					return false;
				}
			}
			if (!canRoofPunch)
			{
				foreach (Building allBuildingsAnimalPenMarker in map.listerBuildings.allBuildingsAnimalPenMarkers)
				{
					if (allBuildingsAnimalPenMarker.Position.GetDistrict(map) == c.GetDistrict(map))
					{
						CompAnimalPenMarker compAnimalPenMarker = allBuildingsAnimalPenMarker.TryGetComp<CompAnimalPenMarker>();
						if (compAnimalPenMarker != null && map.animalPenManager.GetPenMarkerState(compAnimalPenMarker).Enclosed)
						{
							return false;
						}
					}
				}
			}
			return true;
		}

		private static bool IsSafeDropSpot(IntVec3 cell, Map map, Faction faction, IntVec2? size = null, int distToEdge = 25, int distToHostiles = 35, int distToFires = 15, IntVec3? interactionCell = null)
		{
			Faction factionBaseFaction = map.ParentFaction ?? Faction.OfPlayer;
			if (!cell.InBounds(map))
			{
				return false;
			}
			if (size.HasValue)
			{
				CellRect cellRect = GenAdj.OccupiedRect(cell, Rot4.North, size.Value);
				if (!cellRect.InBounds(map))
				{
					return false;
				}
				foreach (IntVec3 item in cellRect)
				{
					if (!item.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
					{
						return false;
					}
					if (!IsGoodDropSpot(item, map, allowFogged: false, canRoofPunch: false, allowIndoors: false))
					{
						return false;
					}
				}
			}
			else if (!IsGoodDropSpot(cell, map, allowFogged: false, canRoofPunch: false, allowIndoors: false))
			{
				return false;
			}
			if (interactionCell.HasValue && !IsGoodDropSpot(cell + interactionCell.Value, map, allowFogged: false, canRoofPunch: false, allowIndoors: false))
			{
				return false;
			}
			if (distToEdge > 0 && cell.CloseToEdge(map, distToEdge))
			{
				return false;
			}
			if (faction != null)
			{
				foreach (IAttackTarget item2 in map.attackTargetsCache.TargetsHostileToFaction(faction))
				{
					if (!item2.ThreatDisabled(null) && item2.Thing.Position.InHorDistOf(cell, distToHostiles))
					{
						return false;
					}
				}
			}
			if (!map.reachability.CanReachFactionBase(cell, factionBaseFaction))
			{
				return false;
			}
			if (size.HasValue)
			{
				foreach (IntVec3 cell2 in CellRect.CenteredOn(cell, size.Value.x, size.Value.z).Cells)
				{
					if (CellHasCrops(cell2))
					{
						return false;
					}
				}
			}
			else if (CellHasCrops(cell))
			{
				return false;
			}
			float minDistToFiresSq = distToFires * distToFires;
			float closestDistSq = float.MaxValue;
			int firesCount = 0;
			RegionTraverser.BreadthFirstTraverse(cell, map, (Region from, Region to) => true, delegate(Region x)
			{
				List<Thing> list = x.ListerThings.ThingsInGroup(ThingRequestGroup.Fire);
				for (int i = 0; i < list.Count; i++)
				{
					float num = cell.DistanceToSquared(list[i].Position);
					if (!(num > minDistToFiresSq))
					{
						if (num < closestDistSq)
						{
							closestDistSq = num;
						}
						firesCount++;
					}
				}
				return closestDistSq <= minDistToFiresSq && firesCount >= 5;
			}, 15);
			if (closestDistSq <= minDistToFiresSq && firesCount >= 5)
			{
				return false;
			}
			return true;
			bool CellHasCrops(IntVec3 c)
			{
				Plant plant = c.GetPlant(map);
				if (plant != null && plant.sown)
				{
					return map.zoneManager.ZoneAt(c) is Zone_Growing;
				}
				return false;
			}
		}
	}
}
