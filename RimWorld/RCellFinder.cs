using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class RCellFinder
{
	private static readonly List<Region> regions = new List<Region>();

	private static HashSet<Thing> tmpBuildings = new HashSet<Thing>();

	private static List<Thing> tmpSpotThings = new List<Thing>();

	private static List<IntVec3> tmpSpotsToAvoid = new List<IntVec3>();

	private static List<IntVec3> tmpEdgeCells = new List<IntVec3>();

	private static List<Thing> tmpNearbyColonyThings = new List<Thing>();

	private static bool IsGoodDestination(IntVec3 c, Map map, bool careAboutDanger)
	{
		if (!c.Standable(map))
		{
			return false;
		}
		if (careAboutDanger && c.GetTerrain(map).dangerous)
		{
			return false;
		}
		return true;
	}

	private static bool IsGoodDestinationFor(IntVec3 c, Pawn pawn, bool careAboutDanger)
	{
		Map map = pawn.Map;
		if (!IsGoodDestination(c, map, careAboutDanger))
		{
			return false;
		}
		if (!c.WalkableBy(map, pawn))
		{
			return false;
		}
		if (!c.Standable(map))
		{
			Building_Door door = c.GetDoor(map);
			if (door == null || !door.CanPhysicallyPass(pawn))
			{
				return false;
			}
		}
		if (c.IsForbidden(pawn))
		{
			return false;
		}
		if (careAboutDanger && c.GetDangerFor(pawn, map) == Danger.Deadly)
		{
			return false;
		}
		if (careAboutDanger && PawnUtility.KnownDangerAt(c, pawn.Map, pawn))
		{
			return false;
		}
		if (careAboutDanger && c.VacuumConcernTo(pawn))
		{
			return false;
		}
		return true;
	}

	public static IntVec3 BestOrderedGotoDestNear(IntVec3 root, Pawn searcher, Predicate<IntVec3> cellValidator = null, bool reachable = true)
	{
		Map map = searcher.Map;
		if (IsGoodDest(root))
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
			if (IsGoodDest(intVec))
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
		bool IsGoodDest(IntVec3 c)
		{
			if (!IsGoodDestinationFor(c, searcher, careAboutDanger: false))
			{
				return false;
			}
			if (cellValidator != null && !cellValidator(c))
			{
				return false;
			}
			if (!map.pawnDestinationReservationManager.CanReserve(c, searcher, draftedOnly: true))
			{
				return false;
			}
			if (reachable && !searcher.CanReach(c, PathEndMode.OnCell, Danger.Deadly))
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i] is Pawn pawn && pawn != searcher && pawn.RaceProps.Humanlike && ((searcher.Faction == Faction.OfPlayer && pawn.Faction == searcher.Faction) || (searcher.Faction != Faction.OfPlayer && pawn.Faction != Faction.OfPlayer)))
				{
					return false;
				}
			}
			return true;
		}
	}

	public static bool TryFindBestExitSpot(Pawn pawn, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn, bool canBash = true)
	{
		if ((mode == TraverseMode.PassAllDestroyableThings || mode == TraverseMode.PassAllDestroyableThingsNotWater || mode == TraverseMode.PassAllDestroyablePlayerOwnedThings) && !pawn.Map.reachability.CanReachMapEdge(pawn.Position, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, canBash)))
		{
			return TryFindRandomPawnEntryCell(out spot, pawn.Map, 0f, allowFogged: true, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, mode));
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
				if (IsGoodDestinationFor(intVec, pawn, careAboutDanger: true) && pawn.CanReach(intVec, PathEndMode.OnCell, Danger.Deadly, canBash, canBashFences: false, mode))
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
		while (!IsGoodDestinationFor(intVec, pawn, careAboutDanger: true) || !pawn.CanReach(intVec, PathEndMode.OnCell, maxDanger, canBashDoors: false, canBashFences: false, mode));
		spot = intVec;
		return true;
	}

	public static bool TryFindExitSpotNear(Pawn pawn, IntVec3 near, float radius, out IntVec3 spot, TraverseMode mode = TraverseMode.ByPawn)
	{
		if ((mode == TraverseMode.PassAllDestroyableThings || mode == TraverseMode.PassAllDestroyableThingsNotWater || mode == TraverseMode.PassAllDestroyablePlayerOwnedThings) && CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly), out spot))
		{
			return true;
		}
		return CellFinder.TryFindRandomEdgeCellNearWith(near, radius, pawn.Map, (IntVec3 x) => pawn.CanReach(x, PathEndMode.OnCell, Danger.Deadly, canBashDoors: false, canBashFences: false, mode), out spot);
	}

	public static bool TryFindExitPortal(Pawn pawn, out Thing portal)
	{
		portal = null;
		if (!pawn.Map.IsPocketMap)
		{
			return false;
		}
		List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.MapPortal);
		if (list.NullOrEmpty())
		{
			return false;
		}
		portal = list.MinBy((Thing x) => x.Position.DistanceToSquared(pawn.Position));
		return true;
	}

	public static IntVec3 RandomWanderDestFor(Pawn pawn, IntVec3 root, float radius, Func<Pawn, IntVec3, IntVec3, bool> validator, Danger maxDanger, bool canBashDoors = false)
	{
		if (radius > 12f)
		{
			Log.Warning("wanderRadius of " + radius + " is greater than Region.GridSize of " + 12 + " and will break.");
		}
		bool flag = false;
		if (root.GetRegion(pawn.Map) != null)
		{
			int maxRegions = Mathf.Max((int)radius / 3, 13);
			bool careAboutSunlight = pawn.genes != null && !pawn.genes.EnjoysSunlight;
			float num = (pawn.RaceProps.Humanlike ? 1f : 0.5f);
			bool careAboutPollution = ModsConfig.BiotechActive && pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) < num;
			bool careAboutRotStink = pawn.GetStatValue(StatDefOf.ToxicEnvironmentResistance) < num;
			CellFinder.AllRegionsNear(regions, root.GetRegion(pawn.Map), maxRegions, TraverseParms.For(pawn, maxDanger, TraverseMode.ByPawn, canBashDoors), (Region reg) => reg.extentsClose.ClosestDistSquaredTo(root) <= radius * radius);
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(root, 0.6f, "root");
			}
			if (regions.Count > 0)
			{
				for (int num2 = 0; num2 < 35; num2++)
				{
					IntVec3 intVec = IntVec3.Invalid;
					for (int num3 = 0; num3 < 5; num3++)
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
					if (!CanWanderToCell(intVec, pawn, root, validator, num2, maxDanger, careAboutSunlight, careAboutPollution, careAboutRotStink, canBashDoors))
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
		if (!CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn) && (validator == null || validator(pawn, c, root)), out var result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, Mathf.FloorToInt(radius), (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 20, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.None) && !c.IsForbidden(pawn), out result) && !CellFinder.TryFindRandomCellNear(root, pawn.Map, 30, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result) && !CellFinder.TryFindRandomCellNear(pawn.Position, pawn.Map, 5, (IntVec3 c) => c.InBounds(pawn.Map) && pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly), out result))
		{
			result = pawn.Position;
		}
		if (flag)
		{
			pawn.Map.debugDrawer.FlashCell(result, 0.4f, "fallback");
		}
		return result;
	}

	private static bool CanWanderToCell(IntVec3 c, Pawn pawn, IntVec3 root, Func<Pawn, IntVec3, IntVec3, bool> validator = null, int tryIndex = 0, Danger maxDanger = Danger.Some, bool careAboutSunlight = false, bool careAboutPollution = true, bool careAboutRotStink = true, bool canBashDoors = false)
	{
		bool flag = false;
		if (!c.WalkableBy(pawn.Map, pawn))
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
		if (!pawn.CanReach(c, PathEndMode.OnCell, maxDanger, canBashDoors))
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
		if (careAboutSunlight && tryIndex < 20 && c.InSunlight(pawn.Map))
		{
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(c, 0.3f, "sun");
			}
			return false;
		}
		if (careAboutPollution && tryIndex < 20 && c.IsPolluted(pawn.Map))
		{
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(c, 0.32f, "pol");
			}
			return false;
		}
		if (careAboutRotStink && tryIndex < 20 && c.AnyGas(pawn.Map, GasType.RotStink))
		{
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(c, 0.2f, "rot");
			}
			return false;
		}
		if (tryIndex < 10)
		{
			TerrainDef terrain = c.GetTerrain(pawn.Map);
			if (terrain.avoidWander && (!terrain.IsWater || !pawn.RaceProps.waterSeeker))
			{
				if (flag)
				{
					pawn.Map.debugDrawer.FlashCell(c, 0.39f, "terr");
				}
				return false;
			}
			if ((Pawn_PathFollower.GetPawnCellBaseCostOverride(pawn, c) ?? pawn.Map.pathing.For(pawn).pathGrid.Cost(c)) > 20)
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
		if (c.GetTerrain(pawn.Map).dangerous)
		{
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(c, 0.9f, "dangerous terrain");
			}
			return false;
		}
		if (pawn.RaceProps.waterSeeker && pawn.Map.terrainGrid.AnyWaterCells && pawn.Faction != Faction.OfPlayer && !pawn.Map.terrainGrid.WaterAt(c))
		{
			CellRect cellRect = CellRect.CenteredOn(c, 8).ClipInsideMap(pawn.Map);
			for (int i = 0; i < 10; i++)
			{
				if (pawn.Map.terrainGrid.WaterAt(cellRect.RandomCell))
				{
					return true;
				}
			}
			if (flag)
			{
				pawn.Map.debugDrawer.FlashCell(c, 0.67f, "no-h2o");
			}
			return false;
		}
		return true;
	}

	public static bool TryFindGoodAdjacentSpotToTouch(Pawn toucher, Thing touchee, out IntVec3 result)
	{
		IntVec3 intVec = IntVec3.Invalid;
		int num = int.MaxValue;
		foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(touchee))
		{
			if (IsGoodDestinationFor(item, toucher, careAboutDanger: true) && toucher.CanReach(item, PathEndMode.OnCell, Danger.Deadly) && ReachabilityImmediate.CanReachImmediate(item, touchee, toucher.Map, PathEndMode.Touch, toucher))
			{
				if (toucher.Position == item)
				{
					intVec = item;
					break;
				}
				int num2 = toucher.Position.DistanceToSquared(item);
				if (num2 < num || (intVec.GetTerrain(toucher.Map).avoidWander && !item.GetTerrain(toucher.Map).avoidWander) || (intVec.GetFirstThing<Building_Trap>(toucher.Map) != null && item.GetFirstThing<Building_Trap>(toucher.Map) == null))
				{
					num = num2;
					intVec = item;
				}
			}
		}
		if (intVec.IsValid)
		{
			result = intVec;
			return true;
		}
		foreach (IntVec3 item2 in GenAdj.CellsAdjacent8Way(touchee).InRandomOrder())
		{
			if (item2.WalkableBy(toucher.Map, toucher) && toucher.CanReach(item2, PathEndMode.OnCell, Danger.Deadly))
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
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => IsGoodDestination(c, map, careAboutDanger: true) && (map.TileInfo.AllowRoofedEdgeWalkIn || !map.roofGrid.Roofed(c)) && map.reachability.CanReachColony(c) && c.GetDistrict(map).TouchesMapEdge && (allowFogged || !c.Fogged(map)) && (extraValidator == null || extraValidator(c)), map, roadChance, out result);
	}

	public static bool TryFindClosestEdgeCellTo(IntVec3 root, Map map, out IntVec3 result)
	{
		result = IntVec3.Invalid;
		Region region = root.GetRegion(map);
		if (region == null)
		{
			return false;
		}
		TraverseParms traverseParms = TraverseParms.For(TraverseMode.PassDoors);
		Region exitRegion = null;
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), delegate(Region r)
		{
			if (!r.touchesMapEdge)
			{
				return false;
			}
			exitRegion = r;
			return true;
		});
		if (exitRegion == null)
		{
			return false;
		}
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => object.Equals(c.GetRegion(map), exitRegion), map, 0f, out result);
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
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), RegionProcessor, 999);
		if (foundResult.IsValid)
		{
			result = foundResult;
			return true;
		}
		result = default(IntVec3);
		return false;
		bool RegionProcessor(Region r)
		{
			if (needMapEdge)
			{
				if (!r.District.TouchesMapEdge)
				{
					return false;
				}
			}
			else if (r.Room.IsPrisonCell)
			{
				return false;
			}
			foundResult = r.RandomCell;
			return true;
		}
	}

	public static IntVec3 RandomAnimalSpawnCell_MapGen(Map map)
	{
		int numStand = 0;
		int numDistrict = 0;
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
			if (c.GetTerrain(map).dangerous)
			{
				return false;
			}
			District district = c.GetDistrict(map);
			if (district == null)
			{
				numDistrict++;
				return false;
			}
			if (!district.TouchesMapEdge)
			{
				numTouch++;
				return false;
			}
			return true;
		}, map, 1000, out var result))
		{
			result = CellFinder.RandomCell(map);
			string[] obj = new string[10]
			{
				"RandomAnimalSpawnCell_MapGen failed: numStand=",
				numStand.ToString(),
				", numDistrict=",
				numDistrict.ToString(),
				", numTouch=",
				numTouch.ToString(),
				". PlayerStartSpot=",
				MapGenerator.PlayerStartSpot.ToString(),
				". Returning ",
				null
			};
			IntVec3 intVec = result;
			obj[9] = intVec.ToString();
			Log.Warning(string.Concat(obj));
		}
		return result;
	}

	public static bool TryFindSkygazeCell(IntVec3 root, Pawn searcher, out IntVec3 result)
	{
		IntVec3 result3;
		Predicate<Region> validator = (Region r) => r.Room.PsychologicallyOutdoors && !r.IsForbiddenEntirely(searcher) && r.TryFindRandomCellInRegionUnforbidden(searcher, Validator, out result3);
		TraverseParms traverseParms = TraverseParms.For(searcher);
		if (!CellFinder.TryFindClosestRegionWith(root.GetRegion(searcher.Map), traverseParms, validator, 45, out var result2))
		{
			result = root;
			return false;
		}
		return CellFinder.RandomRegionNear(result2, 14, traverseParms, validator, searcher).TryFindRandomCellInRegionUnforbidden(searcher, Validator, out result);
		bool Validator(IntVec3 c)
		{
			if (!c.Roofed(searcher.Map) && !c.Fogged(searcher.Map))
			{
				return IsGoodDestinationFor(c, searcher, careAboutDanger: true);
			}
			return false;
		}
	}

	public static bool TryFindTravelDestFrom(IntVec3 root, Map map, out IntVec3 travelDest)
	{
		travelDest = root;
		bool flag = false;
		Predicate<IntVec3> cellValidator = (IntVec3 c) => IsGoodDestination(c, map, careAboutDanger: true) && map.reachability.CanReach(root, c, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.None) && (map.TileInfo.AllowRoofedEdgeWalkIn || !map.roofGrid.Roofed(c));
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
		IEnumerable<Building> source = map.listerBuildings.allBuildingsColonist.Where((Building b) => b.def.building.ai_chillDestination);
		bool desperate;
		int walkRadius;
		int walkRadiusMaxImpassable;
		int minColonyBuildingsLOS;
		for (int num = 0; num < 120; num++)
		{
			if (!source.TryRandomElement(out var result2))
			{
				break;
			}
			desperate = num > 60;
			walkRadius = 6 - num / 20;
			walkRadiusMaxImpassable = 6 - num / 20;
			minColonyBuildingsLOS = 5 - num / 30;
			if (CellFinder.TryFindRandomCellNear(result2.Position, map, 10, FinalValidator, out result, 50))
			{
				return true;
			}
		}
		List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
		for (int num2 = 0; num2 < 120; num2++)
		{
			if (!allBuildingsColonist.TryRandomElement(out var result3))
			{
				break;
			}
			desperate = num2 > 60;
			walkRadius = 6 - num2 / 20;
			walkRadiusMaxImpassable = 6 - num2 / 20;
			minColonyBuildingsLOS = 4 - num2 / 30;
			if (CellFinder.TryFindRandomCellNear(result3.Position, map, 15, FinalValidator, out result, 50))
			{
				return true;
			}
		}
		for (int num3 = 0; num3 < 50; num3++)
		{
			if (!map.mapPawns.FreeColonistsAndPrisonersSpawned.TryRandomElement(out var result4))
			{
				break;
			}
			desperate = num3 > 25;
			walkRadius = 3;
			walkRadiusMaxImpassable = 6;
			minColonyBuildingsLOS = 0;
			if (CellFinder.TryFindRandomCellNear(result4.Position, map, 15, FinalValidator, out result, 50))
			{
				return true;
			}
		}
		desperate = true;
		walkRadius = 3;
		walkRadiusMaxImpassable = 6;
		minColonyBuildingsLOS = 0;
		if (CellFinderLoose.TryGetRandomCellWith(FinalValidator, map, 1000, out result))
		{
			return true;
		}
		return false;
		bool FinalValidator(IntVec3 c)
		{
			if (!c.Standable(map))
			{
				return false;
			}
			District district = c.GetDistrict(map);
			if (!district.Room.PsychologicallyOutdoors || !district.TouchesMapEdge)
			{
				return false;
			}
			if (district.CellCount < 60)
			{
				return false;
			}
			if (root.IsValid)
			{
				TraverseParms traverseParams = ((searcher != null) ? TraverseParms.For(searcher) : ((TraverseParms)TraverseMode.PassDoors));
				if (!map.reachability.CanReach(root, c, PathEndMode.Touch, traverseParams))
				{
					return false;
				}
			}
			if (!desperate && !map.reachability.CanReachColony(c))
			{
				return false;
			}
			if (searcher == null && !IsGoodDestination(c, map, desperate))
			{
				return false;
			}
			if (searcher != null && !IsGoodDestinationFor(c, searcher, desperate))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(c))
			{
				return false;
			}
			int num4 = 0;
			foreach (IntVec3 item in CellRect.CenteredOn(c, walkRadius))
			{
				District district2 = item.GetDistrict(map);
				if (district2 != district)
				{
					num4++;
					if (!desperate && district2 != null && district2.IsDoorway)
					{
						return false;
					}
				}
				if (num4 > walkRadiusMaxImpassable)
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
					for (int i = 0; i < list.Count; i++)
					{
						Thing thing = list[i];
						if (thing.Faction == ofPlayer && thing.Position.InHorDistOf(c, 16f) && GenSight.LineOfSight(thing.Position, c, map, skipFirstCell: true) && tmpBuildings.Add(thing))
						{
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
		}
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
			if (result.WalkableBy(pawn.Map, pawn) && result.DistanceToSquared(pawn.Position) < result.DistanceToSquared(root) && GenSight.LineOfSight(root, result, pawn.Map, skipFirstCell: true))
			{
				return true;
			}
		}
		Region region = pawn.GetRegion();
		for (int j = 0; j < 30; j++)
		{
			IntVec3 randomCell = CellFinder.RandomRegionNear(region, 15, TraverseParms.For(pawn)).RandomCell;
			if (!randomCell.WalkableBy(pawn.Map, pawn) || !((float)(root - randomCell).LengthHorizontalSquared > dist * dist))
			{
				continue;
			}
			using PawnPath path = pawn.Map.pathFinder.FindPathNow(pawn.Position, randomCell, pawn);
			if (PawnPathUtility.TryFindCellAtIndex(path, (int)dist + 3, out result))
			{
				return true;
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
			if (!IsGoodDestination(randomCell, map, careAboutDanger: true) || !map.reachability.CanReach(randomCell, pos, PathEndMode.ClosestTouch, TraverseMode.NoPassClosedDoors, Danger.Deadly))
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

	public static bool TryFindRandomClearCellsNear(IntVec3 near, int amount, Map map, out List<IntVec3> cells, int startingSearchRadius = 5, int maxSearchRadius = int.MaxValue)
	{
		return TryFindRandomCellsNear(near, amount, map, (IntVec3 pos) => pos.GetFirstBuilding(map) == null && pos.Walkable(map), out cells, startingSearchRadius, maxSearchRadius);
	}

	public static bool TryFindRandomCellsNear(IntVec3 near, int amount, Map map, Predicate<IntVec3> validator, out List<IntVec3> cells, int startingSearchRadius = 5, int maxSearchRadius = int.MaxValue)
	{
		cells = new List<IntVec3>();
		for (int i = 0; i < amount; i++)
		{
			if (TryFindRandomCellNearWith(near, validator, map, out var result, startingSearchRadius, maxSearchRadius))
			{
				cells.Add(result);
			}
		}
		return cells.Count == amount;
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

	public static IntVec3 SpotToChewStandingNear(Pawn pawn, Thing ingestible, Predicate<IntVec3> extraValidator = null)
	{
		return SpotToStandDuringJob(pawn, Validator);
		bool Validator(IntVec3 c)
		{
			if (!Toils_Ingest.TryFindAdjacentIngestionPlaceSpot(c, ingestible.def, pawn, out var _))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(c))
			{
				return false;
			}
			return true;
		}
	}

	public static IntVec3 SpotToStandDuringJobInRegion(Region region, Pawn pawn, float maxDistance, bool desperate = false, bool ignoreDanger = false, Predicate<IntVec3> extraValidator = null)
	{
		if (!region.TryFindRandomCellInRegionUnforbidden(pawn, Validator, out var result))
		{
			return IntVec3.Invalid;
		}
		return result;
		bool Validator(IntVec3 c)
		{
			if ((float)(pawn.Position - c).LengthHorizontalSquared > maxDistance * maxDistance)
			{
				return false;
			}
			if (pawn.HostFaction != null && c.GetRoom(pawn.Map) != pawn.GetRoom())
			{
				return false;
			}
			if (!desperate)
			{
				if (!IsGoodDestinationFor(c, pawn, !ignoreDanger))
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
			if (c.ContainsStaticFire(pawn.Map) || c.ContainsTrap(pawn.Map))
			{
				return false;
			}
			if (!pawn.Map.pawnDestinationReservationManager.CanReserve(c, pawn))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(c))
			{
				return false;
			}
			return true;
		}
	}

	public static IntVec3 SpotToStandDuringJob(Pawn pawn, Predicate<IntVec3> extraValidator = null, Region targetRegion = null)
	{
		bool desperate = false;
		bool ignoreDanger = false;
		float maxDistance = 4f;
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
				maxDistance = 8f;
				maxRegions = 12;
				break;
			case 15:
				desperate = true;
				break;
			case 20:
				maxDistance = 15f;
				maxRegions = 16;
				break;
			case 26:
				maxDistance = 5f;
				maxRegions = 4;
				ignoreDanger = true;
				break;
			case 29:
				maxDistance = 15f;
				maxRegions = 16;
				break;
			}
			IntVec3 intVec = SpotToStandDuringJobInRegion(targetRegion ?? CellFinder.RandomRegionNear(region, maxRegions, TraverseParms.For(pawn), null, pawn), pawn, maxDistance, desperate, ignoreDanger, extraValidator);
			if (intVec.IsValid)
			{
				if (DebugViewSettings.drawDestSearch)
				{
					pawn.Map.debugDrawer.FlashCell(intVec, 0.5f, "go!");
				}
				return intVec;
			}
			if (DebugViewSettings.drawDestSearch)
			{
				pawn.Map.debugDrawer.FlashCell(intVec, 0f, i.ToString());
			}
		}
		IntVec3 randomCell = region.RandomCell;
		if (extraValidator != null && !extraValidator(randomCell))
		{
			return IntVec3.Invalid;
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
			for (int num = 0; num < 10; num++)
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

	public static bool TryFindGatheringSpot(Pawn organizer, GatheringDef gatheringDef, bool ignoreRequiredColonistCount, out IntVec3 result)
	{
		bool enjoyableOutside = JoyUtility.EnjoyableOutsideNow(organizer);
		Map map = organizer.Map;
		Predicate<IntVec3> baseValidator = (IntVec3 cell) => GatheringsUtility.ValidateGatheringSpot(cell, gatheringDef, organizer, enjoyableOutside, ignoreRequiredColonistCount);
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
			for (int num = 0; num < 10; num++)
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

	public static IntVec3 FindSiegePositionFrom(IntVec3 entrySpot, Map map, bool allowRoofed = false, bool errorOnFail = true, Func<IntVec3, bool> validator = null, bool requireBuildableTerrain = true)
	{
		if (!entrySpot.IsValid)
		{
			if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && !x.Fogged(map), map, CellFinder.EdgeRoadChance_Ignore, out var result))
			{
				result = CellFinder.RandomCell(map);
			}
			IntVec3 intVec = result;
			Log.Error("Tried to find a siege position from an invalid cell. Using " + intVec.ToString());
			return result;
		}
		IntVec3 result2;
		for (int num = 70; num >= 20; num -= 10)
		{
			if (TryFindSiegePosition(entrySpot, num, map, allowRoofed, out result2, validator, requireBuildableTerrain))
			{
				return result2;
			}
		}
		if (TryFindSiegePosition(entrySpot, 100f, map, allowRoofed, out result2, validator, requireBuildableTerrain))
		{
			return result2;
		}
		if (TryFindSiegePosition(entrySpot, 10f, map, allowRoofed, out result2, validator, requireBuildableTerrain))
		{
			return result2;
		}
		if (errorOnFail)
		{
			IntVec3 intVec = entrySpot;
			string text = intVec.ToString();
			intVec = entrySpot;
			Log.Error("Could not find siege spot from " + text + ", using " + intVec.ToString());
		}
		return entrySpot;
	}

	private static bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, Map map, bool allowRoofed, out IntVec3 result, Func<IntVec3, bool> validator = null, bool requireBuildableTerrain = true)
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
			if (!IsGoodDestination(randomCell, map, careAboutDanger: true) || (requireBuildableTerrain && (!randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) || !randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))) || !map.reachability.CanReach(randomCell, entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some) || !map.reachability.CanReachColony(randomCell) || (validator != null && !validator(randomCell)))
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
			if (requireBuildableTerrain)
			{
				int num3 = 0;
				foreach (IntVec3 item2 in CellRect.CenteredOn(randomCell, 10).ClipInsideMap(map))
				{
					_ = item2;
					if (randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Heavy) && randomCell.SupportsStructureType(map, TerrainAffordanceDefOf.Light))
					{
						num3++;
					}
				}
				if (num3 < 35)
				{
					continue;
				}
			}
			result = randomCell;
			return true;
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindEdgeCellWithPathToPositionAvoidingColony(IntVec3 target, float minDistToColony, float minDistanceToTarget, Map map, out IntVec3 result)
	{
		bool flag = false;
		tmpSpotsToAvoid.Clear();
		foreach (Building item in map.listerBuildings.allBuildingsColonist)
		{
			tmpSpotsToAvoid.Add(item.Position);
		}
		foreach (Pawn item2 in map.mapPawns.FreeColonistsAndPrisonersSpawned)
		{
			tmpSpotsToAvoid.Add(item2.Position);
		}
		if (flag)
		{
			for (int i = 0; i < tmpSpotsToAvoid.Count; i++)
			{
				map.debugDrawer.FlashCell(tmpSpotsToAvoid[i], 1f);
			}
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
			IntVec3 intVec = CellFinder.RandomEdgeCell(map);
			if ((!map.TileInfo.AllowRoofedEdgeWalkIn && intVec.Roofed(map)) || !intVec.Standable(map) || !map.reachability.CanReach(intVec, target, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some) || target.InHorDistOf(intVec, minDistanceToTarget))
			{
				continue;
			}
			bool flag2 = false;
			foreach (IntVec3 item3 in GenSight.PointsOnLineOfSight(intVec, target))
			{
				for (int j = 0; j < tmpSpotsToAvoid.Count; j++)
				{
					if ((float)(tmpSpotsToAvoid[j] - item3).LengthHorizontalSquared < num)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
			if (flag)
			{
				map.debugDrawer.FlashLine(intVec, target, 50, flag2 ? SimpleColor.Red : SimpleColor.Green);
			}
			if (!flag2)
			{
				result = intVec;
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static void FindBestAngleAvoidingSpots(IntVec3 position, List<IntVec3> spotsToAvoid, out float bestAngle, out float bestPerimeter)
	{
		if (tmpSpotsToAvoid.Count == 0)
		{
			bestAngle = Rand.Range(0, 360);
			bestPerimeter = 360f;
			return;
		}
		if (tmpSpotsToAvoid.Count == 1)
		{
			float angleFlat = (tmpSpotsToAvoid[0] - position).AngleFlat;
			bestAngle = angleFlat + 180f;
			bestPerimeter = 360f;
			return;
		}
		tmpSpotsToAvoid.SortBy((IntVec3 s) => (s - position).AngleFlat);
		tmpSpotsToAvoid.Add(tmpSpotsToAvoid.First());
		float num = 0f;
		float num2 = 0f;
		for (int num3 = 1; num3 < tmpSpotsToAvoid.Count; num3++)
		{
			IntVec3 intVec = tmpSpotsToAvoid[num3 - 1] - position;
			IntVec3 intVec2 = tmpSpotsToAvoid[num3] - position;
			float angleFlat2 = intVec.AngleFlat;
			float angleFlat3 = intVec2.AngleFlat;
			float num4 = Mathf.Abs(angleFlat3 - angleFlat2);
			float num5 = ((angleFlat3 < angleFlat2) ? (360f - num4) : num4);
			if (num5 > num2)
			{
				num2 = num5;
				num = intVec.AngleFlat;
			}
		}
		bestAngle = num + num2 / 2f;
		bestPerimeter = num2;
	}

	public static bool TryFindRandomSpotNearAvoidingHostilePawns(Thing thing, Map map, Func<IntVec3, bool> predicate, out IntVec3 result, float maxSearchDistance = 100f, float minDistance = 10f, float maxDistance = 50f, bool avoidColony = true)
	{
		IntVec3 thingPosition = thing.Position;
		bool drawDebug = false;
		tmpSpotsToAvoid.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			if (allPawnsSpawned[i].HostileTo(thing) && allPawnsSpawned[i].Position.InHorDistOf(thingPosition, maxSearchDistance))
			{
				tmpSpotsToAvoid.Add(allPawnsSpawned[i].Position);
			}
		}
		if (avoidColony)
		{
			foreach (Building item in map.listerBuildings.allBuildingsColonist)
			{
				if (item.Position.InHorDistOf(thingPosition, maxSearchDistance))
				{
					tmpSpotsToAvoid.Add(item.Position);
				}
			}
		}
		if (drawDebug)
		{
			for (int j = 0; j < tmpSpotsToAvoid.Count; j++)
			{
				map.debugDrawer.FlashCell(tmpSpotsToAvoid[j], 0.2f);
			}
		}
		FindBestAngleAvoidingSpots(thingPosition, tmpSpotsToAvoid, out var bestAngle, out var bestPerimeter);
		tmpSpotsToAvoid.Clear();
		Vector3 v = IntVec3.North.ToVector3();
		if (drawDebug)
		{
			Vector3 vect = v.RotatedBy(bestAngle) * maxDistance;
			Vector3 vect2 = v.RotatedBy(bestAngle + bestPerimeter) * maxDistance;
			map.debugDrawer.FlashLine(thingPosition, thingPosition + vect.ToIntVec3(), 50, SimpleColor.Red);
			map.debugDrawer.FlashLine(thingPosition, thingPosition + vect2.ToIntVec3(), 50, SimpleColor.Red);
		}
		Func<Vector3, IntVec3> func = delegate(Vector3 direction)
		{
			IntVec3 intVec3 = thingPosition + (direction * minDistance).ToIntVec3();
			IntVec3 intVec4 = intVec3 + (direction * (maxDistance - minDistance)).ToIntVec3();
			if (drawDebug)
			{
				map.debugDrawer.FlashLine(intVec3, intVec4);
			}
			foreach (IntVec3 item2 in GenSight.PointsOnLineOfSight(intVec3, intVec4).InRandomOrder())
			{
				if (predicate(item2))
				{
					return item2;
				}
			}
			return IntVec3.Invalid;
		};
		float num = bestPerimeter / 4f;
		for (float num2 = 0f; num2 < num; num2 += 5f)
		{
			Vector3 arg = v.RotatedBy(bestAngle + num2);
			IntVec3 intVec = func(arg);
			if (intVec.IsValid)
			{
				result = intVec;
				return true;
			}
			if (!Mathf.Approximately(num2, 0f))
			{
				Vector3 arg2 = v.RotatedBy(bestAngle - num2);
				IntVec3 intVec2 = func(arg2);
				if (intVec2.IsValid)
				{
					result = intVec2;
					return true;
				}
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindEdgeCellFromPositionAvoidingColony(IntVec3 position, Map map, Predicate<IntVec3> predicate, out IntVec3 result)
	{
		bool flag = false;
		tmpSpotsToAvoid.Clear();
		List<Pawn> freeColonistsAndPrisonersSpawned = map.mapPawns.FreeColonistsAndPrisonersSpawned;
		for (int i = 0; i < freeColonistsAndPrisonersSpawned.Count; i++)
		{
			tmpSpotsToAvoid.Add(freeColonistsAndPrisonersSpawned[i].Position);
		}
		foreach (Building item in map.listerBuildings.allBuildingsColonist)
		{
			tmpSpotsToAvoid.Add(item.Position);
		}
		if (flag)
		{
			for (int j = 0; j < tmpSpotsToAvoid.Count; j++)
			{
				map.debugDrawer.FlashCell(tmpSpotsToAvoid[j], 0.2f);
			}
		}
		FindBestAngleAvoidingSpots(position, tmpSpotsToAvoid, out var bestAngle, out var bestPerimeter);
		tmpSpotsToAvoid.Clear();
		Vector3 v = IntVec3.North.ToVector3();
		if (flag)
		{
			Vector3 vect = v.RotatedBy(bestAngle) * map.Size.LengthHorizontal;
			Vector3 vect2 = v.RotatedBy(bestAngle + bestPerimeter) * map.Size.LengthHorizontal;
			map.debugDrawer.FlashLine(position, position + vect.ToIntVec3(), 50, SimpleColor.Red);
			map.debugDrawer.FlashLine(position, position + vect2.ToIntVec3(), 50, SimpleColor.Red);
		}
		CellRect cellRect = CellRect.WholeMap(map);
		Vector3 normalized = v.RotatedBy(bestAngle).normalized;
		Vector3 vect3 = position.ToVector3();
		IntVec3 currentIntPosition = vect3.ToIntVec3();
		while (!cellRect.IsOnEdge(currentIntPosition))
		{
			vect3 += normalized;
			currentIntPosition = vect3.ToIntVec3();
			if (!cellRect.Contains(currentIntPosition))
			{
				Log.Error($"Failed to find map edge cell from position {position}");
				result = IntVec3.Invalid;
				return false;
			}
		}
		tmpEdgeCells.Clear();
		foreach (IntVec3 edgeCell in cellRect.EdgeCells)
		{
			tmpEdgeCells.Add(edgeCell);
		}
		tmpEdgeCells.SortBy((IntVec3 p) => p.DistanceToSquared(currentIntPosition));
		for (int num = 0; num < tmpEdgeCells.Count; num++)
		{
			if (predicate(tmpEdgeCells[num]))
			{
				result = tmpEdgeCells[num];
				tmpEdgeCells.Clear();
				return true;
			}
		}
		tmpEdgeCells.Clear();
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindEdgeCellFromThingAvoidingColony(Thing thing, Map map, Func<IntVec3, IntVec3, bool> predicate, out IntVec3 result)
	{
		result = IntVec3.Invalid;
		if (thing.def.passability == Traversability.Impassable)
		{
			foreach (IntVec3 cell in thing.OccupiedRect().ExpandedBy(1).EdgeCells)
			{
				if (cell.InBounds(map) && TryFindEdgeCellFromPositionAvoidingColony(cell, map, (IntVec3 p) => predicate(cell, p), out result))
				{
					return true;
				}
			}
		}
		else if (TryFindEdgeCellFromPositionAvoidingColony(thing.Position, map, (IntVec3 p) => predicate(thing.Position, p), out result))
		{
			return true;
		}
		return false;
	}

	public static bool TryFindAllowedUnroofedSpotOutsideColony(IntVec3 root, Pawn searcher, out IntVec3 result, int minDistanceFromColonyThing = 35, int maxDistanceFromColonyThing = int.MaxValue, int maxRegions = 45, Predicate<Region> extraRegionValidator = null)
	{
		tmpNearbyColonyThings.Clear();
		IntVec3 foundCell = IntVec3.Invalid;
		TraverseParms traverseParms = TraverseParms.For(searcher);
		RegionTraverser.BreadthFirstTraverse(root, searcher.Map, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), delegate(Region r)
		{
			if (extraRegionValidator != null && !extraRegionValidator(r))
			{
				return false;
			}
			if (r.IsForbiddenEntirely(searcher))
			{
				return false;
			}
			List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Faction == Faction.OfPlayer)
				{
					tmpNearbyColonyThings.Add(list[i]);
				}
			}
			List<Thing> list2 = r.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].Faction == Faction.OfPlayer && !tmpNearbyColonyThings.Contains(list2[j]))
				{
					tmpNearbyColonyThings.Add(list2[j]);
				}
			}
			if (r.TryFindRandomCellInRegion(CellValidator, out var result2))
			{
				foundCell = result2;
				return true;
			}
			return false;
		}, maxRegions);
		tmpNearbyColonyThings.Clear();
		result = foundCell;
		return result.IsValid;
		bool CellValidator(IntVec3 c)
		{
			if (c.Roofed(searcher.Map) || c.GetTerrain(searcher.Map).avoidWander || !IsGoodDestinationFor(c, searcher, careAboutDanger: true))
			{
				return false;
			}
			for (int i = 0; i < tmpNearbyColonyThings.Count; i++)
			{
				float num = tmpNearbyColonyThings[i].Position.DistanceTo(c);
				if (num < (float)minDistanceFromColonyThing || num > (float)maxDistanceFromColonyThing)
				{
					return false;
				}
			}
			return true;
		}
	}

	public static bool TryFindRandomMechSelfShutdownSpot(IntVec3 root, Pawn pawn, Map map, out IntVec3 result, bool allowForbidden = false)
	{
		if (CanSelfShutdown(pawn.Position, pawn, map, allowForbidden))
		{
			result = pawn.Position;
			return true;
		}
		return TryFindRandomCellNearWith(root, (IntVec3 c) => CanSelfShutdown(c, pawn, map), map, out result, 5, 20);
	}

	private static bool CanSelfShutdown(IntVec3 c, Pawn pawn, Map map, bool allowForbidden = false)
	{
		if (!c.InBounds(map))
		{
			return false;
		}
		if (!pawn.CanReserve(c))
		{
			return false;
		}
		if (!pawn.CanReach(c, PathEndMode.OnCell, Danger.Some))
		{
			return false;
		}
		if (!c.Standable(map))
		{
			return false;
		}
		if (c.GetTerrain(map).dangerous)
		{
			return false;
		}
		if (!allowForbidden && c.IsForbidden(pawn))
		{
			return false;
		}
		if (c.GetFirstBuilding(map) != null)
		{
			return false;
		}
		Room room = c.GetRoom(map);
		if (room != null && room.IsPrisonCell)
		{
			return false;
		}
		for (int i = 0; i < GenAdj.CardinalDirections.Length; i++)
		{
			IntVec3 c2 = c + GenAdj.CardinalDirections[i];
			if (!c2.InBounds(map))
			{
				continue;
			}
			List<Thing> thingList = c2.GetThingList(map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (thingList[j].def.hasInteractionCell && thingList[j].InteractionCell == c)
				{
					return false;
				}
			}
		}
		return true;
	}

	public static bool TryFindNearbyMechSelfShutdownSpot(IntVec3 root, Pawn pawn, Map map, out IntVec3 result, bool allowForbidden = false)
	{
		foreach (IntVec3 item in GenRadial.RadialCellsAround(root, GenRadial.MaxRadialPatternRadius - 1f, useCenter: true))
		{
			if (CanSelfShutdown(item, pawn, map, allowForbidden))
			{
				result = item;
				return true;
			}
		}
		return TryFindRandomMechSelfShutdownSpot(root, pawn, map, out result, allowForbidden);
	}

	public static bool TryFindNearbyDarkCellFor(Pawn pawn, out IntVec3 result, int maxRegionsTraversed = 15)
	{
		IntVec3 resultLocal = IntVec3.Invalid;
		RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(TraverseMode.NoPassClosedDoors, isDestination: true), delegate(Region r)
		{
			if (r.type == RegionType.Portal)
			{
				return false;
			}
			int num = Mathf.Min(100, r.CellCount);
			for (int i = 0; i < num; i++)
			{
				IntVec3 randomCell = r.RandomCell;
				if (pawn.Map.glowGrid.GroundGlowAt(randomCell) <= 0f)
				{
					resultLocal = randomCell;
					return true;
				}
			}
			return false;
		}, maxRegionsTraversed);
		result = resultLocal;
		return resultLocal.IsValid;
	}

	public static bool TryFindNearbyEmptyCell(Pawn pawn, out IntVec3 cell, Predicate<IntVec3, Map> validator = null, int maxRegions = 20)
	{
		cell = IntVec3.Invalid;
		IntVec3 local = IntVec3.Invalid;
		RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(TraverseParms.For(pawn), isDestination: true), delegate(Region r)
		{
			if (r.type == RegionType.Portal)
			{
				return false;
			}
			int num = Mathf.Min(100, r.CellCount);
			for (int i = 0; i < num; i++)
			{
				IntVec3 randomCell = r.RandomCell;
				if (IsGoodDestinationFor(randomCell, pawn, careAboutDanger: true) && (validator == null || validator(randomCell, pawn.MapHeld)) && randomCell.GetFirstBuilding(pawn.MapHeld) == null && randomCell.Walkable(pawn.MapHeld))
				{
					local = randomCell;
					return true;
				}
			}
			return false;
		}, maxRegions);
		cell = local;
		return cell.IsValid;
	}
}
