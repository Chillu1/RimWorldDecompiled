using System;
using Unity.Collections;
using Verse;

namespace RimWorld;

public class UsedRectPathGridCustomizer : PathRequest.IPathGridCustomizer, IDisposable
{
	private NativeArray<ushort> grid;

	public UsedRectPathGridCustomizer(Map map)
	{
		grid = new NativeArray<ushort>(map.cellIndices.NumGridCells, Allocator.Persistent);
		foreach (CellRect usedRect in MapGenerator.UsedRects)
		{
			foreach (IntVec3 cell in usedRect.ExpandedBy(2).Cells)
			{
				if (cell.InBounds(map))
				{
					grid[map.cellIndices.CellToIndex(cell)] = 10000;
				}
			}
		}
	}

	public NativeArray<ushort> GetOffsetGrid()
	{
		return grid;
	}

	public void Dispose()
	{
		grid.Dispose();
	}
}
