using System.Collections.Generic;

namespace Verse;

public class ConnectivitySource : SimplePathFinderDataSource<CellConnection>
{
	private static readonly HashSet<IntVec3> checkedCells = new HashSet<IntVec3>(160000);

	public ConnectivitySource(Map map)
		: base(map)
	{
	}

	public override void ComputeAll(IEnumerable<PathRequest> _)
	{
		CellIndices cellIndices = map.cellIndices;
		for (int i = 0; i < cellCount; i++)
		{
			data[i] = CellConnection.AllNeighbours;
		}
		int x = map.Size.x;
		int z = map.Size.z;
		for (int j = 0; j < z; j++)
		{
			IntVec3 c = new IntVec3(0, 0, j);
			IntVec3 c2 = new IntVec3(x - 1, 0, j);
			data[cellIndices.CellToIndex(c)] ^= CellConnection.WestNeighbours;
			data[cellIndices.CellToIndex(c2)] ^= CellConnection.EastNeighbours;
		}
		for (int k = 0; k < x; k++)
		{
			IntVec3 c3 = new IntVec3(k, 0, 0);
			IntVec3 c4 = new IntVec3(k, 0, z - 1);
			data[cellIndices.CellToIndex(c3)] ^= CellConnection.SouthNeighbours;
			data[cellIndices.CellToIndex(c4)] ^= CellConnection.NorthNeighbours;
		}
		IntVec3 c5 = new IntVec3(0, 0, 0);
		IntVec3 c6 = new IntVec3(x - 1, 0, 0);
		IntVec3 c7 = new IntVec3(0, 0, z - 1);
		IntVec3 c8 = new IntVec3(x - 1, 0, z - 1);
		data[cellIndices.CellToIndex(c5)] ^= CellConnection.SouthWest;
		data[cellIndices.CellToIndex(c6)] ^= CellConnection.SouthEast;
		data[cellIndices.CellToIndex(c7)] ^= CellConnection.NorthWest;
		data[cellIndices.CellToIndex(c8)] ^= CellConnection.NorthEast;
	}

	public override bool UpdateIncrementally(IEnumerable<PathRequest> _, List<IntVec3> cellDeltas)
	{
		CellIndices cellIndices = map.cellIndices;
		foreach (IntVec3 cellDelta in cellDeltas)
		{
			checkedCells.Clear();
			foreach (IntVec3 cell in CellRect.FromCell(cellDelta).ExpandedBy(1).ClipInsideMap(map)
				.Cells)
			{
				if (checkedCells.Add(cell))
				{
					data[cellIndices.CellToIndex(cell)] = ComputeCellConnectivity(cell);
				}
			}
		}
		return false;
	}

	private CellConnection ComputeCellConnectivity(IntVec3 cell)
	{
		CellConnection cellConnection = CellConnection.Self;
		foreach (CellConnection item in CellConnection.AllNeighbours)
		{
			IntVec3 c = cell + item.Offset();
			if (!c.InBounds(map))
			{
				continue;
			}
			Building building = map.edificeGrid.InnerArray[map.cellIndices.CellToIndex(c)];
			if (building != null && building.def.passability == Traversability.Impassable)
			{
				if (!building.def.destroyable)
				{
					continue;
				}
			}
			else if (!c.WalkableByAny(map))
			{
				continue;
			}
			cellConnection |= item;
		}
		return cellConnection;
	}
}
