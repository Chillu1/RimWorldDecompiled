using System;
using System.Collections.Generic;

namespace Verse;

public sealed class RegionGrid : IDisposable
{
	private readonly Map map;

	private Region[] regionGrid;

	private int curCleanIndex;

	private bool disposed;

	public List<District> allDistricts = new List<District>();

	private List<Room> allRooms = new List<Room>();

	private Dictionary<int, Room> roomLookup = new Dictionary<int, Room>();

	private static HashSet<Region> allRegionsYielded = new HashSet<Region>();

	private const int CleanSquaresPerFrame = 16;

	private HashSet<Region> drawnRegions = new HashSet<Region>();

	public IReadOnlyList<Room> AllRooms => allRooms;

	public IReadOnlyDictionary<int, Room> RoomLookup => roomLookup;

	public Region[] DirectGrid
	{
		get
		{
			if (!map.regionAndRoomUpdater.Enabled && map.regionAndRoomUpdater.AnythingToRebuild)
			{
				Log.Warning("Trying to get the region grid but RegionAndRoomUpdater is disabled. The result may be incorrect.");
			}
			map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
			return regionGrid;
		}
	}

	public IEnumerable<Region> AllRegions_NoRebuild_InvalidAllowed
	{
		get
		{
			if (disposed)
			{
				yield break;
			}
			allRegionsYielded.Clear();
			try
			{
				int count = map.cellIndices.NumGridCells;
				for (int i = 0; i < count; i++)
				{
					if (regionGrid[i] != null && !allRegionsYielded.Contains(regionGrid[i]))
					{
						yield return regionGrid[i];
						allRegionsYielded.Add(regionGrid[i]);
					}
				}
			}
			finally
			{
				allRegionsYielded.Clear();
			}
		}
	}

	public IEnumerable<Region> AllRegions
	{
		get
		{
			if (disposed)
			{
				yield break;
			}
			if (!map.regionAndRoomUpdater.Enabled && map.regionAndRoomUpdater.AnythingToRebuild)
			{
				Log.Warning("Trying to get all valid regions but RegionAndRoomUpdater is disabled. The result may be incorrect.");
			}
			map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
			allRegionsYielded.Clear();
			try
			{
				int count = map.cellIndices.NumGridCells;
				for (int i = 0; i < count; i++)
				{
					if (regionGrid[i] != null && regionGrid[i].valid && !allRegionsYielded.Contains(regionGrid[i]))
					{
						yield return regionGrid[i];
						allRegionsYielded.Add(regionGrid[i]);
					}
				}
			}
			finally
			{
				allRegionsYielded.Clear();
			}
		}
	}

	public RegionGrid(Map map)
	{
		this.map = map;
		regionGrid = new Region[map.cellIndices.NumGridCells];
	}

	public Region GetValidRegionAt(IntVec3 c)
	{
		if (disposed)
		{
			return null;
		}
		if (!c.InBounds(map))
		{
			IntVec3 intVec = c;
			Log.Error("Tried to get valid region out of bounds at " + intVec.ToString());
			return null;
		}
		if (!map.regionAndRoomUpdater.Enabled && map.regionAndRoomUpdater.AnythingToRebuild)
		{
			IntVec3 intVec = c;
			Log.Warning("Trying to get valid region at " + intVec.ToString() + " but RegionAndRoomUpdater is disabled. The result may be incorrect.");
		}
		map.regionAndRoomUpdater.TryRebuildDirtyRegionsAndRooms();
		Region region = regionGrid[map.cellIndices.CellToIndex(c)];
		if (region != null && region.valid)
		{
			return region;
		}
		return null;
	}

	public Region GetValidRegionAt_NoRebuild(IntVec3 c)
	{
		if (disposed)
		{
			return null;
		}
		if (!c.InBounds(map))
		{
			IntVec3 intVec = c;
			Log.Error("Tried to get valid region out of bounds at " + intVec.ToString());
			return null;
		}
		Region region = regionGrid[map.cellIndices.CellToIndex(c)];
		if (region != null && region.valid)
		{
			return region;
		}
		return null;
	}

	public Region GetRegionAt_NoRebuild_InvalidAllowed(IntVec3 c)
	{
		if (disposed)
		{
			return null;
		}
		return regionGrid[map.cellIndices.CellToIndex(c)];
	}

	public void SetRegionAt(IntVec3 c, Region reg)
	{
		regionGrid[map.cellIndices.CellToIndex(c)] = reg;
	}

	public void RoomAdded(Room room)
	{
		allRooms.Add(room);
		roomLookup[room.ID] = room;
	}

	public void RoomRemoved(Room room)
	{
		allRooms.Remove(room);
		roomLookup.Remove(room.ID);
	}

	public void UpdateClean()
	{
		if (disposed)
		{
			return;
		}
		for (int i = 0; i < 16; i++)
		{
			if (curCleanIndex >= regionGrid.Length)
			{
				curCleanIndex = 0;
			}
			Region region = regionGrid[curCleanIndex];
			if (region != null && !region.valid)
			{
				regionGrid[curCleanIndex] = null;
			}
			curCleanIndex++;
		}
	}

	public void DebugDraw()
	{
		if (map != Find.CurrentMap || disposed)
		{
			return;
		}
		if (DebugViewSettings.drawRegionTraversal)
		{
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			currentViewRect.ClipInsideMap(map);
			foreach (IntVec3 item in currentViewRect)
			{
				Region validRegionAt = GetValidRegionAt(item);
				if (validRegionAt != null && !drawnRegions.Contains(validRegionAt))
				{
					validRegionAt.DebugDraw();
					drawnRegions.Add(validRegionAt);
				}
			}
			drawnRegions.Clear();
		}
		IntVec3 intVec = UI.MouseCell();
		if (intVec.InBounds(map))
		{
			if (DebugViewSettings.drawDistricts)
			{
				intVec.GetDistrict(map)?.DebugDraw();
			}
			if (DebugViewSettings.drawRooms)
			{
				intVec.GetRoom(map)?.DebugDraw();
			}
			if (DebugViewSettings.drawRegions || DebugViewSettings.drawRegionLinks || DebugViewSettings.drawRegionThings)
			{
				GetRegionAt_NoRebuild_InvalidAllowed(intVec)?.DebugDrawMouseover();
			}
		}
	}

	public void Dispose()
	{
		regionGrid = null;
		allDistricts.Clear();
		allRooms.Clear();
		roomLookup.Clear();
		allRegionsYielded.Clear();
		disposed = true;
	}
}
