namespace Verse
{
	public sealed class EdificeGrid
	{
		private Map map;

		private Building[] innerArray;

		public Building[] InnerArray => innerArray;

		public Building this[int index] => innerArray[index];

		public Building this[IntVec3 c] => innerArray[map.cellIndices.CellToIndex(c)];

		public EdificeGrid(Map map)
		{
			this.map = map;
			innerArray = new Building[map.cellIndices.NumGridCells];
		}

		public void Register(Building ed)
		{
			CellIndices cellIndices = map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					IntVec3 c = new IntVec3(j, 0, i);
					innerArray[cellIndices.CellToIndex(c)] = ed;
				}
			}
		}

		public void DeRegister(Building ed)
		{
			CellIndices cellIndices = map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					innerArray[cellIndices.CellToIndex(j, i)] = null;
				}
			}
		}
	}
}
