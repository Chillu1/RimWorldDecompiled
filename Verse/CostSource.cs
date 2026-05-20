using System.Collections.Generic;
using Unity.Collections;
using Verse.AI;

namespace Verse;

public class CostSource : SimplePathFinderDataSource<int>
{
	protected readonly PathingContext context;

	public CostSource(Map map, PathingContext context)
		: base(map)
	{
		this.context = context;
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.CopyFrom(context.pathGrid.Grid_Unsafe);
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		NativeArray<int> grid_Unsafe = context.pathGrid.Grid_Unsafe;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int index = cellIndices.CellToIndex(cellDelta);
			data[index] = grid_Unsafe[index];
		}
		return false;
	}
}
