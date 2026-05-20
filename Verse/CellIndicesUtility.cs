using System.Runtime.CompilerServices;

namespace Verse;

public static class CellIndicesUtility
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CellToIndex(IntVec3 c, int sizeX)
	{
		return c.z * sizeX + c.x;
	}

	public static int CellToIndex(int x, int z, int sizeX)
	{
		return z * sizeX + x;
	}

	public static IntVec3 IndexToCell(int ind, int sizeX)
	{
		int newX = ind % sizeX;
		int newZ = ind / sizeX;
		return new IntVec3(newX, 0, newZ);
	}
}
