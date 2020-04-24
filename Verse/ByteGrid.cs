using System;

namespace Verse
{
	public sealed class ByteGrid : IExposable
	{
		private byte[] grid;

		private int mapSizeX;

		private int mapSizeZ;

		public byte this[IntVec3 c]
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

		public byte this[int index]
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

		public byte this[int x, int z]
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

		public ByteGrid()
		{
		}

		public ByteGrid(Map map)
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
				Clear(0);
				return;
			}
			mapSizeX = map.Size.x;
			mapSizeZ = map.Size.z;
			grid = new byte[mapSizeX * mapSizeZ];
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref mapSizeX, "mapSizeX", 0);
			Scribe_Values.Look(ref mapSizeZ, "mapSizeZ", 0);
			DataExposeUtility.ByteArray(ref grid, "grid");
		}

		public void Clear(byte value = 0)
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
				byte b = grid[i];
				if (b > 0)
				{
					CellRenderer.RenderCell(CellIndicesUtility.IndexToCell(i, mapSizeX), (float)(int)b / 255f * 0.5f);
				}
			}
		}
	}
}
