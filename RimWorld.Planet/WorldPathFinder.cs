using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPathFinder
	{
		private struct CostNode
		{
			public int tile;

			public int cost;

			public CostNode(int tile, int cost)
			{
				this.tile = tile;
				this.cost = cost;
			}
		}

		private struct PathFinderNodeFast
		{
			public int knownCost;

			public int heuristicCost;

			public int parentTile;

			public int costNodeCost;

			public ushort status;
		}

		private class CostNodeComparer : IComparer<CostNode>
		{
			public int Compare(CostNode a, CostNode b)
			{
				int cost = a.cost;
				int cost2 = b.cost;
				if (cost > cost2)
				{
					return 1;
				}
				if (cost < cost2)
				{
					return -1;
				}
				return 0;
			}
		}

		private FastPriorityQueue<CostNode> openList;

		private PathFinderNodeFast[] calcGrid;

		private ushort statusOpenValue = 1;

		private ushort statusClosedValue = 2;

		private const int SearchLimit = 500000;

		private static readonly SimpleCurve HeuristicStrength_DistanceCurve = new SimpleCurve
		{
			new CurvePoint(30f, 1f),
			new CurvePoint(40f, 1.3f),
			new CurvePoint(130f, 2f)
		};

		private const float BestRoadDiscount = 0.5f;

		public WorldPathFinder()
		{
			calcGrid = new PathFinderNodeFast[Find.WorldGrid.TilesCount];
			openList = new FastPriorityQueue<CostNode>(new CostNodeComparer());
		}

		public WorldPath FindPath(int startTile, int destTile, Caravan caravan, Func<float, bool> terminator = null)
		{
			if (startTile < 0)
			{
				Log.Error("Tried to FindPath with invalid start tile " + startTile + ", caravan= " + caravan);
				return WorldPath.NotFound;
			}
			if (destTile < 0)
			{
				Log.Error("Tried to FindPath with invalid dest tile " + destTile + ", caravan= " + caravan);
				return WorldPath.NotFound;
			}
			if (caravan != null)
			{
				if (!caravan.CanReach(destTile))
				{
					return WorldPath.NotFound;
				}
			}
			else if (!Find.WorldReachability.CanReach(startTile, destTile))
			{
				return WorldPath.NotFound;
			}
			int num = startTile;
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			Vector3 normalized = grid.GetTileCenter(destTile).normalized;
			float[] movementDifficulty = world.pathGrid.movementDifficulty;
			int num2 = 0;
			int num3 = caravan?.TicksPerMove ?? 3300;
			int num4 = CalculateHeuristicStrength(startTile, destTile);
			statusOpenValue += 2;
			statusClosedValue += 2;
			if (statusClosedValue >= 65435)
			{
				ResetStatuses();
			}
			calcGrid[num].knownCost = 0;
			calcGrid[num].heuristicCost = 0;
			calcGrid[num].costNodeCost = 0;
			calcGrid[num].parentTile = startTile;
			calcGrid[num].status = statusOpenValue;
			openList.Clear();
			openList.Push(new CostNode(num, 0));
			while (true)
			{
				if (openList.Count <= 0)
				{
					Log.Warning(string.Concat(caravan, " pathing from ", startTile, " to ", destTile, " ran out of tiles to process."));
					return WorldPath.NotFound;
				}
				CostNode costNode = openList.Pop();
				if (costNode.cost != calcGrid[costNode.tile].costNodeCost)
				{
					continue;
				}
				num = costNode.tile;
				if (calcGrid[num].status == statusClosedValue)
				{
					continue;
				}
				if (num == destTile)
				{
					return FinalizedPath(num);
				}
				if (num2 > 500000)
				{
					Log.Warning(string.Concat(caravan, " pathing from ", startTile, " to ", destTile, " hit search limit of ", 500000, " tiles."));
					return WorldPath.NotFound;
				}
				int num5 = ((num + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[num + 1] : tileIDToNeighbors_values.Count);
				for (int i = tileIDToNeighbors_offsets[num]; i < num5; i++)
				{
					int num6 = tileIDToNeighbors_values[i];
					if (calcGrid[num6].status == statusClosedValue || world.Impassable(num6))
					{
						continue;
					}
					int num7 = (int)((float)num3 * movementDifficulty[num6] * grid.GetRoadMovementDifficultyMultiplier(num, num6)) + calcGrid[num].knownCost;
					ushort status = calcGrid[num6].status;
					if ((status != statusClosedValue && status != statusOpenValue) || calcGrid[num6].knownCost > num7)
					{
						Vector3 tileCenter = grid.GetTileCenter(num6);
						if (status != statusClosedValue && status != statusOpenValue)
						{
							float num8 = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, normalized));
							calcGrid[num6].heuristicCost = Mathf.RoundToInt((float)num3 * num8 * (float)num4 * 0.5f);
						}
						int num9 = num7 + calcGrid[num6].heuristicCost;
						calcGrid[num6].parentTile = num;
						calcGrid[num6].knownCost = num7;
						calcGrid[num6].status = statusOpenValue;
						calcGrid[num6].costNodeCost = num9;
						openList.Push(new CostNode(num6, num9));
					}
				}
				num2++;
				calcGrid[num].status = statusClosedValue;
				if (terminator != null && terminator(calcGrid[num].costNodeCost))
				{
					break;
				}
			}
			return WorldPath.NotFound;
		}

		public void FloodPathsWithCost(List<int> startTiles, Func<int, int, int> costFunc, Func<int, bool> impassable = null, Func<int, float, bool> terminator = null)
		{
			if (startTiles.Count < 1 || startTiles.Contains(-1))
			{
				Log.Error("Tried to FindPath with invalid start tiles");
				return;
			}
			World world = Find.World;
			WorldGrid grid = world.grid;
			List<int> tileIDToNeighbors_offsets = grid.tileIDToNeighbors_offsets;
			List<int> tileIDToNeighbors_values = grid.tileIDToNeighbors_values;
			if (impassable == null)
			{
				impassable = (int tid) => world.Impassable(tid);
			}
			statusOpenValue += 2;
			statusClosedValue += 2;
			if (statusClosedValue >= 65435)
			{
				ResetStatuses();
			}
			openList.Clear();
			foreach (int startTile in startTiles)
			{
				calcGrid[startTile].knownCost = 0;
				calcGrid[startTile].costNodeCost = 0;
				calcGrid[startTile].parentTile = startTile;
				calcGrid[startTile].status = statusOpenValue;
				openList.Push(new CostNode(startTile, 0));
			}
			while (openList.Count > 0)
			{
				CostNode costNode = openList.Pop();
				if (costNode.cost != calcGrid[costNode.tile].costNodeCost)
				{
					continue;
				}
				int tile = costNode.tile;
				if (calcGrid[tile].status == statusClosedValue)
				{
					continue;
				}
				int num = ((tile + 1 < tileIDToNeighbors_offsets.Count) ? tileIDToNeighbors_offsets[tile + 1] : tileIDToNeighbors_values.Count);
				for (int i = tileIDToNeighbors_offsets[tile]; i < num; i++)
				{
					int num2 = tileIDToNeighbors_values[i];
					if (calcGrid[num2].status != statusClosedValue && !impassable(num2))
					{
						int num3 = costFunc(tile, num2) + calcGrid[tile].knownCost;
						ushort status = calcGrid[num2].status;
						if ((status != statusClosedValue && status != statusOpenValue) || calcGrid[num2].knownCost > num3)
						{
							int num4 = num3;
							calcGrid[num2].parentTile = tile;
							calcGrid[num2].knownCost = num3;
							calcGrid[num2].status = statusOpenValue;
							calcGrid[num2].costNodeCost = num4;
							openList.Push(new CostNode(num2, num4));
						}
					}
				}
				calcGrid[tile].status = statusClosedValue;
				if (terminator != null && terminator(tile, calcGrid[tile].costNodeCost))
				{
					break;
				}
			}
		}

		public List<int>[] FloodPathsWithCostForTree(List<int> startTiles, Func<int, int, int> costFunc, Func<int, bool> impassable = null, Func<int, float, bool> terminator = null)
		{
			FloodPathsWithCost(startTiles, costFunc, impassable, terminator);
			WorldGrid grid = Find.World.grid;
			List<int>[] array = new List<int>[grid.TilesCount];
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (calcGrid[i].status != statusClosedValue)
				{
					continue;
				}
				int parentTile = calcGrid[i].parentTile;
				if (parentTile != i)
				{
					if (array[parentTile] == null)
					{
						array[parentTile] = new List<int>();
					}
					array[parentTile].Add(i);
				}
			}
			return array;
		}

		private WorldPath FinalizedPath(int lastTile)
		{
			WorldPath emptyWorldPath = Find.WorldPathPool.GetEmptyWorldPath();
			int num = lastTile;
			while (true)
			{
				int parentTile = calcGrid[num].parentTile;
				int num2 = num;
				emptyWorldPath.AddNodeAtStart(num2);
				if (num2 == parentTile)
				{
					break;
				}
				num = parentTile;
			}
			emptyWorldPath.SetupFound(calcGrid[lastTile].knownCost);
			return emptyWorldPath;
		}

		private void ResetStatuses()
		{
			int num = calcGrid.Length;
			for (int i = 0; i < num; i++)
			{
				calcGrid[i].status = 0;
			}
			statusOpenValue = 1;
			statusClosedValue = 2;
		}

		private int CalculateHeuristicStrength(int startTile, int destTile)
		{
			float x = Find.WorldGrid.ApproxDistanceInTiles(startTile, destTile);
			return Mathf.RoundToInt(HeuristicStrength_DistanceCurve.Evaluate(x));
		}
	}
}
