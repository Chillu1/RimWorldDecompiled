using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public sealed class BlueprintGrid
	{
		private Map map;

		private List<Blueprint>[] innerArray;

		public List<Blueprint>[] InnerArray => innerArray;

		public BlueprintGrid(Map map)
		{
			this.map = map;
			innerArray = new List<Blueprint>[map.cellIndices.NumGridCells];
		}

		public void Register(Blueprint ed)
		{
			CellIndices cellIndices = map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					int num = cellIndices.CellToIndex(j, i);
					if (innerArray[num] == null)
					{
						innerArray[num] = new List<Blueprint>();
					}
					innerArray[num].Add(ed);
				}
			}
		}

		public void DeRegister(Blueprint ed)
		{
			CellIndices cellIndices = map.cellIndices;
			CellRect cellRect = ed.OccupiedRect();
			for (int i = cellRect.minZ; i <= cellRect.maxZ; i++)
			{
				for (int j = cellRect.minX; j <= cellRect.maxX; j++)
				{
					int num = cellIndices.CellToIndex(j, i);
					innerArray[num].Remove(ed);
					if (innerArray[num].Count == 0)
					{
						innerArray[num] = null;
					}
				}
			}
		}
	}
}
