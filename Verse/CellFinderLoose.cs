using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public static class CellFinderLoose
	{
		public static IntVec3 RandomCellWith(Predicate<IntVec3> validator, Map map, int maxTries = 1000)
		{
			TryGetRandomCellWith(validator, map, maxTries, out IntVec3 result);
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

		public static IntVec3 GetFleeDest(Pawn pawn, List<Thing> threats, float distance = 23f)
		{
			if (pawn.RaceProps.Animal)
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

		public static IntVec3 TryFindCentralCell(Map map, int tightness, int minCellCount, Predicate<IntVec3> extraValidator = null)
		{
			int debug_numStand = 0;
			int debug_numRoom = 0;
			int debug_numTouch = 0;
			int debug_numRoomCellCount = 0;
			int debug_numExtraValidator = 0;
			Predicate<IntVec3> validator = delegate(IntVec3 c)
			{
				if (!c.Standable(map))
				{
					debug_numStand++;
					return false;
				}
				Room room = c.GetRoom(map);
				if (room == null)
				{
					debug_numRoom++;
					return false;
				}
				if (!room.TouchesMapEdge)
				{
					debug_numTouch++;
					return false;
				}
				if (room.CellCount < minCellCount)
				{
					debug_numRoomCellCount++;
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
				if (TryFindRandomNotEdgeCellWith((map.Size.x - num2) / 2, validator, map, out IntVec3 result))
				{
					return result;
				}
			}
			Log.Error("Found no good central spot. Choosing randomly. numStand=" + debug_numStand + ", numRoom=" + debug_numRoom + ", numTouch=" + debug_numTouch + ", numRoomCellCount=" + debug_numRoomCellCount + ", numExtraValidator=" + debug_numExtraValidator);
			return RandomCellWith((IntVec3 x) => x.Standable(map), map);
		}

		public static bool TryFindSkyfallerCell(ThingDef skyfaller, Map map, out IntVec3 cell, int minDistToEdge = 10, IntVec3 nearLoc = default(IntVec3), int nearLocMaxDist = -1, bool allowRoofedCells = true, bool allowCellsWithItems = false, bool allowCellsWithBuildings = false, bool colonyReachable = false, bool avoidColonistsIfExplosive = true, bool alwaysAvoidColonists = false, Predicate<IntVec3> extraValidator = null)
		{
			bool avoidColonists = (avoidColonistsIfExplosive && skyfaller.skyfaller.CausesExplosion) | alwaysAvoidColonists;
			Predicate<IntVec3> validator = delegate(IntVec3 x)
			{
				foreach (IntVec3 item in GenAdj.OccupiedRect(x, Rot4.North, skyfaller.size))
				{
					if (!item.InBounds(map) || item.Fogged(map) || !item.Standable(map) || (item.Roofed(map) && item.GetRoof(map).isThickRoof))
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
				return (extraValidator == null || extraValidator(x)) ? true : false;
			};
			if (nearLocMaxDist > 0)
			{
				return CellFinder.TryFindRandomCellNear(nearLoc, map, nearLocMaxDist, validator, out cell);
			}
			return TryFindRandomNotEdgeCellWith(minDistToEdge, validator, map, out cell);
		}
	}
}
