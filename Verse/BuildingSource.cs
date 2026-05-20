using System;
using System.Collections.Generic;
using LudeonTK;
using Unity.Collections;

namespace Verse;

public class BuildingSource : IPathFinderDataSource, IDisposable
{
	private readonly Map map;

	private readonly int cellCount;

	private NativeBitArray buildings;

	private NativeBitArray destroyable;

	private NativeBitArray player;

	private NativeArray<ushort> hitpoints;

	public NativeBitArray.ReadOnly Buildings => buildings.AsReadOnly();

	public NativeBitArray.ReadOnly Destroyable => destroyable.AsReadOnly();

	public NativeBitArray.ReadOnly Player => player.AsReadOnly();

	public NativeArray<ushort>.ReadOnly Hitpoints => hitpoints.AsReadOnly();

	public BuildingSource(Map map)
	{
		this.map = map;
		cellCount = map.cellIndices.NumGridCells;
		buildings = new NativeBitArray(cellCount, Allocator.Persistent);
		destroyable = new NativeBitArray(cellCount, Allocator.Persistent);
		player = new NativeBitArray(cellCount, Allocator.Persistent);
		hitpoints = new NativeArray<ushort>(cellCount, Allocator.Persistent);
	}

	public void Dispose()
	{
		buildings.Dispose();
		destroyable.Dispose();
		player.Dispose();
		hitpoints.Dispose();
	}

	public void ComputeAll(IEnumerable<PathRequest> _)
	{
		buildings.Clear();
		destroyable.Clear();
		player.Clear();
		hitpoints.Clear();
		Building[] innerArray = map.edificeGrid.InnerArray;
		for (int i = 0; i < cellCount; i++)
		{
			if (innerArray[i] != null)
			{
				SetBuildingData(i);
			}
		}
	}

	public bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		Building[] innerArray = map.edificeGrid.InnerArray;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int num = cellIndices.CellToIndex(cellDelta);
			if (innerArray[num] != null)
			{
				SetBuildingData(num);
				continue;
			}
			buildings.Set(num, value: false);
			destroyable.Set(num, value: false);
			player.Set(num, value: false);
			hitpoints[num] = 0;
		}
		return false;
	}

	private void SetBuildingData(int index)
	{
		Building building = map.edificeGrid[index];
		if (building != null)
		{
			if (building.def.Fillage == FillCategory.Full)
			{
				buildings.Set(index, value: true);
				destroyable.Set(index, building.def.destroyable);
				hitpoints[index] = (ushort)Math.Clamp(building.HitPoints, 0, 65535);
			}
			else
			{
				buildings.Set(index, value: false);
				destroyable.Set(index, value: false);
				hitpoints[index] = 0;
			}
			player.Set(index, building.Faction?.IsPlayer ?? false);
		}
	}
}
