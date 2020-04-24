namespace Verse
{
	public static class CellIndicesUtility
	{
		public static int CellToIndex(IntVec3 c, int mapSizeX)
		{
			return c.z * mapSizeX + c.x;
		}

		public static int CellToIndex(int x, int z, int mapSizeX)
		{
			return z * mapSizeX + x;
		}

		public static IntVec3 IndexToCell(int ind, int mapSizeX)
		{
			int newX = ind % mapSizeX;
			int newZ = ind / mapSizeX;
			return new IntVec3(newX, 0, newZ);
		}
	}
}
