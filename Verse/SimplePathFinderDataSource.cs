using System;
using System.Collections.Generic;
using Unity.Collections;

namespace Verse;

public abstract class SimplePathFinderDataSource<T> : IPathFinderDataSource, IDisposable where T : struct
{
	protected readonly Map map;

	protected readonly int cellCount;

	protected NativeArray<T> data;

	public NativeArray<T>.ReadOnly Data => data.AsReadOnly();

	public SimplePathFinderDataSource(Map map)
	{
		this.map = map;
		cellCount = map.cellIndices.NumGridCells;
		data = new NativeArray<T>(cellCount, Allocator.Persistent);
	}

	public virtual void Dispose()
	{
		data.Dispose();
	}

	public abstract void ComputeAll(IEnumerable<PathRequest> requests);

	public abstract bool UpdateIncrementally(IEnumerable<PathRequest> requests, List<IntVec3> cellDeltas);
}
