using System;
using LudeonTK;
using Unity.Collections;

namespace Verse;

public class MapGenFloatGrid : IDisposable
{
	private readonly Map map;

	private NativeArray<float> grid;

	public float this[IntVec3 c]
	{
		get
		{
			return grid[map.cellIndices.CellToIndex(c)];
		}
		set
		{
			grid[map.cellIndices.CellToIndex(c)] = value;
		}
	}

	internal ref NativeArray<float> Grid_Unsafe => ref grid;

	public MapGenFloatGrid(Map map)
	{
		this.map = map;
		grid = new NativeArray<float>(map.cellIndices.NumGridCells, Allocator.Persistent);
	}

	public void Clear()
	{
		NativeArrayUtility.MemClear(grid);
	}

	public void Dispose()
	{
		grid.Dispose();
	}
}
