using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public class Reachability
	{
		private Map map;

		private Queue<Region> openQueue = new Queue<Region>();

		private List<Region> startingRegions = new List<Region>();

		private List<Region> destRegions = new List<Region>();

		private uint reachedIndex = 1u;

		private int numRegionsOpened;

		private bool working;

		private ReachabilityCache cache = new ReachabilityCache();

		private PathGrid pathGrid;

		private RegionGrid regionGrid;

		public Reachability(Map map)
		{
			this.map = map;
		}

		public void ClearCache()
		{
			if (cache.Count > 0)
			{
				cache.Clear();
			}
		}

		public void ClearCacheFor(Pawn pawn)
		{
			cache.ClearFor(pawn);
		}

		public void ClearCacheForHostile(Thing hostileTo)
		{
			cache.ClearForHostile(hostileTo);
		}

		private void QueueNewOpenRegion(Region region)
		{
			if (region == null)
			{
				Log.ErrorOnce("Tried to queue null region.", 881121);
				return;
			}
			if (region.reachedIndex == reachedIndex)
			{
				Log.ErrorOnce("Region is already reached; you can't open it. Region: " + region.ToString(), 719991);
				return;
			}
			openQueue.Enqueue(region);
			region.reachedIndex = reachedIndex;
			numRegionsOpened++;
		}

		private uint NewReachedIndex()
		{
			return reachedIndex++;
		}

		private void FinalizeCheck()
		{
			working = false;
		}

		public bool CanReachNonLocal(IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			if (dest.Map != null && dest.Map != map)
			{
				return false;
			}
			return CanReach(start, (LocalTargetInfo)dest, peMode, traverseMode, maxDanger);
		}

		public bool CanReachNonLocal(IntVec3 start, TargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			if (dest.Map != null && dest.Map != map)
			{
				return false;
			}
			return CanReach(start, (LocalTargetInfo)dest, peMode, traverseParams);
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseMode traverseMode, Danger maxDanger)
		{
			return CanReach(start, dest, peMode, TraverseParms.For(traverseMode, maxDanger));
		}

		public bool CanReach(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			if (working)
			{
				Log.ErrorOnce("Called CanReach() while working. This should never happen. Suppressing further errors.", 7312233);
				return false;
			}
			if (traverseParams.pawn != null)
			{
				if (!traverseParams.pawn.Spawned)
				{
					return false;
				}
				if (traverseParams.pawn.Map != map)
				{
					Log.Error("Called CanReach() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=" + traverseParams.pawn + " pawn.Map=" + traverseParams.pawn.Map + " map=" + map);
					return false;
				}
			}
			if (ReachabilityImmediate.CanReachImmediate(start, dest, map, peMode, traverseParams.pawn))
			{
				return true;
			}
			if (!dest.IsValid)
			{
				return false;
			}
			if (dest.HasThing && dest.Thing.Map != map)
			{
				return false;
			}
			if (!start.InBounds(map) || !dest.Cell.InBounds(map))
			{
				return false;
			}
			if ((peMode == PathEndMode.OnCell || peMode == PathEndMode.Touch || peMode == PathEndMode.ClosestTouch) && traverseParams.mode != TraverseMode.NoPassClosedDoorsOrWater && traverseParams.mode != TraverseMode.PassAllDestroyableThingsNotWater)
			{
				Room room = RegionAndRoomQuery.RoomAtFast(start, map);
				if (room != null && room == RegionAndRoomQuery.RoomAtFast(dest.Cell, map))
				{
					return true;
				}
			}
			if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
			{
				TraverseParms traverseParams2 = traverseParams;
				traverseParams2.mode = TraverseMode.PassDoors;
				if (CanReach(start, dest, peMode, traverseParams2))
				{
					return true;
				}
			}
			dest = (LocalTargetInfo)GenPath.ResolvePathMode(traverseParams.pawn, dest.ToTargetInfo(map), ref peMode);
			working = true;
			try
			{
				pathGrid = map.pathGrid;
				regionGrid = map.regionGrid;
				reachedIndex++;
				destRegions.Clear();
				switch (peMode)
				{
				case PathEndMode.OnCell:
				{
					Region region = dest.Cell.GetRegion(map);
					if (region != null && region.Allows(traverseParams, isDestination: true))
					{
						destRegions.Add(region);
					}
					break;
				}
				case PathEndMode.Touch:
					TouchPathEndModeUtility.AddAllowedAdjacentRegions(dest, traverseParams, map, destRegions);
					break;
				}
				if (destRegions.Count == 0 && traverseParams.mode != TraverseMode.PassAllDestroyableThings && traverseParams.mode != TraverseMode.PassAllDestroyableThingsNotWater)
				{
					FinalizeCheck();
					return false;
				}
				destRegions.RemoveDuplicates();
				openQueue.Clear();
				numRegionsOpened = 0;
				DetermineStartRegions(start);
				if (openQueue.Count == 0 && traverseParams.mode != TraverseMode.PassAllDestroyableThings && traverseParams.mode != TraverseMode.PassAllDestroyableThingsNotWater)
				{
					FinalizeCheck();
					return false;
				}
				if (startingRegions.Any() && destRegions.Any() && CanUseCache(traverseParams.mode))
				{
					switch (GetCachedResult(traverseParams))
					{
					case BoolUnknown.True:
						FinalizeCheck();
						return true;
					case BoolUnknown.False:
						FinalizeCheck();
						return false;
					}
				}
				if (traverseParams.mode == TraverseMode.PassAllDestroyableThings || traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater || traverseParams.mode == TraverseMode.NoPassClosedDoorsOrWater)
				{
					bool result = CheckCellBasedReachability(start, dest, peMode, traverseParams);
					FinalizeCheck();
					return result;
				}
				bool result2 = CheckRegionBasedReachability(traverseParams);
				FinalizeCheck();
				return result2;
			}
			finally
			{
				working = false;
			}
		}

		private void DetermineStartRegions(IntVec3 start)
		{
			startingRegions.Clear();
			if (pathGrid.WalkableFast(start))
			{
				Region validRegionAt = regionGrid.GetValidRegionAt(start);
				QueueNewOpenRegion(validRegionAt);
				startingRegions.Add(validRegionAt);
				return;
			}
			for (int i = 0; i < 8; i++)
			{
				IntVec3 intVec = start + GenAdj.AdjacentCells[i];
				if (intVec.InBounds(map) && pathGrid.WalkableFast(intVec))
				{
					Region validRegionAt2 = regionGrid.GetValidRegionAt(intVec);
					if (validRegionAt2 != null && validRegionAt2.reachedIndex != reachedIndex)
					{
						QueueNewOpenRegion(validRegionAt2);
						startingRegions.Add(validRegionAt2);
					}
				}
			}
		}

		private BoolUnknown GetCachedResult(TraverseParms traverseParams)
		{
			bool flag = false;
			for (int i = 0; i < startingRegions.Count; i++)
			{
				for (int j = 0; j < destRegions.Count; j++)
				{
					if (destRegions[j] == startingRegions[i])
					{
						return BoolUnknown.True;
					}
					switch (cache.CachedResultFor(startingRegions[i].Room, destRegions[j].Room, traverseParams))
					{
					case BoolUnknown.True:
						return BoolUnknown.True;
					case BoolUnknown.Unknown:
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return BoolUnknown.False;
			}
			return BoolUnknown.Unknown;
		}

		private bool CheckRegionBasedReachability(TraverseParms traverseParams)
		{
			while (openQueue.Count > 0)
			{
				Region region = openQueue.Dequeue();
				for (int i = 0; i < region.links.Count; i++)
				{
					RegionLink regionLink = region.links[i];
					for (int j = 0; j < 2; j++)
					{
						Region region2 = regionLink.regions[j];
						if (region2 == null || region2.reachedIndex == reachedIndex || !region2.type.Passable() || !region2.Allows(traverseParams, isDestination: false))
						{
							continue;
						}
						if (destRegions.Contains(region2))
						{
							for (int k = 0; k < startingRegions.Count; k++)
							{
								cache.AddCachedResult(startingRegions[k].Room, region2.Room, traverseParams, reachable: true);
							}
							return true;
						}
						QueueNewOpenRegion(region2);
					}
				}
			}
			for (int l = 0; l < startingRegions.Count; l++)
			{
				for (int m = 0; m < destRegions.Count; m++)
				{
					cache.AddCachedResult(startingRegions[l].Room, destRegions[m].Room, traverseParams, reachable: false);
				}
			}
			return false;
		}

		private bool CheckCellBasedReachability(IntVec3 start, LocalTargetInfo dest, PathEndMode peMode, TraverseParms traverseParams)
		{
			IntVec3 foundCell = IntVec3.Invalid;
			Region[] directRegionGrid = regionGrid.DirectGrid;
			PathGrid pathGrid = map.pathGrid;
			CellIndices cellIndices = map.cellIndices;
			map.floodFiller.FloodFill(start, delegate(IntVec3 c)
			{
				int num = cellIndices.CellToIndex(c);
				if ((traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater || traverseParams.mode == TraverseMode.NoPassClosedDoorsOrWater) && c.GetTerrain(map).IsWater)
				{
					return false;
				}
				if (traverseParams.mode == TraverseMode.PassAllDestroyableThings || traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater)
				{
					if (!pathGrid.WalkableFast(num))
					{
						Building edifice = c.GetEdifice(map);
						if (edifice == null || !PathFinder.IsDestroyable(edifice))
						{
							return false;
						}
					}
				}
				else if (traverseParams.mode != TraverseMode.NoPassClosedDoorsOrWater)
				{
					Log.ErrorOnce("Do not use this method for non-cell based modes!", 938476762);
					if (!pathGrid.WalkableFast(num))
					{
						return false;
					}
				}
				Region region = directRegionGrid[num];
				return (region == null || region.Allows(traverseParams, isDestination: false)) ? true : false;
			}, delegate(IntVec3 c)
			{
				if (ReachabilityImmediate.CanReachImmediate(c, dest, map, peMode, traverseParams.pawn))
				{
					foundCell = c;
					return true;
				}
				return false;
			});
			if (foundCell.IsValid)
			{
				if (CanUseCache(traverseParams.mode))
				{
					Region validRegionAt = regionGrid.GetValidRegionAt(foundCell);
					if (validRegionAt != null)
					{
						for (int i = 0; i < startingRegions.Count; i++)
						{
							cache.AddCachedResult(startingRegions[i].Room, validRegionAt.Room, traverseParams, reachable: true);
						}
					}
				}
				return true;
			}
			if (CanUseCache(traverseParams.mode))
			{
				for (int j = 0; j < startingRegions.Count; j++)
				{
					for (int k = 0; k < destRegions.Count; k++)
					{
						cache.AddCachedResult(startingRegions[j].Room, destRegions[k].Room, traverseParams, reachable: false);
					}
				}
			}
			return false;
		}

		public bool CanReachColony(IntVec3 c)
		{
			return CanReachFactionBase(c, Faction.OfPlayer);
		}

		public bool CanReachFactionBase(IntVec3 c, Faction factionBaseFaction)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return CanReach(c, MapGenerator.PlayerStartSpot, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors));
			}
			if (!c.Walkable(map))
			{
				return false;
			}
			Faction faction = map.ParentFaction ?? Faction.OfPlayer;
			List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(faction);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].CanReach(c, PathEndMode.OnCell, Danger.Deadly))
				{
					return true;
				}
			}
			TraverseParms traverseParams = TraverseParms.For(TraverseMode.PassDoors);
			if (faction == Faction.OfPlayer)
			{
				List<Building> allBuildingsColonist = map.listerBuildings.allBuildingsColonist;
				for (int j = 0; j < allBuildingsColonist.Count; j++)
				{
					if (CanReach(c, allBuildingsColonist[j], PathEndMode.Touch, traverseParams))
					{
						return true;
					}
				}
			}
			else
			{
				List<Thing> list2 = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
				for (int k = 0; k < list2.Count; k++)
				{
					if (list2[k].Faction == faction && CanReach(c, list2[k], PathEndMode.Touch, traverseParams))
					{
						return true;
					}
				}
			}
			return CanReachBiggestMapEdgeRoom(c);
		}

		public bool CanReachBiggestMapEdgeRoom(IntVec3 c)
		{
			Room room = null;
			for (int i = 0; i < map.regionGrid.allRooms.Count; i++)
			{
				Room room2 = map.regionGrid.allRooms[i];
				if (room2.TouchesMapEdge && (room == null || room2.RegionCount > room.RegionCount))
				{
					room = room2;
				}
			}
			if (room == null)
			{
				return false;
			}
			return CanReach(c, room.Regions[0].AnyCell, PathEndMode.OnCell, TraverseParms.For(TraverseMode.PassDoors));
		}

		public bool CanReachMapEdge(IntVec3 c, TraverseParms traverseParms)
		{
			if (traverseParms.pawn != null)
			{
				if (!traverseParms.pawn.Spawned)
				{
					return false;
				}
				if (traverseParms.pawn.Map != map)
				{
					Log.Error("Called CanReachMapEdge() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=" + traverseParms.pawn + " pawn.Map=" + traverseParms.pawn.Map + " map=" + map);
					return false;
				}
			}
			Region region = c.GetRegion(map);
			if (region == null)
			{
				return false;
			}
			if (region.Room.TouchesMapEdge)
			{
				return true;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, isDestination: false);
			bool foundReg = false;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (r.Room.TouchesMapEdge)
				{
					foundReg = true;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999);
			return foundReg;
		}

		public bool CanReachUnfogged(IntVec3 c, TraverseParms traverseParms)
		{
			if (traverseParms.pawn != null)
			{
				if (!traverseParms.pawn.Spawned)
				{
					return false;
				}
				if (traverseParms.pawn.Map != map)
				{
					Log.Error("Called CanReachUnfogged() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=" + traverseParms.pawn + " pawn.Map=" + traverseParms.pawn.Map + " map=" + map);
					return false;
				}
			}
			if (!c.InBounds(map))
			{
				return false;
			}
			if (!c.Fogged(map))
			{
				return true;
			}
			Region region = c.GetRegion(map);
			if (region == null)
			{
				return false;
			}
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParms, isDestination: false);
			bool foundReg = false;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				if (!r.AnyCell.Fogged(map))
				{
					foundReg = true;
					return true;
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 9999);
			return foundReg;
		}

		private bool CanUseCache(TraverseMode mode)
		{
			if (mode != TraverseMode.PassAllDestroyableThingsNotWater)
			{
				return mode != TraverseMode.NoPassClosedDoorsOrWater;
			}
			return false;
		}
	}
}
