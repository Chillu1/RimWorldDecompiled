using System.Collections.Generic;

namespace Verse;

public class WaterSource : SimpleBoolPathFinderDataSource
{
	public WaterSource(Map map)
		: base(map)
	{
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.Clear();
		TerrainGrid terrainGrid = map.terrainGrid;
		for (int i = 0; i < cellCount; i++)
		{
			TerrainDef terrainDef = terrainGrid.TerrainAt(i);
			if (terrainDef != null && terrainDef.IsWater)
			{
				data.Set(i, value: true);
			}
		}
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		TerrainGrid terrainGrid = map.terrainGrid;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int num = cellIndices.CellToIndex(cellDelta);
			data.Set(num, terrainGrid.TerrainAt(num)?.IsWater ?? false);
		}
		return false;
	}
}
