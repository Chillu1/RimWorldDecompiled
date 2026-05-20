using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class CellFinderLoose
{
	public static IntVec3 RandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries = 1000)
	{
		TryGetRandomCellWith(validator, map, maxTries, out var result);
		return result;
	}

	public static bool TryGetRandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries, out IntVec3 result)
	{
		for (int i = 0; i < maxTries; i++)
		{
			result = CellFinder.RandomCell(map);
			if (validator(result))
			{
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool TryFindRandomNotEdgeCellWith(int minEdgeDistance, Predicate<IntVec3> validator, Map map, out IntVec3 result)
	{
		for (int i = 0; i < 1000; i++)
		{
			result = CellFinder.RandomNotEdgeCell(minEdgeDistance, map);
			if (result.IsValid && validator(result))
			{
				return true;
			}
		}
		result = IntVec3.Invalid;
		return false;
	}

	public static bool GetFleeExitPosition(Pawn pawn, float radius, out IntVec3 position)
	{
		return CellFinder.TryFindRandomEdgeCellNearWith(pawn.Position, radius, pawn.Map, (IntVec3 p) => GenSight.LineOfSight(pawn.Position, p, pawn.Map) && pawn.CanReach(p, PathEndMode.OnCell, Danger.Deadly), out position);
	}

	public static IntVec3 GetFleeDest(Pawn pawn, List<Thing> threats, float distance = 23f)
	{
		if (pawn.IsAnimal)
		{
			return GetFleeDestAnimal(pawn, threats, distance);
		}
		return GetFleeDestToolUser(pawn, threats, distance);
	}

	public static IntVec3 GetFleeDestAnimal(Pawn pawn, List<Thing> threats, float distance = 23f)
	{
		Vector3 normalized = (pawn.Position - threats[0].Position).ToVector3().normalized;
		float num = distance - pawn.Position.DistanceTo(threats[0].Position);
		for (float num2 = 200f; num2 <= 360f; num2 += 10f)
		{
			IntVec3 intVec = pawn.Position + (normalized.RotatedBy(Rand.Range((0f - num2) / 2f, num2 / 2f)) * num).ToIntVec3();
			if (CanFleeToLocation(pawn, intVec))
			{
				return intVec;
			}
		}
		float num3 = num;
		while (num3 * 3f > num)
		{
			IntVec3 intVec2 = pawn.Position + IntVec3Utility.RandomHorizontalOffset(num3);
			if (CanFleeToLocation(pawn, intVec2))
			{
				return intVec2;
			}
			num3 -= distance / 10f;
		}
		return pawn.Position;
	}

	public static bool CanFleeToLocation(Pawn pawn, IntVec3 location)
	{
		if (!location.Standable(pawn.Map))
		{
			return false;
		}
		if (!pawn.Map.pawnDestinationReservationManager.CanReserve(location, pawn))
		{
			return false;
		}
		if (location.GetRegion(pawn.Map).type == RegionType.Portal)
		{
			return false;
		}
		if (location.GetTerrain(pawn.Map).dangerous)
		{
			return false;
		}
		if (pawn.roping.IsRoped && !location.InHorDistOf(pawn.roping.RopedToSpot, 8f))
		{
			return false;
		}
		if (!pawn.CanReach(location, PathEndMode.OnCell, Danger.Deadly))
		{
			return false;
		}
		return true;
	}

	public static IntVec3 GetFleeDestToolUser(Pawn pawn, List<Thing> threats, float distance = 23f)
	{
		IntVec3 bestPos = pawn.Position;
		float bestScore = -1f;
		TraverseParms traverseParms = TraverseParms.For(pawn);
		RegionTraverser.BreadthFirstTraverse(pawn.GetRegion(), (Region from, Region reg) => reg.Allows(traverseParms, isDestination: false), delegate(Region reg)
		{
			Danger danger = reg.DangerFor(pawn);
			Map map = pawn.Map;
			foreach (IntVec3 cell in reg.Cells)
			{
				if (cell.Standable(map) && !reg.IsDoorway)
				{
					if (cell.GetTerrain(pawn.Map).dangerous)
					{
						return false;
					}
					Thing thing = null;
					float num = 0f;
					for (int i = 0; i < threats.Count; i++)
					{
						float num2 = cell.DistanceToSquared(threats[i].Position);
						if (thing == null || num2 < num)
						{
							thing = threats[i];
							num = num2;
						}
					}
					float num3 = Mathf.Sqrt(num);
					float num4 = Mathf.Pow(Mathf.Min(num3, distance), 1.2f);
					num4 *= Mathf.InverseLerp(50f, 0f, (cell - pawn.Position).LengthHorizontal);
					if (cell.GetRoom(map) != thing.GetRoom())
					{
						num4 *= 4.2f;
					}
					else if (num3 < 8f)
					{
						num4 *= 0.05f;
					}
					if (!map.pawnDestinationReservationManager.CanReserve(cell, pawn))
					{
						num4 *= 0.5f;
					}
					if (danger == Danger.Deadly)
					{
						num4 *= 0.8f;
					}
					if (ModsConfig.AnomalyActive && (pawn.RaceProps.Humanlike || pawn.IsPlayerControlled) && map.gameConditionManager.MapBrightness < 0.1f && map.glowGrid.PsychGlowAt(cell) == PsychGlow.Dark)
					{
						num4 *= 0.1f;
					}
					if (num4 > bestScore)
					{
						bestPos = cell;
						bestScore = num4;
					}
				}
			}
			return false;
		}, 20);
		return bestPos;
	}

	public static IntVec3 TryFindCentralCell(Map map, int tightness, int minCellCount, Predicate<IntVec3> extraValidator = null, bool returnInvalidOnFail = false)
	{
		int debug_numStand = 0;
		int debug_numDistrict = 0;
		int debug_numTouch = 0;
		int debug_numDistrictCellCount = 0;
		int debug_numExtraValidator = 0;
		Predicate<IntVec3> validator = delegate(IntVec3 c)
		{
			if (!c.Standable(map))
			{
				debug_numStand++;
				return false;
			}
			District district = c.GetDistrict(map);
			if (district == null)
			{
				debug_numDistrict++;
				return false;
			}
			if (!district.TouchesMapEdge)
			{
				debug_numTouch++;
				return false;
			}
			if (district.CellCount < minCellCount)
			{
				debug_numDistrictCellCount++;
				return false;
			}
			if (extraValidator != null && !extraValidator(c))
			{
				debug_numExtraValidator++;
				return false;
			}
			return true;
		};
		for (int num = tightness; num >= 1; num--)
		{
			int num2 = map.Size.x / num;
			if (TryFindRandomNotEdgeCellWith((map.Size.x - num2) / 2, validator, map, out var result))
			{
				return result;
			}
		}
		if (returnInvalidOnFail)
		{
			return IntVec3.Invalid;
		}
		Log.Error("Found no good central spot. Choosing randomly. numStand=" + debug_numStand + ", numDistrict=" + debug_numDistrict + ", numTouch=" + debug_numTouch + ", numDistrictCellCount=" + debug_numDistrictCellCount + ", numExtraValidator=" + debug_numExtraValidator);
		return RandomCellWith((IntVec3 x) => x.Standable(map), map);
	}

	public static bool TryFindSkyfallerCell(ThingDef skyfaller, Map map, TerrainAffordanceDef terrainAffordance, out IntVec3 cell, int minDistToEdge = 10, IntVec3 nearLoc = default(IntVec3), int nearLocMaxDist = -1, bool allowRoofedCells = true, bool allowCellsWithItems = false, bool allowCellsWithBuildings = false, bool colonyReachable = false, bool avoidColonistsIfExplosive = true, bool alwaysAvoidColonists = false, Predicate<IntVec3> extraValidator = null)
	{
		bool avoidColonists = (avoidColonistsIfExplosive && skyfaller.skyfaller.CausesExplosion) || alwaysAvoidColonists;
		if (nearLocMaxDist > 0)
		{
			return CellFinder.TryFindRandomCellNear(nearLoc, map, nearLocMaxDist, Validator, out cell);
		}
		return TryFindRandomNotEdgeCellWith(minDistToEdge, Validator, map, out cell);
		bool Validator(IntVec3 x)
		{
			foreach (IntVec3 item in GenAdj.OccupiedRect(x, Rot4.North, skyfaller.size))
			{
				if (!item.InBounds(map) || item.Fogged(map) || !item.Standable(map) || (item.Roofed(map) && item.GetRoof(map).isThickRoof))
				{
					return false;
				}
				if (!item.GetAffordances(map).Contains(terrainAffordance))
				{
					return false;
				}
				RoofDef roof = item.GetRoof(map);
				if (roof != null && roof.isThickRoof)
				{
					return false;
				}
				if (!allowRoofedCells && item.Roofed(map))
				{
					return false;
				}
				if (!allowCellsWithItems && item.GetFirstItem(map) != null)
				{
					return false;
				}
				if (!allowCellsWithBuildings && item.GetFirstBuilding(map) != null)
				{
					return false;
				}
				if (item.GetFirstSkyfaller(map) != null)
				{
					return false;
				}
				foreach (Thing thing in item.GetThingList(map))
				{
					if (thing.def.preventSkyfallersLandingOn)
					{
						return false;
					}
				}
			}
			if (avoidColonists && SkyfallerUtility.CanPossiblyFallOnColonist(skyfaller, x, map))
			{
				return false;
			}
			if (minDistToEdge > 0 && x.DistanceToEdge(map) < minDistToEdge)
			{
				return false;
			}
			if (colonyReachable && !map.reachability.CanReachColony(x))
			{
				return false;
			}
			if (extraValidator != null && !extraValidator(x))
			{
				return false;
			}
			return true;
		}
	}

	public static IntVec3 GetFallbackDest(Pawn pawn, List<Thing> threats, float maxDistance = 40f, float minDistanceFromThreat = 0f, float minDistanceFromRoot = 0f, int maxRegions = 20, Func<IntVec3, bool> validator = null)
	{
		IntVec3 bestPos = IntVec3.Invalid;
		float bestScore = -1f;
		TraverseParms traverseParms = TraverseParms.For(pawn);
		RegionTraverser.BreadthFirstTraverse(pawn.GetRegion(), (Region from, Region reg) => reg.Allows(traverseParms, isDestination: false), delegate(Region reg)
		{
			reg.DangerFor(pawn);
			Map map = pawn.Map;
			foreach (IntVec3 cell in reg.Cells)
			{
				if ((validator == null || validator(cell)) && cell.Standable(map))
				{
					int num = cell.DistanceToSquared(pawn.Position);
					if (!((float)num < minDistanceFromRoot * minDistanceFromRoot) && !((float)num > maxDistance * maxDistance))
					{
						Thing thing = null;
						float num2 = 0f;
						for (int i = 0; i < threats.Count; i++)
						{
							int num3 = cell.DistanceToSquared(threats[i].Position);
							if (thing == null || (float)num3 < num2)
							{
								thing = threats[i];
								num2 = num3;
							}
						}
						if (!(num2 < minDistanceFromThreat * minDistanceFromThreat))
						{
							float num4 = Mathf.Sqrt(num2);
							if (!map.pawnDestinationReservationManager.CanReserve(cell, pawn))
							{
								num4 *= 0.5f;
							}
							num4 += CoverUtility.TotalSurroundingCoverScore(cell, pawn.Map);
							if (num4 > bestScore)
							{
								bestPos = cell;
								bestScore = num4;
							}
						}
					}
				}
			}
			return false;
		}, maxRegions);
		return bestPos;
	}
}
