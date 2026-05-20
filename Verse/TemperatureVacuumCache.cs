using System.Collections.Generic;

namespace Verse;

public sealed class TemperatureVacuumCache : IExposable
{
	private readonly Map map;

	internal TemperatureVacuumSaveLoad TemperatureVacuumSaveLoad;

	private readonly CachedTempInfo[] tempCache;

	private readonly HashSet<int> processedRoomIDs = new HashSet<int>();

	private readonly List<CachedTempInfo> relevantTempInfoList = new List<CachedTempInfo>();

	public TemperatureVacuumCache(Map map)
	{
		this.map = map;
		tempCache = new CachedTempInfo[map.cellIndices.NumGridCells];
		TemperatureVacuumSaveLoad = new TemperatureVacuumSaveLoad(map);
	}

	public void ResetTemperatureCache()
	{
		int numGridCells = map.cellIndices.NumGridCells;
		for (int i = 0; i < numGridCells; i++)
		{
			tempCache[i].Reset();
		}
	}

	public void ExposeData()
	{
		TemperatureVacuumSaveLoad.DoExposeWork();
	}

	public void ResetCachedCellInfo(IntVec3 c)
	{
		tempCache[map.cellIndices.CellToIndex(c)].Reset();
	}

	private void SetCachedCellInfo(IntVec3 c, CachedTempInfo info)
	{
		tempCache[map.cellIndices.CellToIndex(c)] = info;
	}

	public void TryCacheRegionTempInfo(IntVec3 c, Region reg)
	{
		Room room = reg.Room;
		if (room != null)
		{
			SetCachedCellInfo(c, new CachedTempInfo(room.ID, room.CellCount, room.Temperature, room.Vacuum));
		}
	}

	public bool TryGetAverageCachedRoomTempVacuum(Room r, out float temperature, out float vacuum)
	{
		CellIndices cellIndices = map.cellIndices;
		foreach (IntVec3 cell in r.Cells)
		{
			CachedTempInfo item = map.TemperatureVacuumCache.tempCache[cellIndices.CellToIndex(cell)];
			if (item.numCells > 0 && !processedRoomIDs.Contains(item.roomID))
			{
				relevantTempInfoList.Add(item);
				processedRoomIDs.Add(item.roomID);
			}
		}
		int num = 0;
		float num2 = 0f;
		float num3 = 0f;
		foreach (CachedTempInfo relevantTempInfo in relevantTempInfoList)
		{
			num += relevantTempInfo.numCells;
			num2 += relevantTempInfo.temperature * (float)relevantTempInfo.numCells;
			num3 += relevantTempInfo.vacuum * (float)relevantTempInfo.numCells;
		}
		temperature = num2 / (float)num;
		vacuum = num3 / (float)num;
		bool result = !relevantTempInfoList.NullOrEmpty();
		processedRoomIDs.Clear();
		relevantTempInfoList.Clear();
		return result;
	}
}
