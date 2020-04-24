namespace Verse
{
	public class CellGrid
	{
		private int[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public IntVec3 this[IntVec3 c]
		{
			get
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				return CellIndicesUtility.IndexToCell(grid[num], mapSizeX);
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				grid[num] = CellIndicesUtility.CellToIndex(value, mapSizeX);
			}
		}

		public IntVec3 this[int index]
		{
			get
			{
				return CellIndicesUtility.IndexToCell(grid[index], mapSizeX);
			}
			set
			{
				grid[index] = CellIndicesUtility.CellToIndex(value, mapSizeX);
			}
		}

		public IntVec3 this[int x, int z]
		{
			get
			{
				int num = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
				return CellIndicesUtility.IndexToCell(grid[num], mapSizeX);
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
				grid[num] = CellIndicesUtility.CellToIndex(x, z, mapSizeX);
			}
		}

		public int CellsCount => grid.Length;

		public CellGrid()
		{
		}

		public CellGrid(Map map)
		{
			ClearAndResizeTo(map);
		}

		public bool MapSizeMatches(Map map)
		{
			if (mapSizeX == map.Size.x)
			{
				return mapSizeZ == map.Size.z;
			}
			return false;
		}

		public void ClearAndResizeTo(Map map)
		{
			if (MapSizeMatches(map) && grid != null)
			{
				Clear();
				return;
			}
			mapSizeX = map.Size.x;
			mapSizeZ = map.Size.z;
			grid = new int[mapSizeX * mapSizeZ];
			Clear();
		}

		public void Clear()
		{
			int num = CellIndicesUtility.CellToIndex(IntVec3.Invalid, mapSizeX);
			for (int i = 0; i < grid.Length; i++)
			{
				grid[i] = num;
			}
		}
	}
}
