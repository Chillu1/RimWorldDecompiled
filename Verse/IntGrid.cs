using System;

namespace Verse
{
	public sealed class IntGrid
	{
		private int[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public int this[IntVec3 c]
		{
			get
			{
				return grid[CellIndicesUtility.CellToIndex(c, mapSizeX)];
			}
			set
			{
				int num = CellIndicesUtility.CellToIndex(c, mapSizeX);
				grid[num] = value;
			}
		}

		public int this[int index]
		{
			get
			{
				return grid[index];
			}
			set
			{
				grid[index] = value;
			}
		}

		public int this[int x, int z]
		{
			get
			{
				return grid[CellIndicesUtility.CellToIndex(x, z, mapSizeX)];
			}
			set
			{
				grid[CellIndicesUtility.CellToIndex(x, z, mapSizeX)] = value;
			}
		}

		public int CellsCount => grid.Length;

		public IntGrid()
		{
		}

		public IntGrid(Map map)
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
		}

		public void Clear(int value = 0)
		{
			if (value == 0)
			{
				Array.Clear(grid, 0, grid.Length);
				return;
			}
			for (int i = 0; i < grid.Length; i++)
			{
				grid[i] = value;
			}
		}

		public void DebugDraw()
		{
			for (int i = 0; i < grid.Length; i++)
			{
				int num = grid[i];
				if (num > 0)
				{
					CellRenderer.RenderCell(CellIndicesUtility.IndexToCell(i, mapSizeX), (float)(num % 100) / 100f * 0.5f);
				}
			}
		}
	}
}
