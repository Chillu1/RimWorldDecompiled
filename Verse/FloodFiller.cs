using System;
using System.Collections.Generic;

namespace Verse
{
	public class FloodFiller
	{
		private Map map;

		private bool working;

		private Queue<IntVec3> openSet = new Queue<IntVec3>();

		private IntGrid traversalDistance;

		private CellGrid parentGrid;

		private List<int> visited = new List<int>();

		public FloodFiller(Map map)
		{
			this.map = map;
			traversalDistance = new IntGrid(map);
			traversalDistance.Clear(-1);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Action<IntVec3> processor, int maxCellsToProcess = int.MaxValue, bool rememberParents = false, IEnumerable<IntVec3> extraRoots = null)
		{
			FloodFill(root, passCheck, delegate(IntVec3 cell, int traversalDist)
			{
				processor(cell);
				return false;
			}, maxCellsToProcess, rememberParents, extraRoots);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Action<IntVec3, int> processor, int maxCellsToProcess = int.MaxValue, bool rememberParents = false, IEnumerable<IntVec3> extraRoots = null)
		{
			FloodFill(root, passCheck, delegate(IntVec3 cell, int traversalDist)
			{
				processor(cell, traversalDist);
				return false;
			}, maxCellsToProcess, rememberParents, extraRoots);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Func<IntVec3, bool> processor, int maxCellsToProcess = int.MaxValue, bool rememberParents = false, IEnumerable<IntVec3> extraRoots = null)
		{
			FloodFill(root, passCheck, (IntVec3 cell, int traversalDist) => processor(cell), maxCellsToProcess, rememberParents, extraRoots);
		}

		public void FloodFill(IntVec3 root, Predicate<IntVec3> passCheck, Func<IntVec3, int, bool> processor, int maxCellsToProcess = int.MaxValue, bool rememberParents = false, IEnumerable<IntVec3> extraRoots = null)
		{
			if (working)
			{
				Log.Error("Nested FloodFill calls are not allowed. This will cause bugs.");
			}
			working = true;
			ClearVisited();
			if (rememberParents && parentGrid == null)
			{
				parentGrid = new CellGrid(map);
			}
			if (root.IsValid && extraRoots == null && !passCheck(root))
			{
				if (rememberParents)
				{
					parentGrid[root] = IntVec3.Invalid;
				}
				working = false;
				return;
			}
			int area = map.Area;
			IntVec3[] cardinalDirectionsAround = GenAdj.CardinalDirectionsAround;
			int num = cardinalDirectionsAround.Length;
			CellIndices cellIndices = map.cellIndices;
			int num2 = 0;
			openSet.Clear();
			if (root.IsValid)
			{
				int num3 = cellIndices.CellToIndex(root);
				visited.Add(num3);
				traversalDistance[num3] = 0;
				openSet.Enqueue(root);
			}
			if (extraRoots != null)
			{
				IList<IntVec3> list = extraRoots as IList<IntVec3>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						int num4 = cellIndices.CellToIndex(list[i]);
						visited.Add(num4);
						traversalDistance[num4] = 0;
						openSet.Enqueue(list[i]);
					}
				}
				else
				{
					foreach (IntVec3 extraRoot in extraRoots)
					{
						int num5 = cellIndices.CellToIndex(extraRoot);
						visited.Add(num5);
						traversalDistance[num5] = 0;
						openSet.Enqueue(extraRoot);
					}
				}
			}
			if (rememberParents)
			{
				for (int j = 0; j < visited.Count; j++)
				{
					IntVec3 intVec = cellIndices.IndexToCell(visited[j]);
					parentGrid[visited[j]] = (passCheck(intVec) ? intVec : IntVec3.Invalid);
				}
			}
			while (openSet.Count > 0)
			{
				IntVec3 intVec2 = openSet.Dequeue();
				int num6 = traversalDistance[cellIndices.CellToIndex(intVec2)];
				if (processor(intVec2, num6))
				{
					break;
				}
				num2++;
				if (num2 == maxCellsToProcess)
				{
					break;
				}
				for (int k = 0; k < num; k++)
				{
					IntVec3 intVec3 = intVec2 + cardinalDirectionsAround[k];
					int num7 = cellIndices.CellToIndex(intVec3);
					if (intVec3.InBounds(map) && traversalDistance[num7] == -1 && passCheck(intVec3))
					{
						visited.Add(num7);
						openSet.Enqueue(intVec3);
						traversalDistance[num7] = num6 + 1;
						if (rememberParents)
						{
							parentGrid[num7] = intVec2;
						}
					}
				}
				if (openSet.Count > area)
				{
					Log.Error("Overflow on flood fill (>" + area + " cells). Make sure we're not flooding over the same area after we check it.");
					working = false;
					return;
				}
			}
			working = false;
		}

		public void ReconstructLastFloodFillPath(IntVec3 dest, List<IntVec3> outPath)
		{
			outPath.Clear();
			if (parentGrid == null || !dest.InBounds(map) || !parentGrid[dest].IsValid)
			{
				return;
			}
			int num = 0;
			int num2 = map.Area + 1;
			IntVec3 intVec = dest;
			while (true)
			{
				num++;
				if (num > num2)
				{
					Log.Error("Too many iterations.");
					break;
				}
				if (!intVec.IsValid)
				{
					break;
				}
				outPath.Add(intVec);
				if (parentGrid[intVec] == intVec)
				{
					break;
				}
				intVec = parentGrid[intVec];
			}
			outPath.Reverse();
		}

		private void ClearVisited()
		{
			int i = 0;
			for (int count = visited.Count; i < count; i++)
			{
				int index = visited[i];
				traversalDistance[index] = -1;
				if (parentGrid != null)
				{
					parentGrid[index] = IntVec3.Invalid;
				}
			}
			visited.Clear();
			openSet.Clear();
		}
	}
}
