using System;
using System.Collections.Generic;

namespace Verse
{
	public static class GenMorphology
	{
		private static HashSet<IntVec3> tmpOutput = new HashSet<IntVec3>();

		private static HashSet<IntVec3> cellsSet = new HashSet<IntVec3>();

		private static List<IntVec3> tmpEdgeCells = new List<IntVec3>();

		public static void Erode(List<IntVec3> cells, int count, Map map, Predicate<IntVec3> extraPredicate = null)
		{
			if (count <= 0)
			{
				return;
			}
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			cellsSet.Clear();
			cellsSet.AddRange(cells);
			tmpEdgeCells.Clear();
			for (int i = 0; i < cells.Count; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					IntVec3 item = cells[i] + cardinalDirections[j];
					if (!cellsSet.Contains(item))
					{
						tmpEdgeCells.Add(cells[i]);
						break;
					}
				}
			}
			if (!tmpEdgeCells.Any())
			{
				return;
			}
			tmpOutput.Clear();
			Predicate<IntVec3> passCheck = (extraPredicate == null) ? ((Predicate<IntVec3>)((IntVec3 x) => cellsSet.Contains(x))) : ((Predicate<IntVec3>)((IntVec3 x) => cellsSet.Contains(x) && extraPredicate(x)));
			map.floodFiller.FloodFill(IntVec3.Invalid, passCheck, delegate(IntVec3 cell, int traversalDist)
			{
				if (traversalDist >= count)
				{
					tmpOutput.Add(cell);
				}
				return false;
			}, int.MaxValue, rememberParents: false, tmpEdgeCells);
			cells.Clear();
			cells.AddRange(tmpOutput);
		}

		public static void Dilate(List<IntVec3> cells, int count, Map map, Predicate<IntVec3> extraPredicate = null)
		{
			if (count <= 0)
			{
				return;
			}
			map.floodFiller.FloodFill(IntVec3.Invalid, extraPredicate ?? ((Predicate<IntVec3>)((IntVec3 x) => true)), delegate(IntVec3 cell, int traversalDist)
			{
				if (traversalDist > count)
				{
					return true;
				}
				if (traversalDist != 0)
				{
					cells.Add(cell);
				}
				return false;
			}, int.MaxValue, rememberParents: false, cells);
		}

		public static void Open(List<IntVec3> cells, int count, Map map)
		{
			Erode(cells, count, map);
			Dilate(cells, count, map);
		}

		public static void Close(List<IntVec3> cells, int count, Map map)
		{
			Dilate(cells, count, map);
			Erode(cells, count, map);
		}
	}
}
