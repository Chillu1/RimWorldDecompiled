using System.Collections.Generic;

namespace Verse
{
	public static class DijkstraUtility
	{
		private static List<IntVec3> adjacentCells = new List<IntVec3>();

		public static IEnumerable<IntVec3> AdjacentCellsNeighborsGetter(IntVec3 cell, Map map)
		{
			adjacentCells.Clear();
			IntVec3[] array = GenAdj.AdjacentCells;
			for (int i = 0; i < array.Length; i++)
			{
				IntVec3 intVec = cell + array[i];
				if (intVec.InBounds(map))
				{
					adjacentCells.Add(intVec);
				}
			}
			return adjacentCells;
		}
	}
}
