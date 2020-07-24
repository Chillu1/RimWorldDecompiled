using System.Collections.Generic;

namespace Verse
{
	public sealed class RegionGrid
	{
		private Map map;

		private Region[] regionGrid;

		private int curCleanIndex;

		public List<Room> allRooms = new List<Room>();

		public static HashSet<Region> allRegionsYielded = new HashSet<Region>();

		private const int CleanSquaresPerFrame = 16;

		public HashSet<Region> drawnRegions = new HashSet<Region>();

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
			if (!c.InBounds(map))
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
				return null;
			}
			if (!map.regionAndRoomUpdater.Enabled && map.regionAndRoomUpdater.AnythingToRebuild)
			{
				Log.Warning(string.Concat("Trying to get valid region at ", c, " but RegionAndRoomUpdater is disabled. The result may be incorrect."));
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
			if (!c.InBounds(map))
			{
				Log.Error("Tried to get valid region out of bounds at " + c);
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
			return regionGrid[map.cellIndices.CellToIndex(c)];
		}

		public void SetRegionAt(IntVec3 c, Region reg)
		{
			regionGrid[map.cellIndices.CellToIndex(c)] = reg;
		}

		public void UpdateClean()
		{
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
			if (map != Find.CurrentMap)
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
				if (DebugViewSettings.drawRooms)
				{
					intVec.GetRoom(map, RegionType.Set_All)?.DebugDraw();
				}
				if (DebugViewSettings.drawRoomGroups)
				{
					intVec.GetRoomGroup(map)?.DebugDraw();
				}
				if (DebugViewSettings.drawRegions || DebugViewSettings.drawRegionLinks || DebugViewSettings.drawRegionThings)
				{
					GetRegionAt_NoRebuild_InvalidAllowed(intVec)?.DebugDrawMouseover();
				}
			}
		}
	}
}
