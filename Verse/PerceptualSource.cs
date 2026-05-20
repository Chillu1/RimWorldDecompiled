using System;
using System.Collections.Generic;
using LudeonTK;
using Unity.Collections;

namespace Verse;

public class PerceptualSource : IPathFinderDataSource, IDisposable
{
	private readonly Map map;

	private NativeArray<ushort> costDrafted;

	private NativeArray<ushort> costUndrafted;

	public NativeArray<ushort>.ReadOnly CostDrafted => costDrafted.AsReadOnly();

	public NativeArray<ushort>.ReadOnly CostUndrafted => costUndrafted.AsReadOnly();

	public PerceptualSource(Map map)
	{
		this.map = map;
		int numGridCells = map.cellIndices.NumGridCells;
		costDrafted = new NativeArray<ushort>(numGridCells, Allocator.Persistent);
		costUndrafted = new NativeArray<ushort>(numGridCells, Allocator.Persistent);
	}

	public void Dispose()
	{
		costDrafted.Dispose();
		costUndrafted.Dispose();
	}

	public void ComputeAll(IEnumerable<PathRequest> _)
	{
		costDrafted.Clear();
		costUndrafted.Clear();
		TerrainGrid terrainGrid = map.terrainGrid;
		for (int i = 0; i < map.cellIndices.NumGridCells; i++)
		{
			TerrainDef terrainDef = terrainGrid.TerrainAt(i);
			if (terrainDef != null)
			{
				costUndrafted[i] = (ushort)Math.Clamp(terrainDef.extraNonDraftedPerceivedPathCost, 0, 65535);
				costDrafted[i] = (ushort)Math.Clamp(terrainDef.extraDraftedPerceivedPathCost, 0, 65535);
			}
		}
	}

	public bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		TerrainGrid terrainGrid = map.terrainGrid;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int num = cellIndices.CellToIndex(cellDelta);
			TerrainDef terrainDef = terrainGrid.TerrainAt(num);
			if (terrainDef != null)
			{
				costUndrafted[num] = (ushort)Math.Clamp(terrainDef.extraNonDraftedPerceivedPathCost, 0, 65535);
				costDrafted[num] = (ushort)Math.Clamp(terrainDef.extraDraftedPerceivedPathCost, 0, 65535);
			}
			else
			{
				costUndrafted[num] = 0;
				costDrafted[num] = 0;
			}
		}
		return false;
	}
}
