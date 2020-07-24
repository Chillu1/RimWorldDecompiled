using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class RCellFinder
	{
		private static List<Region> regions = new List<Region>();

		private static HashSet<Thing> tmpBuildings = new HashSet<Thing>();

		private static List<Thing> tmpSpotThings = new List<Thing>();

		public static IntVec3 BestOrderedGotoDestNear(IntVec3 root, Pawn searcher)
		{
			Map map = searcher.Map;
			Predicate<IntVec3> predicate = delegate(IntVec3 c)
			{
				if (!map.pawnDestinationReservationManager.CanReserve(c, searcher, draftedOnly: true) || !c.Standable(map) || !searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
				{
					return false;
				}
				List<Thing> thingList = c.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					Pawn pawn = thingList[i] as Pawn;
					if (pawn != null && pawn != searcher && pawn.RaceProps.Humanlike && ((searcher.Faction == Faction.OfPlayer && pawn.Faction == searcher.Faction) || (searcher.Faction != Faction.OfPlayer && pawn.Faction != Faction.OfPlayer)))
					{
						return false;
					}
				}
				return true;
			};
			if (predicate(root))
			{
				return root;
			}
			int num = 1;
			IntVec3 result = default(IntVec3);
			float num2 = -1000f;
			bool flag = false;
			int num3 = GenRadial.NumCellsInRadius(30f);
			while (true)
			{
				IntVec3 intVec = root + GenRadial.RadialPattern[num];
				if (predicate(intVec))
				{
					float num4 = CoverUtility.TotalSurroundingCoverScore(intVec, map);
					if (num4 > num2)
					{
						num2 = num4;
						result = intVec;
						flag = true;
					}
				}
				if (num >= 8 && flag)
				{
					break;
				}
				num++;
				if (num >= num3)
				{
					return searcher.Position;
				}
			}
			return result;
		}

		public static bool TryFindBestExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAllDestroyableThings && !pawn.Map.reachability.CanReachMapEdge(pawn.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBash: true)))
			{
				return TryFindRandomPawnEntryCell(out spot, pawn.Map, 0f, allowFogged: true, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly, canBash: false, mode));
			}
			int num = 0;
			int num2 = 0;
			IntVec3 intVec;
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					spot = pawn.Position;
					return false;
				}
				IntVec3 result;
				bool num3 = CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, num, null, out result);
				num += 4;
				if (num3)
				{
					int num4 = result.x;
					intVec = new IntVec3(0, 0, result.z);
					if (pawn.Map.Size.z - result.z < num4)
					{
						num4 = pawn.Map.Size.z - result.z;
						intVec = new IntVec3(result.x, 0, pawn.Map.Size.z - 1);
					}
					if (pawn.Map.Size.x - result.x < num4)
					{
						num4 = pawn.Map.Size.x - result.x;
						intVec = new IntVec3(pawn.Map.Size.x - 1, 0, result.z);
					}
					if (result.z < num4)
					{
						intVec = new IntVec3(result.x, 0, 0);
					}
					if (intVec.Standable(pawn.Map) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, canBash: true, mode))
					{
						break;
					}
				}
			}
			spot = intVec;
			return true;
		}

		public static bool TryFindRandomExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			Danger maxDanger = Danger.Some;
			int num = 0;
			IntVec3 intVec;
			do
			{
				num++;
				if (num > 40)
				{
					spot = pawn.Position;
					return false;
				}
				if (num > 15)
				{
					maxDanger = Danger.Deadly;
				}
				intVec = CellFinder.RandomCell(pawn.Map);
				int num2 = Rand.RangeInclusive(0, 3);
				if (num2 == 0)
				{
					intVec.x = 0;
				}
				if (num2 == 1)
				{
					intVec.x = pawn.Map.Size.x - 1;
				}
				if (num2 == 2)
				{
					intVec.z = 0;
				}
				if (num2 == 3)
				{
					intVec.z = pawn.Map.Size.z - 1;
				}
			}
			while (!intVec.Standable(pawn.Map) || !pawn.CanReach(intVec, PathEndMode.OnCell, maxDanger, canBash: false, mode));
			spot = intVec;
			return true;
		}

		public static bool TryFindExitSpotNear(Pawn pawn, IntVec3 near, float radius, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
		{
			if (mode == TraverseMode.PassAllDestroyableThings && CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly), out spot))
			{
				return true;
			}
			return CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly, canBash: false, mode), out spot);
		}

		public static IntVec3 RandomWanderDestFor(Pawn pawn, IntVec3 root, float radius, Func<Pawn, IntVec3, IntVec3, bool> validator, Danger maxDanger)
		{
			if (radius > 12f)
			{
				Log.Warning("wanderRadius of " + radius + " is greater than Region.GridSize of " + 12 + " and will break.");
			}
			bool flag = false;
			if (root.GetRegion(pawn.Map) != null)
			{
				int maxRegions = Mathf.Max((int)radius / 3, 13);
				CellFinder.AllRegionsNear(regions, root.GetRegion(pawn.Map), maxRegions, TraverseParms.For(pawn), (Region reg) => reg.extentsClose.ClosestDistSquaredTo(root) <= radius * radius);
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(root, 0.6f, "root");
				}
				if (regions.Count > 0)
				{
					for (int i = 0; i < 35; i++)
					{
						IntVec3 intVec = IntVec3.Invalid;
						for (int j = 0; j < 5; j++)
						{
							IntVec3 randomCell = regions.RandomElementByWeight((Region reg) => reg.CellCount).RandomCell;
							if ((float)randomCell.DistanceToSquared(root) <= radius * radius)
							{
								intVec = randomCell;
								break;
							}
						}
						if (!intVec.IsValid)
						{
							if (flag)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.32f, "distance");
							}
							continue;
						}
						if (!CanWanderToCell(intVec, pawn, root, validator, i, maxDanger))
						{
							if (flag)
							{
								pawn.Map.debugDrawer.FlashCell(intVec, 0.6f, "validation");
							}
							continue;
						}
						if (flag)
						{
							pawn.Map.debugDrawer.FlashCell(intVec, 0.9f, "go!");
						}
						regions.Clear();
						return intVec;
					}
				}
				regions.Clear();
			}
			if (!CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn) && (validator == null || validator(pawn, c, root)), out IntVec3 result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 20, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 30, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 5, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result))
			{
				result = pawn.Position;
			}
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(result, 0.4f, "fallback");
			}
			return result;
		}

		private static bool CanWanderToCell(IntVec3 c, Pawn pawn, IntVec3 root, Func<Pawn, IntVec3, IntVec3, bool> validator, int tryIndex, Danger maxDanger)
		{
			bool flag = false;
			if (!c.Walkable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0f, "walk");
				}
				return false;
			}
			if (c.IsForbidden(pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "forbid");
				}
				return false;
			}
			if (tryIndex < 10 && !c.Standable(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.25f, "stand");
				}
				return false;
			}
			if (!pawn.CanReach(c, PathEndMode.OnCell, maxDanger))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.6f, "reach");
				}
				return false;
			}
			if (PawnUtility.KnownDangerAt(c, pawn.Map, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.1f, "trap");
				}
				return false;
			}
			if (tryIndex < 10)
			{
				if (c.GetTerrain(pawn.Map).avoidWander)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.39f, "terr");
					}
					return false;
				}
				if (pawn.Map.pathGrid.PerceivedPathCostAt(c) > 20)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.4f, "pcost");
					}
					return false;
				}
				if ((int)c.GetDangerFor(pawn, pawn.Map) > 1)
				{
					if (flag)
					{
						pawn.Map.debugDrawer.FlashCell(c, 0.4f, "danger");
					}
					return false;
				}
			}
			else if (tryIndex < 15 && c.GetDangerFor(pawn, pawn.Map) == Danger.Deadly)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.4f, "deadly");
				}
				return false;
			}
			if (!pawn.Map.pawnDestinationReservationManager.CanReserve(c, pawn))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.75f, "resvd");
				}
				return false;
			}
			if (validator != null && !validator(pawn, c, root))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.15f, "valid");
				}
				return false;
			}
			if (c.GetDoor(pawn.Map) != null)
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.32f, "door");
				}
				return false;
			}
			if (c.ContainsStaticFire(pawn.Map))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.9f, "fire");
				}
				return false;
			}
			return true;
		}

		public static bool TryFindGoodAdjacentSpotToTouch(Pawn toucher, Thing touchee, out IntVec3 result)
		{
			foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder())
			{
				if (item.Standable(toucher.Map) && !PawnUtility.KnownDangerAt(item, toucher.Map, toucher))
				{
					result = item;
					return true;
				}
			}
			foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder())
			{
				if (item2.Walkable(toucher.Map))
				{
					result = item2;
					return true;
				}
			}
			result = touchee.Position;
			return false;
		}

		public static bool TryFindRandomPawnEntryCell(out IntVec3 result, Map map, float roadChance, bool allowFogged = false, Predicate<IntVec3> extraValidator = null)
		{
			return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.Standable(map) && !map.roofGrid.Roofed(c) && map.reachability.CanReachColony(c) && c.GetRoom(map).TouchesMapEdge && (allowFogged || !c.Fogged(map)) && (extraValidator == null || extraValidator(c)), map, roadChance, out result);
		}

		public static bool TryFindPrisonerReleaseCell(Pawn prisoner, Pawn warden, out IntVec3 result)
		{
			if (prisoner.Map != warden.Map)
			{
				result = IntVec3.Invalid;
				return false;
			}
			Region region = prisoner.GetRegion();
			if (region == null)
			{
				result = default(IntVec3);
				return false;
			}
			TraverseParms traverseParms = TraverseParms.For(warden);
			bool needMapEdge = prisoner.Faction != warden.Faction;
			IntVec3 foundResult = IntVec3.Invalid;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (needMapEdge)
				{
					if (!r.Room.TouchesMapEdge)
					{
						return false;
					}
				}
				else if (r.Room.isPrisonCell)
				{
					return false;
				}
				foundResult = r.RandomCell;
				return true;
			};
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), regionProcessor, 999);
			if (foundResult.IsValid)
			{
				result = foundResult;
				return true;
			}
			result = default(IntVec3);
			return false;
		}

		public static IntVec3 RandomAnimalSpawnCell_MapGen(Map map)
		{
			int numStand = 0;
			int numRoom = 0;
			int numTouch = 0;
			if (!CellFinderLoose.TryGetRandomCellWith(delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					numStand++;
					return false;
				}
				if (c.GetTerrain(map).avoidWander)
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (room == null)
				{
					numRoom++;
					return false;
				}
				if (!room.TouchesMapEdge)
				{
					numTouch++;
					return false;
				}
				return true;
			}, map, 1000, out IntVec3 result))
			{
				result = CellFinder.RandomCell(map);
				Log.Warning(string.Concat("RandomAnimalSpawnCell_MapGen failed: numStand=", numStand, ", numRoom=", numRoom, ", numTouch=", numTouch, ". PlayerStartSpot=", MapGenerator.PlayerStartSpot, ". Returning ", result));
			}
			return result;
		}

		public static bool TryFindSkygazeCell(IntVec3 root, Pawn searcher, out IntVec3 result)
		{
			Predicate<IntVec3> cellValidator = (IntVec3 c) => !c.Roofed(searcher.Map) && !c.GetTerrain(searcher.Map).avoidWander;
			IntVec3 result3;
			Predicate<Region> validator = (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(searcher) && r.TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out result3);
			TraverseParms traverseParms = TraverseParms.For(searcher);
			if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(searcher.Map), traverseParms, validator, 300, out Region result2))
			{
				result = root;
				return false;
			}
			return CellFinder.RandomRegionNear(result2, 14, traverseParms, validator, searcher).TryFindRandomCellInRegionUnforbidden(searcher, cellValidator, out result);
		}

		public static bool TryFindTravelDestFrom(IntVec3 root, Map map, out IntVec3 travelDest)
		{
			travelDest = root;
			bool flag = false;
			Predicate<IntVec3> cellValidator = (IntVec3 c) => map.reachability.CanReach(root, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.None) && !map.roofGrid.Roofed(c);
			if (root.x == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == map.Size.x - 1 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			else if (root.x == map.Size.x - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.x == 0 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			else if (root.z == 0)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == map.Size.z - 1 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			else if (root.z == map.Size.z - 1)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => c.z == 0 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 10000 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			if (!flag)
			{
				flag = CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => (c - root).LengthHorizontalSquared > 2500 && cellValidator(c), map, CellFinder.EdgeRoadChance_Always, out travelDest);
			}
			return flag;
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 originCell, Map map, out IntVec3 result)
		{
			return TryFindRandomSpotJustOutsideColony(originCell, map, null, out result);
		}

		public static bool TryFindRandomSpotJustOutsideColony(Pawn searcher, out IntVec3 result)
		{
			return TryFindRandomSpotJustOutsideColony(searcher.Position, searcher.Map, searcher, out result);
		}

		public static bool TryFindRandomSpotJustOutsideColony(IntVec3 root, Map map, Pawn searcher, out IntVec3 result, Predicate<IntVec3> extraValidator = null)
		{
			bool desperate = false;
			int minColonyBuildingsLOS = 0;
			int walkRadius = 0;
			int walkRadiusMaxImpassable = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = c.GetRoom(map);
				if (!room.PsychologicallyOutdoors || !room.TouchesMapEdge)
				{
					return false;
				}
				if (room == null || room.CellCount < 60)
				{
					return false;
				}
				if (root.IsValid)
				{
					TraverseParms traverseParams = (searcher != null) ? TraverseParms.For(searcher) : ((TraverseParms)TraverseMode.PassDoors);
					if (!map.reachability.CanReach(root, c, PathEndMode.Touch, traverseParams))
					{
						return false;
					}
				}
				if (!desperate && !map.reachability.CanReachColony(c))
				{
					return false;
				}
				if (extraValidator != null && !extraValidator(c))
				{
					return false;
				}
				int num = 0;
				foreach (IntVec3 item in CellRect.CenteredOn(c, walkRadius))
				{
					Room room2 = item.GetRoom(map);
					if (room2 != room)
					{
						num++;
						if (!desperate && room2 != null && room2.IsDoorway)
						{
							return false;
						}
					}
					if (num > walkRadiusMaxImpassable)
					{
						return false;
					}
				}
				if (minColonyBuildingsLOS > 0)
				{
					int colonyBuildingsLOSFound = 0;
					tmpBuildings.Clear();
					RegionTraverser.BreadthFirstTraverse(c, map, (Region from, Region to) => true, delegate(Region reg)
					{
						Faction ofPlayer = Faction.OfPlayer;
						List<Thing> list = reg.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
						for (int l = 0; l < list.Count; l++)
						{
							Thing thing = list[l];
							if (thing.Faction == ofPlayer && thing.Position.InHorDistOf(c, 16f) && GenSight.LineOfSight(thing.Position, c, map, skipFirstCell: true) && !tmpBuildings.Contains(thing))
							{
								tmpBuildings.Add(thing);
								colonyBuildingsLOSFound++;
								if (colonyBuildingsLOSFound >= minColonyBuildingsLOS)
								{
									return true;
								}
							}
						}
						return false;
					}, 12);
					tmpBuildings.Clear();
					if (colonyBuildingsLOSFound < minColonyBuildingsLOS)
					{
						return false;
					}
				}
				return true;
			};
			IEnumerable<Building> source = map.listerBuildings.allBuildingsColonist.Where((Building b) => b.def.building.ai_chillDestination);
			for (int i = 0; i < 120; i++)
			{
				Building result2 = null;
				if (!source.TryRandomElement(out result2))
				{
					break;
				}
				desperate = (i > 60);
				walkRadius = 6 - i / 20;
				walkRadiusMaxImpassable = 6 - i / 20;
				minColonyBuildingsLOS = 5 - i / 30;
				if (CellFinder.TryFindRandomCellNear(result2.Position, map, 10, validator, out result, 50))
				{
					return true;
				}
			}
			List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
			for (int j = 0; j < 120; j++)
			{
				Building result3 = null;
				if (!allBuildingsColonist.TryRandomElement(out result3))
				{
					break;
				}
				desperate = (j > 60);
				walkRadius = 6 - j / 20;
				walkRadiusMaxImpassable = 6 - j / 20;
				minColonyBuildingsLOS = 4 - j / 30;
				if (CellFinder.TryFindRandomCellNear(result3.Position, map, 15, validator, out result, 50))
				{
					return true;
				}
			}
			for (int k = 0; k < 50; k++)
			{
				Pawn result4 = null;
				if (!map.mapPawns.FreeColonistsAndPrisonersSpawned.TryRandomElement(out result4))
				{
					break;
				}
				desperate = (k > 25);
				walkRadius = 3;
				walkRadiusMaxImpassable = 6;
				minColonyBuildingsLOS = 0;
				if (CellFinder.TryFindRandomCellNear(result4.Position, map, 15, validator, out result, 50))
				{
					return true;
				}
			}
			desperate = true;
			walkRadius = 3;
			walkRadiusMaxImpassable = 6;
			minColonyBuildingsLOS = 0;
			if (CellFinderLoose.TryGetRandomCellWith(validator, map, 1000, out result))
			{
				return true;
			}
			return false;
		}

		public static bool TryFindRandomCellInRegionUnforbidden(this Region reg, Pawn pawn, Predicate<IntVec3> validator, out IntVec3 result)
		{
			if (reg == null)
			{
				throw new ArgumentNullException("reg");
			}
			if (reg.IsForbiddenEntirely(pawn))
			{
				result = IntVec3.Invalid;
				return false;
			}
			return reg.TryFindRandomCellInRegion((IntVec3 c) => !c.IsForbidden(pawn) && (validator == null || validator(c)), out result);
		}

		public static bool TryFindDirectFleeDestination(IntVec3 root, float dist, Pawn pawn, out IntVec3 result)
		{
			for (int i = 0; i < 30; i++)
			{
				result = root + IntVec3.FromVector3(Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * dist);
				if (result.Walkable(pawn.Map) && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, pawn.Map, skipFirstCell: true))
				{
					return true;
				}
			}
			Region region = pawn.GetRegion();
			for (int j = 0; j < 30; j++)
			{
				IntVec3 randomCell = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn)).RandomCell;
				if (!randomCell.Walkable(pawn.Map) || !((float)(root - randomCell).LengthHorizontalSquared > dist * dist))
				{
					continue;
				}
				using (PawnPath path = pawn.Map.pathFinder.FindPath(pawn.Position, randomCell, pawn))
				{
					if (PawnPathUtility.TryFindCellAtIndex(path, (int)dist + 3, out result))
					{
						return true;
					}
				}
			}
			result = pawn.Position;
			return false;
		}

		public static bool TryFindRandomCellOutsideColonyNearTheCenterOfTheMap(IntVec3 pos, Map map, float minDistToColony, out IntVec3 result)
		{
			int num = 30;
			CellRect cellRect = CellRect.CenteredOn(map.Center, num);
			cellRect.ClipInsideMap(map);
			List<IntVec3> list = new List<IntVec3>();
			if (minDistToColony > 0f)
			{
				foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
				{
					list.Add(item.Position);
				}
				foreach (Building item2 in map.listerBuildings.allBuildingsColonist)
				{
					list.Add(item2.Position);
				}
			}
			float num2 = minDistToColony * minDistToColony;
			int num3 = 0;
			while (true)
			{
				num3++;
				if (num3 > 50)
				{
					if (num > map.Size.x)
					{
						break;
					}
					num = (int)((float)num * 1.5f);
					cellRect = CellRect.CenteredOn(map.Center, num);
					cellRect.ClipInsideMap(map);
					num3 = 0;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (!randomCell.Standable(map) || !map.reachability.CanReach(randomCell, pos, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Deadly))
				{
					continue;
				}
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if ((float)(list[i] - randomCell).LengthHorizontalSquared < num2)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = randomCell;
					return true;
				}
			}
			result = pos;
			return false;
		}

		public static bool TryFindRandomCellNearTheCenterOfTheMapWith(Predicate<IntVec3> validator, Map map, out IntVec3 result)
		{
			int startingSearchRadius = Mathf.Clamp(Mathf.Max(map.Size.x, map.Size.z) / 20, 3, 25);
			return TryFindRandomCellNearWith(map.Center, validator, map, out result, startingSearchRadius);
		}

		public static bool TryFindRandomCellNearWith(IntVec3 near, Predicate<IntVec3> validator, Map map, out IntVec3 result, int startingSearchRadius = 5, int maxSearchRadius = int.MaxValue)
		{
			int num = startingSearchRadius;
			CellRect cellRect = CellRect.CenteredOn(near, num);
			cellRect.ClipInsideMap(map);
			int num2 = 0;
			while (true)
			{
				num2++;
				if (num2 > 30)
				{
					if (num >= maxSearchRadius || (num > map.Size.x * 2 && num > map.Size.z * 2))
					{
						break;
					}
					num = Mathf.Min((int)((float)num * 1.5f), maxSearchRadius);
					cellRect = CellRect.CenteredOn(near, num);
					cellRect.ClipInsideMap(map);
					num2 = 0;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (validator(randomCell))
				{
					result = randomCell;
					return true;
				}
			}
			result = near;
			return false;
		}

		public static IntVec3 SpotToChewStandingNear(Pawn pawn, Thing ingestible)
		{
			IntVec3 root = pawn.Position;
			Room rootRoom = pawn.GetRoom();
			bool desperate = false;
			bool ignoreDanger = false;
			float maxDist = 4f;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				IntVec3 placeSpot = root - c;
				if ((float)placeSpot.LengthHorizontalSquared > maxDist * maxDist)
				{
					return false;
				}
				if (pawn.HostFaction != null && c.GetRoom(pawn.Map) != rootRoom)
				{
					return false;
				}
				if (!desperate)
				{
					if (!c.Standable(pawn.Map))
					{
						return false;
					}
					if (GenPlace.HaulPlaceBlockerIn(null, c, pawn.Map, checkBlueprintsAndFrames: false) != null)
					{
						return false;
					}
					if (c.GetRegion(pawn.Map).type == RegionType.Portal)
					{
						return false;
					}
				}
				if (!ignoreDanger && c.GetDangerFor(pawn, pawn.Map) != Danger.None)
				{
					return false;
				}
				if (c.ContainsStaticFire(pawn.Map) || c.ContainsTrap(pawn.Map))
				{
					return false;
				}
				if (!pawn.Map.pawnDestinationReservationManager.CanReserve(c, pawn))
				{
					return false;
				}
				return Toils_Ingest.TryFindAdjacentIngestionPlaceSpot(c, ingestible.def, pawn, out placeSpot) ? true : false;
			};
			int maxRegions = 1;
			Region region = pawn.GetRegion();
			for (int i = 0; i < 30; i++)
			{
				switch (i)
				{
				case 1:
					desperate = true;
					break;
				case 2:
					desperate = false;
					maxRegions = 4;
					break;
				case 6:
					desperate = true;
					break;
				case 10:
					desperate = false;
					maxDist = 8f;
					maxRegions = 12;
					break;
				case 15:
					desperate = true;
					break;
				case 20:
					maxDist = 15f;
					maxRegions = 16;
					break;
				case 26:
					maxDist = 5f;
					maxRegions = 4;
					ignoreDanger = true;
					break;
				case 29:
					maxDist = 15f;
					maxRegions = 16;
					break;
				}
				if (CellFinder.RandomRegionNear(region, maxRegions, TraverseParms.For(pawn)).TryFindRandomCellInRegionUnforbidden(pawn, validator, out IntVec3 result))
				{
					if (DebugViewSettings.drawDestSearch)
					{
						pawn.Map.debugDrawer.FlashCell(result, 0.5f, "go!");
					}
					return result;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					pawn.Map.debugDrawer.FlashCell(result, 0f, i.ToString());
				}
			}
			return region.RandomCell;
		}

		public static bool TryFindMarriageSite(Pawn firstFiance, Pawn secondFiance, out IntVec3 result)
		{
			if (!firstFiance.CanReach(secondFiance, PathEndMode.ClosestTouch, Danger.Deadly))
			{
				result = IntVec3.Invalid;
				return false;
			}
			Map map = firstFiance.Map;
			if ((from x in map.listerBuildings.AllBuildingsColonistOfDef(ThingDefOf.MarriageSpot)
				where MarriageSpotUtility.IsValidMarriageSpotFor(x.Position, firstFiance, secondFiance)
				select x.Position).TryRandomElement(out result))
			{
				return true;
			}
			Predicate<IntVec3> noMarriageSpotValidator = delegate(IntVec3 cell)
			{
				IntVec3 c = cell + LordToil_MarriageCeremony.OtherFianceNoMarriageSpotCellOffset;
				if (!c.InBounds(map))
				{
					return false;
				}
				if (c.IsForbidden(firstFiance) || c.IsForbidden(secondFiance))
				{
					return false;
				}
				if (!c.Standable(map))
				{
					return false;
				}
				Room room = cell.GetRoom(map);
				return (room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10) ? true : false;
			};
			foreach (CompGatherSpot item in map.gatherSpotLister.activeSpots.InRandomOrder())
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(item.parent.Position, item.parent.Map, 4);
					if (MarriageSpotUtility.IsValidMarriageSpotFor(intVec, firstFiance, secondFiance) && noMarriageSpotValidator(intVec))
					{
						result = intVec;
						return true;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(firstFiance.Position, firstFiance.Map, 25, (IntVec3 cell) => MarriageSpotUtility.IsValidMarriageSpotFor(cell, firstFiance, secondFiance) && noMarriageSpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		public static bool TryFindGatheringSpot(Pawn organizer, GatheringDef gatheringDef, out IntVec3 result)
		{
			bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
			Map map = organizer.Map;
			Predicate<IntVec3> baseValidator = (IntVec3 cell) => GatheringsUtility.ValidateGatheringSpot(cell, gatheringDef, organizer, enjoyableOutside);
			List<ThingDef> gatherSpotDefs = gatheringDef.gatherSpotDefs;
			try
			{
				foreach (ThingDef item in gatherSpotDefs)
				{
					foreach (Building item2 in map.listerBuildings.AllBuildingsColonistOfDef(item))
					{
						tmpSpotThings.Add(item2);
					}
				}
				if ((from x in tmpSpotThings
					where baseValidator(x.Position)
					select x.Position).TryRandomElement(out result))
				{
					return true;
				}
			}
			finally
			{
				tmpSpotThings.Clear();
			}
			Predicate<IntVec3> noPartySpotValidator = delegate(IntVec3 cell)
			{
				Room room = cell.GetRoom(map);
				return (room == null || room.IsHuge || room.PsychologicallyOutdoors || room.CellCount >= 10) ? true : false;
			};
			foreach (CompGatherSpot item3 in map.gatherSpotLister.activeSpots.InRandomOrder())
			{
				for (int i = 0; i < 10; i++)
				{
					IntVec3 intVec = CellFinder.RandomClosewalkCellNear(item3.parent.Position, item3.parent.Map, 4);
					if (baseValidator(intVec) && noPartySpotValidator(intVec))
					{
						result = intVec;
						return true;
					}
				}
			}
			if (CellFinder.TryFindRandomCellNear(organizer.Position, organizer.Map, 25, (IntVec3 cell) => baseValidator(cell) && noPartySpotValidator(cell), out result))
			{
				return true;
			}
			result = IntVec3.Invalid;
			return false;
		}

		[Obsolete]
		public static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot, Map map)
		{
			return FindSiegePositionFrom_NewTemp(entrySpot, map);
		}

		public static IntVec3 FindSiegePositionFrom_NewTemp(IntVec3 entrySpot, Map map, bool allowRoofed = false)
		{
			if (!entrySpot.IsValid)
			{
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 result))
				{
					result = CellFinder.RandomCell(map);
				}
				Log.Error("Tried to find a siege position from an invalid cell. Using " + result);
				return result;
			}
			IntVec3 result2;
			for (int num = 70; num >= 20; num -= 10)
			{
				if (TryFindSiegePosition(entrySpot, num, map, allowRoofed, out result2))
				{
					return result2;
				}
			}
			if (TryFindSiegePosition(entrySpot, 100f, map, allowRoofed, out result2))
			{
				return result2;
			}
			Log.Error(string.Concat("Could not find siege spot from ", entrySpot, ", using ", entrySpot));
			return entrySpot;
		}

		private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed, out IntVec3 result)
		{
			CellRect cellRect = CellRect.CenteredOn(entrySpot, 60);
			cellRect.ClipInsideMap(map);
			cellRect = cellRect.ContractedBy(14);
			List<IntVec3> list = new List<IntVec3>();
			foreach (Pawn item in map.mapPawns.FreeColonistsSpawned)
			{
				list.Add(item.Position);
			}
			foreach (Building allBuildingsColonistCombatTarget in map.listerBuildings.allBuildingsColonistCombatTargets)
			{
				list.Add(allBuildingsColonistCombatTarget.Position);
			}
			float num = minDistToColony * minDistToColony;
			int num2 = 0;
			while (true)
			{
				num2++;
				if (num2 > 200)
				{
					break;
				}
				IntVec3 randomCell = cellRect.RandomCell;
				if (!randomCell.Standable(map) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light) || !map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some) || !map.reachability.CanReachColony(randomCell))
				{
					continue;
				}
				bool flag = false;
				for (int i = 0; i < list.Count; i++)
				{
					if ((float)(list[i] - randomCell).LengthHorizontalSquared < num)
					{
						flag = true;
						break;
					}
				}
				if (flag || (!allowRoofed && randomCell.Roofed(map)))
				{
					continue;
				}
				int num3 = 0;
				foreach (IntVec3 item2 in CellRect.CenteredOn(randomCell, 10).ClipInsideMap(map))
				{
					_ = item2;
					if (randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
					{
						num3++;
					}
				}
				if (num3 >= 35)
				{
					result = randomCell;
					return true;
				}
			}
			result = IntVec3.Invalid;
			return false;
		}
	}
}
