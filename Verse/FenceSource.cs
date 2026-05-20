using System.Collections.Generic;

namespace Verse;

public class FenceSource : SimpleBoolPathFinderDataSource
{
	public FenceSource(Map map)
		: base(map)
	{
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		data.Clear();
		CellIndices cellIndices = map.cellIndices;
		foreach (Thing item in (IEnumerable<Thing>)map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial))
		{
			if (item.def.IsFence)
			{
				int pos = cellIndices.CellToIndex(item.Position);
				data.Set(pos, value: true);
			}
		}
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		Building[] innerArray = map.edificeGrid.InnerArray;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			int num = cellIndices.CellToIndex(cellDelta);
			data.Set(num, innerArray[num]?.def.IsFence ?? false);
		}
		return false;
	}
}
