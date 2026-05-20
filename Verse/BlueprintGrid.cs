using System.Collections.Generic;
using RimWorld;

namespace Verse;

public sealed class BlueprintGrid
{
	private readonly Map map;

	private readonly List<Blueprint>[] innerArray;

	public List<Blueprint> this[int index] => innerArray[index];

	public List<Blueprint> this[IntVec3 c] => innerArray[map.cellIndices.CellToIndex(c)];

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
				List<Blueprint>[] array = innerArray;
				int num2 = num;
				if (array[num2] == null)
				{
					array[num2] = new List<Blueprint>();
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
