namespace Verse
{
	public class CellIndices
	{
		private int mapSizeX;

		private int mapSizeZ;

		public int NumGridCells => mapSizeX * mapSizeZ;

		public CellIndices(Map map)
		{
			mapSizeX = map.Size.x;
			mapSizeZ = map.Size.z;
		}

		public int CellToIndex(IntVec3 c)
		{
			return CellIndicesUtility.CellToIndex(c, mapSizeX);
		}

		public int CellToIndex(int x, int z)
		{
			return CellIndicesUtility.CellToIndex(x, z, mapSizeX);
		}

		public IntVec3 IndexToCell(int ind)
		{
			return CellIndicesUtility.IndexToCell(ind, mapSizeX);
		}
	}
}
