using System.Collections.Generic;
using LudeonTK;
using Unity.Collections;

namespace Verse;

public class FogSource : SimpleBoolPathFinderDataSource
{
	public FogSource(Map map)
		: base(map)
	{
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.CopyFrom(map.fogGrid.FogGrid_Unsafe);
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		NativeBitArray fogGrid_Unsafe = map.fogGrid.FogGrid_Unsafe;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int pos = cellIndices.CellToIndex(cellDelta);
			data.Set(pos, fogGrid_Unsafe.IsSet(pos));
		}
		return false;
	}
}
