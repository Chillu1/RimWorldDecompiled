using System.Runtime.CompilerServices;

namespace Verse;

public readonly struct CellIndices
{
	private readonly int sizeX;

	private readonly int sizeZ;

	public int NumGridCells => sizeX * sizeZ;

	public int SizeX => sizeX;

	public int SizeZ => sizeZ;

	public int this[IntVec3 cell] => CellToIndex(cell);

	public IntVec3 this[int index] => IndexToCell(index);

	public CellIndices(Map map)
	{
		sizeX = map.Size.x;
		sizeZ = map.Size.z;
	}

	public CellIndices(int sizeX, int sizeZ)
	{
		this.sizeX = sizeX;
		this.sizeZ = sizeZ;
	}

	public bool Contains(IntVec3 c)
	{
		return Contains(CellIndicesUtility.CellToIndex(c, sizeX));
	}

	public bool Contains(int ind)
	{
		if (ind >= 0)
		{
			return ind < sizeX * sizeZ;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CellToIndex(IntVec3 c)
	{
		return CellIndicesUtility.CellToIndex(c, sizeX);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CellToIndex(int x, int z)
	{
		return CellIndicesUtility.CellToIndex(x, z, sizeX);
	}

	public IntVec3 IndexToCell(int ind)
	{
		return CellIndicesUtility.IndexToCell(ind, sizeX);
	}
}
