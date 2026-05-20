using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Verse;

public class AreaSource : IPathFinderDataSource, IDisposable
{
	private readonly Map map;

	private readonly int cellCount;

	private readonly Dictionary<Area, NativeBitArray> areaToBits = new Dictionary<Area, NativeBitArray>();

	private readonly Dictionary<Area, List<IntVec3>> areaToDeltas = new Dictionary<Area, List<IntVec3>>();

	private readonly List<Area> tmpToPrune = new List<Area>();

	public AreaSource(Map map)
	{
		this.map = map;
		cellCount = map.cellIndices.NumGridCells;
	}

	public void Dispose()
	{
		foreach (NativeBitArray value in areaToBits.Values)
		{
			value.Dispose();
		}
	}

	public NativeBitArray DataForArea(Area area)
	{
		return areaToBits[area];
	}

	public void ComputeAll(IEnumerable<PathRequest> _)
	{
		Prune();
		foreach (Area allArea in map.areaManager.AllAreas)
		{
			if (!areaToBits.TryGetValue(allArea, out var value))
			{
				value = new NativeBitArray(cellCount, Allocator.Persistent);
				areaToBits[allArea] = value;
			}
			BoolGrid innerGrid = allArea.InnerGrid;
			for (int i = 0; i < cellCount; i++)
			{
				value.Set(i, innerGrid[i]);
			}
		}
	}

	public bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> __)
	{
		Prune();
		bool result = false;
		CellIndices cellIndices = map.cellIndices;
		foreach (Area allArea in map.areaManager.AllAreas)
		{
			if (!areaToBits.TryGetValue(allArea, out var value))
			{
				value = new NativeBitArray(cellCount, Allocator.Persistent);
				areaToBits[allArea] = value;
				BoolGrid innerGrid = allArea.InnerGrid;
				for (int i = 0; i < cellCount; i++)
				{
					value.Set(i, innerGrid[i]);
				}
				result = true;
			}
			if (!areaToDeltas.TryGetValue(allArea, out var value2))
			{
				continue;
			}
			BoolGrid innerGrid2 = allArea.InnerGrid;
			foreach (IntVec3 item in value2)
			{
				int num = cellIndices.CellToIndex(item);
				value.Set(num, innerGrid2[num]);
				result = true;
			}
		}
		return result;
	}

	public void Notify_AreaDelta(Area area, IntVec3 cell)
	{
		if (!areaToDeltas.TryGetValue(area, out var value))
		{
			value = new List<IntVec3>();
			areaToDeltas[area] = value;
		}
		value.Add(cell);
	}

	private void Prune()
	{
		List<Area> allAreas = map.areaManager.AllAreas;
		foreach (Area key in areaToBits.Keys)
		{
			if (!allAreas.Contains(key))
			{
				tmpToPrune.Add(key);
			}
		}
		foreach (Area item in tmpToPrune)
		{
			areaToBits[item].Dispose();
			areaToBits.Remove(item);
			areaToDeltas.Remove(item);
		}
		tmpToPrune.Clear();
	}
}
