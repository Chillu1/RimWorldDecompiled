using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Verse;

public abstract class SimpleBoolPathFinderDataSource : IPathFinderDataSource, IDisposable
{
	protected readonly Map map;

	protected readonly int cellCount;

	protected NativeBitArray data;

	public NativeBitArray.ReadOnly Data => data.AsReadOnly();

	public SimpleBoolPathFinderDataSource(Map map)
	{
		this.map = map;
		cellCount = map.cellIndices.NumGridCells;
		data = new NativeBitArray(cellCount, Allocator.Persistent);
	}

	public virtual void Dispose()
	{
		data.Dispose();
	}

	public abstract void ComputeAll(IEnumerable<PathRequest> requests);

	public abstract bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas);
}
