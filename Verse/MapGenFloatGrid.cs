namespace Verse
{
	public class MapGenFloatGrid
	{
		private Map map;

		private float[] grid;

		public float this[IntVec3 c]
		{
			get
			{
				return grid[map.cellIndices.CellToIndex(c)];
			}
			set
			{
				grid[map.cellIndices.CellToIndex(c)] = value;
			}
		}

		public MapGenFloatGrid(Map map)
		{
			this.map = map;
			grid = new float[map.cellIndices.NumGridCells];
		}
	}
}
