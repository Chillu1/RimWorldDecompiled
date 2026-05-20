using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse;

public class RegionAndRoomUpdater
{
	private Map map;

	private List<Region> newRegions = new List<Region>();

	private List<District> newDistricts = new List<District>();

	private HashSet<District> reusedOldDistricts = new HashSet<District>();

	private List<Room> newRooms = new List<Room>();

	private HashSet<Room> reusedOldRooms = new HashSet<Room>();

	private List<Region> currentRegionGroup = new List<Region>();

	private List<District> currentDistrictGroup = new List<District>();

	private Stack<District> tmpDistrictStack = new Stack<District>();

	private HashSet<District> tmpVisitedDistricts = new HashSet<District>();

	private bool initialized;

	private bool working;

	private bool enabledInt = true;

	public Dictionary<int, Room> roomLookup = new Dictionary<int, Room>();

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
		map.TemperatureVacuumCache.ResetTemperatureCache();
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
		catch (Exception ex)
		{
			Log.Error("Exception while rebuilding dirty regions: " + ex);
		}
		newRegions.Clear();
		map.regionDirtyer.SetAllClean();
		initialized = true;
		working = false;
		if (DebugSettings.detectRegionListersBugs)
		{
			Autotests_RegionListers.CheckBugs(map);
		}
		map.events.Notify_RegionsRoomsChanged();
	}

	private void RegenerateNewRegionsFromDirtyCells()
	{
		newRegions.Clear();
		foreach (IntVec3 dirtyCell in map.regionDirtyer.DirtyCells)
		{
			if (dirtyCell.GetRegion(map, RegionType.Set_All) == null)
			{
				Region region = map.regionMaker.TryGenerateRegionFrom(dirtyCell);
				if (region != null)
				{
					newRegions.Add(region);
				}
			}
		}
	}

	private void CreateOrUpdateRooms()
	{
		newDistricts.Clear();
		reusedOldDistricts.Clear();
		newRooms.Clear();
		reusedOldRooms.Clear();
		int numRegionGroups = CombineNewRegionsIntoContiguousGroups();
		CreateOrAttachToExistingDistricts(numRegionGroups);
		int numRooms = CombineNewAndReusedDistrictsIntoContiguousRooms();
		CreateOrAttachToExistingRooms(numRooms);
		NotifyAffectedDistrictsAndRoomsAndUpdateTemperatureVacuum();
		newDistricts.Clear();
		reusedOldDistricts.Clear();
		newRooms.Clear();
		reusedOldRooms.Clear();
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

	private void CreateOrAttachToExistingDistricts(int numRegionGroups)
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
			if (!currentRegionGroup[0].type.AllowsMultipleRegionsPerDistrict())
			{
				if (currentRegionGroup.Count != 1)
				{
					Log.Error("Region type doesn't allow multiple regions per room but there are >1 regions in this group.");
				}
				District district = District.MakeNew(map);
				currentRegionGroup[0].District = district;
				newDistricts.Add(district);
				continue;
			}
			bool multipleOldNeighborDistricts;
			District district2 = FindCurrentRegionGroupNeighborWithMostRegions(out multipleOldNeighborDistricts);
			if (district2 == null)
			{
				District item = RegionTraverser.FloodAndSetDistricts(currentRegionGroup[0], map, null);
				newDistricts.Add(item);
			}
			else if (!multipleOldNeighborDistricts)
			{
				for (int k = 0; k < currentRegionGroup.Count; k++)
				{
					currentRegionGroup[k].District = district2;
				}
				reusedOldDistricts.Add(district2);
			}
			else
			{
				RegionTraverser.FloodAndSetDistricts(currentRegionGroup[0], map, district2);
				reusedOldDistricts.Add(district2);
			}
		}
	}

	private int CombineNewAndReusedDistrictsIntoContiguousRooms()
	{
		int num = 0;
		foreach (District reusedOldDistrict in reusedOldDistricts)
		{
			reusedOldDistrict.newOrReusedRoomIndex = -1;
		}
		foreach (District item in reusedOldDistricts.Concat(newDistricts))
		{
			if (item.newOrReusedRoomIndex >= 0)
			{
				continue;
			}
			tmpDistrictStack.Clear();
			tmpDistrictStack.Push(item);
			item.newOrReusedRoomIndex = num;
			while (tmpDistrictStack.Count != 0)
			{
				District district = tmpDistrictStack.Pop();
				foreach (District neighbor in district.Neighbors)
				{
					if (neighbor.newOrReusedRoomIndex < 0 && ShouldBeInTheSameRoom(district, neighbor))
					{
						neighbor.newOrReusedRoomIndex = num;
						tmpDistrictStack.Push(neighbor);
					}
				}
			}
			tmpDistrictStack.Clear();
			num++;
		}
		return num;
	}

	private void CreateOrAttachToExistingRooms(int numRooms)
	{
		for (int i = 0; i < numRooms; i++)
		{
			currentDistrictGroup.Clear();
			foreach (District reusedOldDistrict in reusedOldDistricts)
			{
				if (reusedOldDistrict.newOrReusedRoomIndex == i)
				{
					currentDistrictGroup.Add(reusedOldDistrict);
				}
			}
			for (int j = 0; j < newDistricts.Count; j++)
			{
				if (newDistricts[j].newOrReusedRoomIndex == i)
				{
					currentDistrictGroup.Add(newDistricts[j]);
				}
			}
			bool multipleOldNeighborRooms;
			Room room = FindCurrentRoomNeighborWithMostRegions(out multipleOldNeighborRooms);
			if (room == null)
			{
				Room room2 = Room.MakeNew(map);
				roomLookup[room2.ID] = room2;
				FloodAndSetRooms(currentDistrictGroup[0], room2);
				newRooms.Add(room2);
			}
			else if (!multipleOldNeighborRooms)
			{
				for (int k = 0; k < currentDistrictGroup.Count; k++)
				{
					currentDistrictGroup[k].Room = room;
				}
				reusedOldRooms.Add(room);
			}
			else
			{
				FloodAndSetRooms(currentDistrictGroup[0], room);
				reusedOldRooms.Add(room);
			}
		}
	}

	private void FloodAndSetRooms(District start, Room room)
	{
		tmpDistrictStack.Clear();
		tmpDistrictStack.Push(start);
		tmpVisitedDistricts.Clear();
		tmpVisitedDistricts.Add(start);
		while (tmpDistrictStack.Count != 0)
		{
			District district = tmpDistrictStack.Pop();
			district.Room = room;
			foreach (District neighbor in district.Neighbors)
			{
				if (!tmpVisitedDistricts.Contains(neighbor) && ShouldBeInTheSameRoom(district, neighbor))
				{
					tmpDistrictStack.Push(neighbor);
					tmpVisitedDistricts.Add(neighbor);
				}
			}
		}
		tmpVisitedDistricts.Clear();
		tmpDistrictStack.Clear();
	}

	private void NotifyAffectedDistrictsAndRoomsAndUpdateTemperatureVacuum()
	{
		foreach (District reusedOldDistrict in reusedOldDistricts)
		{
			reusedOldDistrict.Notify_RoomShapeOrContainedBedsChanged();
		}
		for (int i = 0; i < newDistricts.Count; i++)
		{
			newDistricts[i].Notify_RoomShapeOrContainedBedsChanged();
		}
		foreach (Room reusedOldRoom in reusedOldRooms)
		{
			reusedOldRoom.Notify_RoomShapeChanged();
		}
		for (int j = 0; j < newRooms.Count; j++)
		{
			Room room = newRooms[j];
			room.Notify_RoomShapeChanged();
			if (map.TemperatureVacuumCache.TryGetAverageCachedRoomTempVacuum(room, out var temperature, out var vacuum))
			{
				room.Temperature = temperature;
				room.Vacuum = vacuum;
			}
			else if (map.Biome.inVacuum)
			{
				room.Vacuum = 1f;
			}
		}
	}

	private District FindCurrentRegionGroupNeighborWithMostRegions(out bool multipleOldNeighborDistricts)
	{
		multipleOldNeighborDistricts = false;
		District district = null;
		for (int i = 0; i < currentRegionGroup.Count; i++)
		{
			foreach (Region item in currentRegionGroup[i].NeighborsOfSameType)
			{
				if (item.District == null || reusedOldDistricts.Contains(item.District))
				{
					continue;
				}
				if (district == null)
				{
					district = item.District;
				}
				else if (item.District != district)
				{
					multipleOldNeighborDistricts = true;
					if (item.District.RegionCount > district.RegionCount)
					{
						district = item.District;
					}
				}
			}
		}
		return district;
	}

	private Room FindCurrentRoomNeighborWithMostRegions(out bool multipleOldNeighborRooms)
	{
		multipleOldNeighborRooms = false;
		Room room = null;
		for (int i = 0; i < currentDistrictGroup.Count; i++)
		{
			foreach (District neighbor in currentDistrictGroup[i].Neighbors)
			{
				if (neighbor.Room == null || !ShouldBeInTheSameRoom(currentDistrictGroup[i], neighbor) || reusedOldRooms.Contains(neighbor.Room))
				{
					continue;
				}
				if (room == null)
				{
					room = neighbor.Room;
				}
				else if (neighbor.Room != room)
				{
					multipleOldNeighborRooms = true;
					if (neighbor.Room.RegionCount > room.RegionCount)
					{
						room = neighbor.Room;
					}
				}
			}
		}
		return room;
	}

	private bool ShouldBeInTheSameRoom(District a, District b)
	{
		RegionType regionType = a.RegionType;
		RegionType regionType2 = b.RegionType;
		if (regionType == RegionType.Normal || regionType == RegionType.ImpassableFreeAirExchange || regionType == RegionType.Fence)
		{
			if (regionType2 != RegionType.Normal && regionType2 != RegionType.ImpassableFreeAirExchange)
			{
				return regionType2 == RegionType.Fence;
			}
			return true;
		}
		return false;
	}
}
