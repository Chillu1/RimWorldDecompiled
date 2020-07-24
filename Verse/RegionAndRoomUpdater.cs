using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class RegionAndRoomUpdater
	{
		private Map map;

		private List<Region> newRegions = new List<Region>();

		private List<Room> newRooms = new List<Room>();

		private HashSet<Room> reusedOldRooms = new HashSet<Room>();

		private List<RoomGroup> newRoomGroups = new List<RoomGroup>();

		private HashSet<RoomGroup> reusedOldRoomGroups = new HashSet<RoomGroup>();

		private List<Region> currentRegionGroup = new List<Region>();

		private List<Room> currentRoomGroup = new List<Room>();

		private Stack<Room> tmpRoomStack = new Stack<Room>();

		private HashSet<Room> tmpVisitedRooms = new HashSet<Room>();

		private bool initialized;

		private bool working;

		private bool enabledInt = true;

		public bool Enabled
		{
			get
			{
				return enabledInt;
			}
			set
			{
				enabledInt = value;
			}
		}

		public bool AnythingToRebuild
		{
			get
			{
				if (!map.regionDirtyer.AnyDirty)
				{
					return !initialized;
				}
				return true;
			}
		}

		public RegionAndRoomUpdater(Map map)
		{
			this.map = map;
		}

		public void RebuildAllRegionsAndRooms()
		{
			if (!Enabled)
			{
				Log.Warning("Called RebuildAllRegionsAndRooms() but RegionAndRoomUpdater is disabled. Regions won't be rebuilt.");
			}
			map.temperatureCache.ResetTemperatureCache();
			map.regionDirtyer.SetAllDirty();
			TryRebuildDirtyRegionsAndRooms();
		}

		public void TryRebuildDirtyRegionsAndRooms()
		{
			if (working || !Enabled)
			{
				return;
			}
			working = true;
			if (!initialized)
			{
				RebuildAllRegionsAndRooms();
			}
			if (!map.regionDirtyer.AnyDirty)
			{
				working = false;
				return;
			}
			try
			{
				RegenerateNewRegionsFromDirtyCells();
				CreateOrUpdateRooms();
			}
			catch (Exception arg)
			{
				Log.Error("Exception while rebuilding dirty regions: " + arg);
			}
			newRegions.Clear();
			map.regionDirtyer.SetAllClean();
			initialized = true;
			working = false;
			if (DebugSettings.detectRegionListersBugs)
			{
				Autotests_RegionListers.CheckBugs(map);
			}
		}

		private void RegenerateNewRegionsFromDirtyCells()
		{
			newRegions.Clear();
			List<IntVec3> dirtyCells = map.regionDirtyer.DirtyCells;
			for (int i = 0; i < dirtyCells.Count; i++)
			{
				IntVec3 intVec = dirtyCells[i];
				if (intVec.GetRegion(map, RegionType.Set_All) == null)
				{
					Region region = map.regionMaker.TryGenerateRegionFrom(intVec);
					if (region != null)
					{
						newRegions.Add(region);
					}
				}
			}
		}

		private void CreateOrUpdateRooms()
		{
			newRooms.Clear();
			reusedOldRooms.Clear();
			newRoomGroups.Clear();
			reusedOldRoomGroups.Clear();
			int numRegionGroups = CombineNewRegionsIntoContiguousGroups();
			CreateOrAttachToExistingRooms(numRegionGroups);
			int numRoomGroups = CombineNewAndReusedRoomsIntoContiguousGroups();
			CreateOrAttachToExistingRoomGroups(numRoomGroups);
			NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperature();
			newRooms.Clear();
			reusedOldRooms.Clear();
			newRoomGroups.Clear();
			reusedOldRoomGroups.Clear();
		}

		private int CombineNewRegionsIntoContiguousGroups()
		{
			int num = 0;
			for (int i = 0; i < newRegions.Count; i++)
			{
				if (newRegions[i].newRegionGroupIndex < 0)
				{
					RegionTraverser.FloodAndSetNewRegionIndex(newRegions[i], num);
					num++;
				}
			}
			return num;
		}

		private void CreateOrAttachToExistingRooms(int numRegionGroups)
		{
			for (int i = 0; i < numRegionGroups; i++)
			{
				currentRegionGroup.Clear();
				for (int j = 0; j < newRegions.Count; j++)
				{
					if (newRegions[j].newRegionGroupIndex == i)
					{
						currentRegionGroup.Add(newRegions[j]);
					}
				}
				if (!currentRegionGroup[0].type.AllowsMultipleRegionsPerRoom())
				{
					if (currentRegionGroup.Count != 1)
					{
						Log.Error("Region type doesn't allow multiple regions per room but there are >1 regions in this group.");
					}
					Room room = Room.MakeNew(map);
					currentRegionGroup[0].Room = room;
					newRooms.Add(room);
					continue;
				}
				bool multipleOldNeighborRooms;
				Room room2 = FindCurrentRegionGroupNeighborWithMostRegions(out multipleOldNeighborRooms);
				if (room2 == null)
				{
					Room item = RegionTraverser.FloodAndSetRooms(currentRegionGroup[0], map, null);
					newRooms.Add(item);
				}
				else if (!multipleOldNeighborRooms)
				{
					for (int k = 0; k < currentRegionGroup.Count; k++)
					{
						currentRegionGroup[k].Room = room2;
					}
					reusedOldRooms.Add(room2);
				}
				else
				{
					RegionTraverser.FloodAndSetRooms(currentRegionGroup[0], map, room2);
					reusedOldRooms.Add(room2);
				}
			}
		}

		private int CombineNewAndReusedRoomsIntoContiguousGroups()
		{
			int num = 0;
			foreach (Room reusedOldRoom in reusedOldRooms)
			{
				reusedOldRoom.newOrReusedRoomGroupIndex = -1;
			}
			foreach (Room item in reusedOldRooms.Concat(newRooms))
			{
				if (item.newOrReusedRoomGroupIndex >= 0)
				{
					continue;
				}
				tmpRoomStack.Clear();
				tmpRoomStack.Push(item);
				item.newOrReusedRoomGroupIndex = num;
				while (tmpRoomStack.Count != 0)
				{
					Room room = tmpRoomStack.Pop();
					foreach (Room neighbor in room.Neighbors)
					{
						if (neighbor.newOrReusedRoomGroupIndex < 0 && ShouldBeInTheSameRoomGroup(room, neighbor))
						{
							neighbor.newOrReusedRoomGroupIndex = num;
							tmpRoomStack.Push(neighbor);
						}
					}
				}
				tmpRoomStack.Clear();
				num++;
			}
			return num;
		}

		private void CreateOrAttachToExistingRoomGroups(int numRoomGroups)
		{
			for (int i = 0; i < numRoomGroups; i++)
			{
				currentRoomGroup.Clear();
				foreach (Room reusedOldRoom in reusedOldRooms)
				{
					if (reusedOldRoom.newOrReusedRoomGroupIndex == i)
					{
						currentRoomGroup.Add(reusedOldRoom);
					}
				}
				for (int j = 0; j < newRooms.Count; j++)
				{
					if (newRooms[j].newOrReusedRoomGroupIndex == i)
					{
						currentRoomGroup.Add(newRooms[j]);
					}
				}
				bool multipleOldNeighborRoomGroups;
				RoomGroup roomGroup = FindCurrentRoomGroupNeighborWithMostRegions(out multipleOldNeighborRoomGroups);
				if (roomGroup == null)
				{
					RoomGroup roomGroup2 = RoomGroup.MakeNew(map);
					FloodAndSetRoomGroups(currentRoomGroup[0], roomGroup2);
					newRoomGroups.Add(roomGroup2);
				}
				else if (!multipleOldNeighborRoomGroups)
				{
					for (int k = 0; k < currentRoomGroup.Count; k++)
					{
						currentRoomGroup[k].Group = roomGroup;
					}
					reusedOldRoomGroups.Add(roomGroup);
				}
				else
				{
					FloodAndSetRoomGroups(currentRoomGroup[0], roomGroup);
					reusedOldRoomGroups.Add(roomGroup);
				}
			}
		}

		private void FloodAndSetRoomGroups(Room start, RoomGroup roomGroup)
		{
			tmpRoomStack.Clear();
			tmpRoomStack.Push(start);
			tmpVisitedRooms.Clear();
			tmpVisitedRooms.Add(start);
			while (tmpRoomStack.Count != 0)
			{
				Room room = tmpRoomStack.Pop();
				room.Group = roomGroup;
				foreach (Room neighbor in room.Neighbors)
				{
					if (!tmpVisitedRooms.Contains(neighbor) && ShouldBeInTheSameRoomGroup(room, neighbor))
					{
						tmpRoomStack.Push(neighbor);
						tmpVisitedRooms.Add(neighbor);
					}
				}
			}
			tmpVisitedRooms.Clear();
			tmpRoomStack.Clear();
		}

		private void NotifyAffectedRoomsAndRoomGroupsAndUpdateTemperature()
		{
			foreach (Room reusedOldRoom in reusedOldRooms)
			{
				reusedOldRoom.Notify_RoomShapeOrContainedBedsChanged();
			}
			for (int i = 0; i < newRooms.Count; i++)
			{
				newRooms[i].Notify_RoomShapeOrContainedBedsChanged();
			}
			foreach (RoomGroup reusedOldRoomGroup in reusedOldRoomGroups)
			{
				reusedOldRoomGroup.Notify_RoomGroupShapeChanged();
			}
			for (int j = 0; j < newRoomGroups.Count; j++)
			{
				RoomGroup roomGroup = newRoomGroups[j];
				roomGroup.Notify_RoomGroupShapeChanged();
				if (map.temperatureCache.TryGetAverageCachedRoomGroupTemp(roomGroup, out float result))
				{
					roomGroup.Temperature = result;
				}
			}
		}

		private Room FindCurrentRegionGroupNeighborWithMostRegions(out bool multipleOldNeighborRooms)
		{
			multipleOldNeighborRooms = false;
			Room room = null;
			for (int i = 0; i < currentRegionGroup.Count; i++)
			{
				foreach (Region item in currentRegionGroup[i].NeighborsOfSameType)
				{
					if (item.Room == null || reusedOldRooms.Contains(item.Room))
					{
						continue;
					}
					if (room == null)
					{
						room = item.Room;
					}
					else if (item.Room != room)
					{
						multipleOldNeighborRooms = true;
						if (item.Room.RegionCount > room.RegionCount)
						{
							room = item.Room;
						}
					}
				}
			}
			return room;
		}

		private RoomGroup FindCurrentRoomGroupNeighborWithMostRegions(out bool multipleOldNeighborRoomGroups)
		{
			multipleOldNeighborRoomGroups = false;
			RoomGroup roomGroup = null;
			for (int i = 0; i < currentRoomGroup.Count; i++)
			{
				foreach (Room neighbor in currentRoomGroup[i].Neighbors)
				{
					if (neighbor.Group == null || !ShouldBeInTheSameRoomGroup(currentRoomGroup[i], neighbor) || reusedOldRoomGroups.Contains(neighbor.Group))
					{
						continue;
					}
					if (roomGroup == null)
					{
						roomGroup = neighbor.Group;
					}
					else if (neighbor.Group != roomGroup)
					{
						multipleOldNeighborRoomGroups = true;
						if (neighbor.Group.RegionCount > roomGroup.RegionCount)
						{
							roomGroup = neighbor.Group;
						}
					}
				}
			}
			return roomGroup;
		}

		private bool ShouldBeInTheSameRoomGroup(Room a, Room b)
		{
			RegionType regionType = a.RegionType;
			RegionType regionType2 = b.RegionType;
			if (regionType == RegionType.Normal || regionType == RegionType.ImpassableFreeAirExchange)
			{
				if (regionType2 != RegionType.Normal)
				{
					return regionType2 == RegionType.ImpassableFreeAirExchange;
				}
				return true;
			}
			return false;
		}
	}
}
