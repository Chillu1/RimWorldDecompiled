using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class CellFinder
{
	public static float EdgeRoadChance_Ignore = 0f;

	public static float EdgeRoadChance_Animal = 0f;

	public static float EdgeRoadChance_Hostile = 0.2f;

	public static float EdgeRoadChance_Neutral = 0.75f;

	public static float EdgeRoadChance_Friendly = 0.75f;

	public static float EdgeRoadChance_Always = 1f;

	private static List<IntVec3> workingCells = new List<IntVec3>();

	private static List<Region> workingRegions = new List<Region>();

	private static List<int> workingListX = new List<int>();

	private static List<int> workingListZ = new List<int>();

	private static List<int> cardinalIndices = new List<int> { 0, 1, 2, 3 };

	private static List<int> adjacentIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };

	private static List<IntVec3> mapEdgeCells;

	private static IntVec3 mapEdgeCellsSize;

	private static List<IntVec3>[] mapSingleEdgeCells = new List<IntVec3>[4];

	private static IntVec3 mapSingleEdgeCellsSize;

	private static Dictionary<IntVec3, float> tmpDistances = new Dictionary<IntVec3, float>();

	private static Dictionary<IntVec3, IntVec3> tmpParents = new Dictionary<IntVec3, IntVec3>();

	private static List<IntVec3> tmpCells = new List<IntVec3>();

	private static List<Thing> tmpUniqueWipedThings = new List<Thing>();

	public static IntVec3 RandomCell(Map map)
	{
		return new IntVec3(Rand.Range(0, map.Size.x), 0, Rand.Range(0, map.Size.z));
	}

	public static bool TryFindRandomCell(Map map, Predicate<IntVec3> validator, out IntVec3 result)
	{
		for (int i = 0; i < 1000; i++)
		{
			result = RandomCell(map);
			if (validator == null || validator(result))
			{
				return true;
			}
		}
		workingCells.Clear();
		for (int j = 0; j < map.Size.x; j++)
		{
			for (int k = 0; k < map.Size.z; k++)
			{
				if (validator(new IntVec3(j, 0, k)))
				{
					workingCells.Add(new IntVec3(j, 0, k));
				}
			}
		}
		if (workingCells.Empty())
		{
			result = RandomCell(map);
			return false;
		}
		result = workingCells.RandomElement();
		return true;
	}

	public static IntVec3 RandomEdgeCell(Map map)
	{
		IntVec3 result = default(IntVec3);
		if (Rand.Value < 0.5f)
		{
			if (Rand.Value < 0.5f)
			{
				result.x = 0;
			}
			else
			{
				result.x = map.Size.x - 1;
			}
			result.z = Rand.Range(0, map.Size.z);
		}
		else
		{
			if (Rand.Value < 0.5f)
			{
				result.z = 0;
			}
			else
			{
				result.z = map.Size.z - 1;
			}
			result.x = Rand.Range(0, map.Size.x);
		}
		return result;
	}

	public static IntVec3 RandomEdgeCell(Rot4 dir, Map map)
	{
		if (dir == Rot4.North)
		{
			return new IntVec3(Rand.Range(0, map.Size.x), 0, map.Size.z - 1);
		}
		if (dir == Rot4.South)
		{
			return new IntVec3(Rand.Range(0, map.Size.x), 0, 0);
		}
		if (dir == Rot4.West)
		{
			return new IntVec3(0, 0, Rand.Range(0, map.Size.z));
		}
		if (dir == Rot4.East)
		{
			return new IntVec3(map.Size.x - 1, 0, Rand.Range(0, map.Size.z));
		}
		return IntVec3.Invalid;
	}

	public static IntVec3 RandomNotEdgeCell(int minEdgeDistance, Map map)
	{
		if (minEdgeDistance > map.Size.x / 2 || minEdgeDistance > map.Size.z / 2)
		{
			return IntVec3.Invalid;
		}
		int newX = Rand.Range(minEdgeDistance, map.Size.x - minEdgeDistance);
		int newZ = Rand.Range(minEdgeDistance, map.Size.z - minEdgeDistance);
		return new IntVec3(newX, 0, newZ);
	}

	public static bool TryFindClosestRegionWith(Region rootReg, TraverseParms traverseParms, Predicate<Region> validator, int maxRegions, out Region result, RegionType traversableRegionTypes = RegionType.Set_Passable)
	{
		if (rootReg == null)
		{
			result = null;
			return false;
		}
		Region localResult = null;
		RegionTraverser.BreadthFirstTraverse(rootReg, (Region from, Region r) => r.Allows(traverseParms, isDestination: true), delegate(Region r)
		{
			if (validator(r))
			{
				localResult = r;
				return true;
			}
			return false;
		}, maxRegions, traversableRegionTypes);
		result = localResult;
		return result != null;
	}

	public static Region RandomRegionNear(Region root, int maxRegions, TraverseParms traverseParms, Predicate<Region> validator = null, Pawn pawnToAllow = null, RegionType traversableRegionTypes = RegionType.Set_Passable)
	{
		if (root == null)
		{
			throw new ArgumentNullException("root");
		}
		if (maxRegions <= 1)
		{
			return root;
		}
		workingRegions.Clear();
		RegionTraverser.BreadthFirstTraverse(root, (Region from, Region r) => (validator == null || validator(r)) && r.Allows(traverseParms, isDestination: true) && (pawnToAllow == null || !r.IsForbiddenEntirely(pawnToAllow)), delegate(Region r)
		{
			workingRegions.Add(r);
			return false;
		}, maxRegions, traversableRegionTypes);
		Region result = workingRegions.RandomElementByWeight((Region r) => r.CellCount);
		workingRegions.Clear();
		return result;
	}

	public static void AllRegionsNear(List<Region> results, Region root, int maxRegions, TraverseParms traverseParms, Predicate<Region> validator = null, Pawn pawnToAllow = null, RegionType traversableRegionTypes = RegionType.Set_Passable)
	{
		if (results == null)
		{
			Log.ErrorOnce("Attempted to call AllRegionsNear with an invalid results list", 60733193);
			return;
		}
		results.Clear();
		if (root == null)
		{
			Log.ErrorOnce("Attempted to call AllRegionsNear with an invalid root", 9107839);
			return;
		}
		RegionTraverser.BreadthFirstTraverse(root, (Region from, Region r) => (validator == null || validator(r)) && r.Allows(traverseParms, isDestination: true) && (pawnToAllow == null || !r.IsForbiddenEntirely(pawnToAllow)), delegate(Region r)
		{
			results.Add(r);
			return false;
		}, maxRegions, traversableRegionTypes);
	}

	public static bool TryFindRandomReachableNearbyCell(IntVec3 root, Map map, float radius, TraverseParms traverseParms, Predicate<IntVec3> cellValidator, Predicate<Region> regionValidator, out IntVec3 result, int maxRegions = 999999)
	{
		if (map == null)
		{
			Log.ErrorOnce("Tried to find reachable cell in a null map", 61037855);
			result = IntVec3.Invalid;
			return false;
		}
		if (!root.Standable(map))
		{
			Thing thing = root.GetThingList(map).FirstOrDefault((Thing x) => x.def.passability != Traversability.Standable);
			if (thing != null)
			{
				foreach (IntVec3 item in GenAdj.CellsAdjacent8Way(thing))
				{
					if (item.Standable(map))
					{
						root = item;
						break;
					}
				}
			}
		}
		Region region = root.GetRegion(map);
		if (region == null)
		{
			result = IntVec3.Invalid;
			return false;
		}
		workingRegions.Clear();
		float radSquared = radius * radius;
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: true) && (radius > 1000f || r.extentsClose.ClosestDistSquaredTo(root) <= radSquared) && (regionValidator == null || regionValidator(r)), delegate(Region r)
		{
			workingRegions.Add(r);
			return false;
		}, maxRegions);
		while (workingRegions.Count > 0)
		{
			Region region2 = workingRegions.RandomElementByWeight((Region r) => r.CellCount);
			if (region2.TryFindRandomCellInRegion((IntVec3 c) => (float)(c - root).LengthHorizontalSquared <= radSquared && (cellValidator == null || cellValidator(c)), out result))
			{
				workingRegions.Clear();
				return true;
			}
			workingRegions.Remove(region2);
		}
		result = IntVec3.Invalid;
		workingRegions.Clear();
		return false;
	}

	public static bool TryFindRandomReachableCellNearPosition(IntVec3 root, IntVec3 targetPos, Map map, float radius, TraverseParms traverseParms, Predicate<IntVec3> cellValidator, Predicate<Region> regionValidator, out IntVec3 result, int maxRegions = 999999)
	{
		if (map == null)
		{
			Log.ErrorOnce("Tried to find reachable cell in a null map", 61037855);
			result = IntVec3.Invalid;
			return false;
		}
		Region region = root.GetRegion(map);
		if (region == null)
		{
			result = IntVec3.Invalid;
			return false;
		}
		workingRegions.Clear();
		float radSquared = radius * radius;
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: true) && (regionValidator == null || regionValidator(r)), delegate(Region r)
		{
			if (radius > 1000f || r.extentsClose.ClosestDistSquaredTo(targetPos) <= radSquared)
			{
				workingRegions.Add(r);
			}
			return false;
		}, maxRegions);
		while (workingRegions.Count > 0)
		{
			Region region2 = workingRegions.RandomElementByWeight((Region r) => r.CellCount);
			if (region2.TryFindRandomCellInRegion((IntVec3 c) => (float)(c - targetPos).LengthHorizontalSquared <= radSquared && (cellValidator == null || cellValidator(c)), out result))
			{
				workingRegions.Clear();
				return true;
			}
			workingRegions.Remove(region2);
		}
		result = IntVec3.Invalid;
		workingRegions.Clear();
		return false;
	}

	public static IntVec3 RandomClosewalkCellNear(IntVec3 root, Map map, int radius, Predicate<IntVec3> extraValidator = null)
	{
		if (TryRandomClosewalkCellNear(root, map, radius, out var result, extraValidator))
		{
			return result;
		}
		return root;
	}

	public static bool TryRandomClosewalkCellNear(IntVec3 root, Map map, int radius, out IntVec3 result, Predicate<IntVec3> extraValidator = null)
	{
		return TryFindRandomReachableNearbyCell(root, map, radius, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblocked(forceFenceblocked: true), (IntVec3 c) => c.Standable(map) && (extraValidator == null || extraValidator(c)), null, out result);
	}

	public static IntVec3 RandomClosewalkCellNearNotForbidden(Pawn pawn, int radius, Predicate<IntVec3> validator = null)
	{
		IntVec3 position = pawn.Position;
		Map map = pawn.Map;
		if (!TryFindRandomReachableNearbyCell(position, map, radius, TraverseParms.For(TraverseMode.NoPassClosedDoors).WithFenceblockedOf(pawn), (IntVec3 c) => !c.IsForbidden(pawn) && c.Standable(map) && (validator == null || validator(c)), null, out var result))
		{
			return RandomClosewalkCellNear(position, map, radius, validator);
		}
		return result;
	}

	public static bool TryFindRandomCellInRegion(this Region reg, Predicate<IntVec3> validator, out IntVec3 result)
	{
		for (int i = 0; i < 10; i++)
		{
			result = reg.RandomCell;
			if (validator == null || validator(result))
			{
				return true;
			}
		}
		workingCells.Clear();
		workingCells.AddRange(reg.Cells);
		workingCells.Shuffle();
		for (int j = 0; j < workingCells.Count; j++)
		{
			result = workingCells[j];
			if (validator == null || validator(result))
			{
				return true;
			}
		}
		result = reg.RandomCell;
		return false;
	}

	public static bool TryFindRandomCellNear(IntVec3 root, Map map, int squareRadius, Predicate<IntVec3> validator, out IntVec3 result, int maxTries = -1)
	{
		int num = root.x - squareRadius;
		int num2 = root.x + squareRadius;
		int num3 = root.z - squareRadius;
		int num4 = root.z + squareRadius;
		int num5 = (num2 - num + 1) * (num4 - num3 + 1);
		if (num < 0)
		{
			num = 0;
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num2 > map.Size.x)
		{
			num2 = map.Size.x;
		}
		if (num4 > map.Size.z)
		{
			num4 = map.Size.z;
		}
		int num6;
		bool flag;
		if (maxTries < 0 || maxTries >= num5)
		{
			num6 = 20;
			flag = false;
		}
		else
		{
			num6 = maxTries;
			flag = true;
		}
		for (int i = 0; i < num6; i++)
		{
			IntVec3 intVec = new IntVec3(Rand.RangeInclusive(num, num2), 0, Rand.RangeInclusive(num3, num4));
			if (validator == null || validator(intVec))
			{
				if (DebugViewSettings.drawDestSearch)
				{
					map.debugDrawer.FlashCell(intVec, 0.5f, "found");
				}
				result = intVec;
				return true;
			}
			if (DebugViewSettings.drawDestSearch)
			{
				map.debugDrawer.FlashCell(intVec, 0f, "inv");
			}
		}
		if (flag)
		{
			result = root;
			return false;
		}
		workingListX.Clear();
		workingListZ.Clear();
		for (int j = num; j <= num2; j++)
		{
			workingListX.Add(j);
		}
		for (int k = num3; k <= num4; k++)
		{
			workingListZ.Add(k);
		}
		workingListX.Shuffle();
		workingListZ.Shuffle();
		for (int l = 0; l < workingListX.Count; l++)
		{
			for (int m = 0; m < workingListZ.Count; m++)
			{
				IntVec3 intVec = new IntVec3(workingListX[l], 0, workingListZ[m]);
				if (validator(intVec))
				{
					if (DebugViewSettings.drawDestSearch)
					{
						map.debugDrawer.FlashCell(intVec, 0.6f, "found2");
					}
					result = intVec;
					return true;
				}
				if (DebugViewSettings.drawDestSearch)
				{
					map.debugDrawer.FlashCell(intVec, 0.25f, "inv2");
				}
			}
		}
		result = root;
		return false;
	}

	public static bool TryFindRandomAdjacentCell(IntVec3 root, Map map, Predicate<IntVec3> validator, out IntVec3 result, bool diagonal = true)
	{
		IntVec3[] array = (diagonal ? GenAdj.AdjacentCells : GenAdj.CardinalDirections);
		List<int> obj = (diagonal ? adjacentIndices : cardinalIndices);
		obj.Shuffle();
		foreach (int item in obj)
		{
			result = root + array[item];
			if (validator(result))
			{
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindRandomPawnExitCell(Pawn searcher, out IntVec3 result)
	{
		return TryFindRandomEdgeCellWith((IntVec3 c) => (searcher.Map.TileInfo.AllowRoofedEdgeWalkIn || !searcher.Map.roofGrid.Roofed(c)) && c.WalkableBy(searcher.Map, searcher) && searcher.CanReach(c, PathEndMode.OnCell, Danger.Some), searcher.Map, 0f, out result);
	}

	public static bool TryFindRandomEdgeCellWith(Predicate<IntVec3> validator, Map map, float roadChance, out IntVec3 result)
	{
		if (Rand.Chance(roadChance) && map.roadInfo.roadEdgeTiles.Where((IntVec3 c) => validator(c)).TryRandomElement(out result))
		{
			return true;
		}
		for (int num = 0; num < 100; num++)
		{
			result = RandomEdgeCell(map);
			if (validator(result))
			{
				return true;
			}
		}
		if (mapEdgeCells == null || map.Size != mapEdgeCellsSize)
		{
			mapEdgeCellsSize = map.Size;
			mapEdgeCells = new List<IntVec3>();
			foreach (IntVec3 edgeCell in CellRect.WholeMap(map).EdgeCells)
			{
				mapEdgeCells.Add(edgeCell);
			}
		}
		mapEdgeCells.Shuffle();
		for (int num2 = 0; num2 < mapEdgeCells.Count; num2++)
		{
			try
			{
				if (validator(mapEdgeCells[num2]))
				{
					result = mapEdgeCells[num2];
					return true;
				}
			}
			catch (Exception arg)
			{
				Log.Error($"TryFindRandomEdgeCellWith exception validating {mapEdgeCells[num2]}: {arg}");
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindRandomEdgeCellWith(Predicate<IntVec3> validator, Map map, Rot4 dir, float roadChance, out IntVec3 result)
	{
		if (Rand.Value < roadChance)
		{
			bool flag = map.roadInfo.roadEdgeTiles.Where((IntVec3 c) => validator(c) && c.OnEdge(map, dir)).TryRandomElement(out result);
			if (flag)
			{
				return flag;
			}
		}
		for (int num = 0; num < 100; num++)
		{
			result = RandomEdgeCell(dir, map);
			if (validator(result))
			{
				return true;
			}
		}
		int asInt = dir.AsInt;
		if (mapSingleEdgeCells[asInt] == null || map.Size != mapSingleEdgeCellsSize)
		{
			mapSingleEdgeCellsSize = map.Size;
			mapSingleEdgeCells[asInt] = new List<IntVec3>();
			foreach (IntVec3 edgeCell in CellRect.WholeMap(map).GetEdgeCells(dir))
			{
				mapSingleEdgeCells[asInt].Add(edgeCell);
			}
		}
		List<IntVec3> list = mapSingleEdgeCells[asInt];
		list.Shuffle();
		int num2 = 0;
		for (int count = list.Count; num2 < count; num2++)
		{
			try
			{
				if (validator(list[num2]))
				{
					result = list[num2];
					return true;
				}
			}
			catch (Exception ex)
			{
				Log.Error("TryFindRandomEdgeCellWith exception validating " + list[num2].ToString() + ": " + ex.ToString());
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindRandomEdgeCellNearWith(IntVec3 near, float radius, Map map, Predicate<IntVec3> validator, out IntVec3 spot)
	{
		CellRect cellRect = CellRect.CenteredOn(near, Mathf.CeilToInt(radius)).ClipInsideMap(map);
		if (cellRect.ExpandedBy(1).Area == cellRect.ExpandedBy(1).ClipInsideMap(map).Area)
		{
			spot = IntVec3.Invalid;
			return false;
		}
		Predicate<IntVec3> predicate = (IntVec3 x) => x.InHorDistOf(near, radius) && x.OnEdge(map) && validator(x);
		if (CellRect.WholeMap(map).EdgeCellsCount < cellRect.Area)
		{
			return TryFindRandomEdgeCellWith(predicate, map, EdgeRoadChance_Ignore, out spot);
		}
		return TryFindRandomCellInsideWith(cellRect, predicate, out spot);
	}

	public static bool TryFindBestPawnStandCell(Pawn forPawn, out IntVec3 cell, bool cellByCell = false)
	{
		cell = IntVec3.Invalid;
		int num = -1;
		float radius = 10f;
		while (true)
		{
			tmpDistances.Clear();
			tmpParents.Clear();
			Dijkstra<IntVec3>.Run(forPawn.Position, (IntVec3 x) => GetAdjacentCardinalCellsForBestStandCell(x, radius, forPawn), delegate(IntVec3 from, IntVec3 to)
			{
				float num4 = 1f;
				if (from.x != to.x && from.z != to.z)
				{
					num4 = 1.4142135f;
				}
				if (!to.Standable(forPawn.Map))
				{
					num4 += 3f;
				}
				if (PawnUtility.AnyPawnBlockingPathAt(to, forPawn))
				{
					num4 = ((to.GetThingList(forPawn.Map).Find((Thing x) => x is Pawn && x.HostileTo(forPawn)) == null) ? (num4 + 15f) : (num4 + 40f));
				}
				if (to.GetEdifice(forPawn.Map) is Building_Door { FreePassage: false } building_Door)
				{
					num4 = ((!building_Door.PawnCanOpen(forPawn)) ? (num4 + 50f) : (num4 + 6f));
				}
				return num4;
			}, tmpDistances, tmpParents);
			if (tmpDistances.Count == num)
			{
				return false;
			}
			float num2 = 0f;
			foreach (KeyValuePair<IntVec3, float> tmpDistance in tmpDistances)
			{
				if ((!cell.IsValid || !(tmpDistance.Value >= num2)) && tmpDistance.Key.WalkableBy(forPawn.Map, forPawn) && !PawnUtility.AnyPawnBlockingPathAt(tmpDistance.Key, forPawn))
				{
					Building_Door door = tmpDistance.Key.GetDoor(forPawn.Map);
					if (door == null || door.FreePassage)
					{
						cell = tmpDistance.Key;
						num2 = tmpDistance.Value;
					}
				}
			}
			if (cell.IsValid)
			{
				if (!cellByCell)
				{
					return true;
				}
				IntVec3 intVec = cell;
				int num3 = 0;
				while (intVec.IsValid && intVec != forPawn.Position)
				{
					num3++;
					if (num3 >= 10000)
					{
						Log.Error("Too many iterations.");
						break;
					}
					if (intVec.WalkableBy(forPawn.Map, forPawn))
					{
						Building_Door door2 = intVec.GetDoor(forPawn.Map);
						if (door2 == null || door2.FreePassage)
						{
							cell = intVec;
						}
					}
					intVec = tmpParents[intVec];
				}
				return true;
			}
			if (radius > (float)forPawn.Map.Size.x && radius > (float)forPawn.Map.Size.z)
			{
				break;
			}
			radius *= 2f;
			num = tmpDistances.Count;
		}
		return false;
	}

	public static bool TryFindRandomCellInsideWith(CellRect cellRect, Predicate<IntVec3> predicate, out IntVec3 result)
	{
		int num = Mathf.Max(Mathf.RoundToInt(Mathf.Sqrt(cellRect.Area)), 5);
		for (int i = 0; i < num; i++)
		{
			IntVec3 randomCell = cellRect.RandomCell;
			if (predicate(randomCell))
			{
				result = randomCell;
				return true;
			}
		}
		tmpCells.Clear();
		foreach (IntVec3 item in cellRect)
		{
			tmpCells.Add(item);
		}
		tmpCells.Shuffle();
		int j = 0;
		for (int count = tmpCells.Count; j < count; j++)
		{
			if (predicate(tmpCells[j]))
			{
				result = tmpCells[j];
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static IntVec3 RandomSpawnCellForPawnNear(IntVec3 root, Map map, int firstTryWithRadius = 4)
	{
		if (TryFindRandomSpawnCellForPawnNear(root, map, out var result, firstTryWithRadius))
		{
			return result;
		}
		return root;
	}

	public static bool TryFindRandomSpawnCellForPawnNear(IntVec3 root, Map map, out IntVec3 result, int firstTryWithRadius = 4, Predicate<IntVec3> extraValidator = null)
	{
		if (root.Standable(map) && root.GetFirstPawn(map) == null)
		{
			result = root;
			return true;
		}
		bool rootFogged = root.Fogged(map);
		int num = firstTryWithRadius;
		for (int i = 0; i < 3; i++)
		{
			if (TryFindRandomReachableNearbyCell(root, map, num, TraverseParms.For(TraverseMode.NoPassClosedDoors), (IntVec3 c) => c.Standable(map) && (rootFogged || !c.Fogged(map)) && c.GetFirstPawn(map) == null && (extraValidator == null || extraValidator(c)), null, out result))
			{
				return true;
			}
			num *= 2;
		}
		num = firstTryWithRadius + 1;
		while (true)
		{
			if (TryRandomClosewalkCellNear(root, map, num, out result))
			{
				return true;
			}
			if (num > map.Size.x / 2 && num > map.Size.z / 2)
			{
				break;
			}
			num *= 2;
		}
		result = root;
		return false;
	}

	public static IntVec3 FindNoWipeSpawnLocNear(IntVec3 near, Map map, ThingDef thingToSpawn, Rot4 rot, int maxDist = 2, Predicate<IntVec3> extraValidator = null)
	{
		int num = GenRadial.NumCellsInRadius(maxDist);
		IntVec3 result = IntVec3.Invalid;
		float num2 = 0f;
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = near + GenRadial.RadialPattern[i];
			if (!intVec.InBounds(map))
			{
				continue;
			}
			CellRect cellRect = GenAdj.OccupiedRect(intVec, rot, thingToSpawn.size);
			if (!cellRect.InBounds(map) || !GenSight.LineOfSight(near, intVec, map, skipFirstCell: true) || (extraValidator != null && !extraValidator(intVec)) || (thingToSpawn.category == ThingCategory.Building && !GenConstruct.CanBuildOnTerrain(thingToSpawn, intVec, map, rot)))
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			tmpUniqueWipedThings.Clear();
			foreach (IntVec3 item in cellRect)
			{
				if (item.Impassable(map))
				{
					flag2 = true;
				}
				List<Thing> thingList = item.GetThingList(map);
				for (int j = 0; j < thingList.Count; j++)
				{
					if (thingList[j] is Pawn)
					{
						flag = true;
					}
					else if (GenSpawn.SpawningWipes(thingToSpawn, thingList[j].def) && !tmpUniqueWipedThings.Contains(thingList[j]))
					{
						tmpUniqueWipedThings.Add(thingList[j]);
					}
				}
			}
			if (flag && thingToSpawn.passability == Traversability.Impassable)
			{
				tmpUniqueWipedThings.Clear();
				continue;
			}
			if (flag2 && thingToSpawn.category == ThingCategory.Item)
			{
				tmpUniqueWipedThings.Clear();
				continue;
			}
			float num3 = 0f;
			for (int k = 0; k < tmpUniqueWipedThings.Count; k++)
			{
				if (tmpUniqueWipedThings[k].def.category == ThingCategory.Building && !tmpUniqueWipedThings[k].def.CostList.NullOrEmpty() && tmpUniqueWipedThings[k].def.CostStuffCount == 0)
				{
					List<ThingDefCountClass> list = tmpUniqueWipedThings[k].CostListAdjusted();
					for (int l = 0; l < list.Count; l++)
					{
						num3 += list[l].thingDef.GetStatValueAbstract(StatDefOf.MarketValue) * (float)list[l].count * (float)tmpUniqueWipedThings[k].stackCount;
					}
				}
				else
				{
					num3 += tmpUniqueWipedThings[k].MarketValue * (float)tmpUniqueWipedThings[k].stackCount;
				}
				if (tmpUniqueWipedThings[k].def.category == ThingCategory.Building || tmpUniqueWipedThings[k].def.category == ThingCategory.Item)
				{
					num3 = Mathf.Max(num3, 0.001f);
				}
			}
			tmpUniqueWipedThings.Clear();
			if (!result.IsValid || num3 < num2)
			{
				if (num3 == 0f)
				{
					return intVec;
				}
				result = intVec;
				num2 = num3;
			}
		}
		if (!result.IsValid)
		{
			return near;
		}
		return result;
	}

	private static IEnumerable<IntVec3> GetAdjacentCardinalCellsForBestStandCell(IntVec3 x, float radius, Pawn pawn)
	{
		if ((float)(x - pawn.Position).LengthManhattan > radius)
		{
			yield break;
		}
		for (int i = 0; i < 4; i++)
		{
			IntVec3 intVec = x + GenAdj.CardinalDirections[i];
			if (intVec.InBounds(pawn.Map) && intVec.WalkableBy(pawn.Map, pawn) && (!(intVec.GetEdifice(pawn.Map) is Building_Door building_Door) || building_Door.CanPhysicallyPass(pawn)))
			{
				yield return intVec;
			}
		}
	}

	public static IntVec3 StandableCellNear(IntVec3 root, Map map, float radius, Predicate<IntVec3> validator = null)
	{
		int num = GenRadial.NumCellsInRadius(radius);
		for (int i = 0; i < num; i++)
		{
			IntVec3 intVec = GenRadial.RadialPattern[i] + root;
			if ((validator == null || validator(intVec)) && intVec.Standable(map))
			{
				return intVec;
			}
		}
		return IntVec3.Invalid;
	}
}
