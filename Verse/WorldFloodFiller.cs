using RimWorld.Planet;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class WorldFloodFiller
	{
		private bool working;

		private Queue<int> openSet = new Queue<int>();

		private List<int> traversalDistance = new List<int>();

		private List<int> visited = new List<int>();

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<int> extraRootTiles = null)
		{
			FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile);
				return false;
			}, maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Action<int, int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<int> extraRootTiles = null)
		{
			FloodFill(rootTile, passCheck, delegate(int tile, int traversalDistance)
			{
				processor(tile, traversalDistance);
				return false;
			}, maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Predicate<int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<int> extraRootTiles = null)
		{
			FloodFill(rootTile, passCheck, (int tile, int traversalDistance) => processor(tile), maxTilesToProcess, extraRootTiles);
		}

		public void FloodFill(int rootTile, Predicate<int> passCheck, Func<int, int, bool> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<int> extraRootTiles = null)
		{
			if (working)
			{
				Log.Error("Nested FloodFill calls are not allowed. This will cause bugs.");
			}
			working = true;
			ClearVisited();
			if (rootTile != -1 && extraRootTiles == null && !passCheck(rootTile))
			{
				working = false;
				return;
			}
			int tilesCount = Find.WorldGrid.TilesCount;
			int num = tilesCount;
			if (traversalDistance.Count != tilesCount)
			{
				traversalDistance.Clear();
				for (int i = 0; i < tilesCount; i++)
				{
					traversalDistance.Add(-1);
				}
			}
			WorldGrid worldGrid = Find.WorldGrid;
			List<int> tileIDToNeighbors_offsets = worldGrid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = worldGrid.tileIDToNeighbors_values;
			int num2 = 0;
			openSet.Clear();
			if (rootTile != -1)
			{
				visited.Add(rootTile);
				traversalDistance[rootTile] = 0;
				openSet.Enqueue(rootTile);
			}
			if (extraRootTiles != null)
			{
				visited.AddRange(extraRootTiles);
				IList<int> list = extraRootTiles as IList<int>;
				if (list != null)
				{
					for (int j = 0; j < list.Count; j++)
					{
						int num3 = list[j];
						traversalDistance[num3] = 0;
						openSet.Enqueue(num3);
					}
				}
				else
				{
					foreach (int extraRootTile in extraRootTiles)
					{
						traversalDistance[extraRootTile] = 0;
						openSet.Enqueue(extraRootTile);
					}
				}
			}
			while (openSet.Count > 0)
			{
				int num4 = openSet.Dequeue();
				int num5 = traversalDistance[num4];
				if (processor(num4, num5))
				{
					break;
				}
				num2++;
				if (num2 == maxTilesToProcess)
				{
					break;
				}
				int num6 = (num4 + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[num4 + 1] : tileIDToNeighbors_values.Count;
				for (int k = tileIDToNeighbors_offsets[num4]; k < num6; k++)
				{
					int num7 = tileIDToNeighbors_values[k];
					if (traversalDistance[num7] == -1 && passCheck(num7))
					{
						visited.Add(num7);
						openSet.Enqueue(num7);
						traversalDistance[num7] = num5 + 1;
					}
				}
				if (openSet.Count > num)
				{
					Log.Error("Overflow on world flood fill (>" + num + " cells). Make sure we're not flooding over the same area after we check it.");
					working = false;
					return;
				}
			}
			working = false;
		}

		private void ClearVisited()
		{
			int i = 0;
			for (int count = visited.Count; i < count; i++)
			{
				traversalDistance[visited[i]] = -1;
			}
			visited.Clear();
			openSet.Clear();
		}
	}
}
