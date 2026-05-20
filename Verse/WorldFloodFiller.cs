using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Unity.Collections;

namespace Verse;

public class WorldFloodFiller
{
	public delegate bool WorldFillPredicate(PlanetTile from, PlanetTile to, int distance);

	private bool working;

	private PlanetLayer layer;

	private readonly Queue<(PlanetTile from, PlanetTile to)> openSet = new Queue<(PlanetTile, PlanetTile)>();

	private readonly List<int> traversalDistance = new List<int>();

	private readonly List<PlanetTile> visited = new List<PlanetTile>();

	public WorldFloodFiller(PlanetLayer layer)
	{
		this.layer = layer;
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, Action<PlanetTile> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		FloodFill(rootTile, passCheck, delegate(PlanetTile _, PlanetTile tile, int _)
		{
			processor(tile);
			return false;
		}, maxTilesToProcess, extraRootTiles);
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, Action<PlanetTile, int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		FloodFill(rootTile, passCheck, delegate(PlanetTile _, PlanetTile tile, int dist)
		{
			processor(tile, dist);
			return false;
		}, maxTilesToProcess, extraRootTiles);
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, Action<PlanetTile, PlanetTile, int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		FloodFill(rootTile, passCheck, delegate(PlanetTile from, PlanetTile tile, int dist)
		{
			processor(from, tile, dist);
			return false;
		}, maxTilesToProcess, extraRootTiles);
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, Predicate<PlanetTile> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		FloodFill(rootTile, passCheck, (PlanetTile _, PlanetTile tile, int _) => processor(tile), maxTilesToProcess, extraRootTiles);
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, Predicate<PlanetTile, int> processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		FloodFill(rootTile, passCheck, (PlanetTile _, PlanetTile tile, int dist) => processor(tile, dist), maxTilesToProcess, extraRootTiles);
	}

	public void FloodFill(PlanetTile rootTile, Predicate<PlanetTile> passCheck, WorldFillPredicate processor, int maxTilesToProcess = int.MaxValue, IEnumerable<PlanetTile> extraRootTiles = null)
	{
		if (working)
		{
			Log.Error("Nested FloodFill calls are not allowed. This will cause bugs.");
		}
		using (ProfilerBlock.Scope("WorldFloodFill"))
		{
			working = true;
			ClearVisited();
			if (rootTile.Valid && extraRootTiles == null && !passCheck(rootTile))
			{
				working = false;
				return;
			}
			int tilesCount = rootTile.Layer.TilesCount;
			int num = tilesCount;
			if (traversalDistance.Count < tilesCount)
			{
				traversalDistance.Capacity = tilesCount;
				for (int i = traversalDistance.Count; i < tilesCount; i++)
				{
					traversalDistance.Add(-1);
				}
			}
			NativeArray<int> unsafeTileIDToNeighbors_offsets = layer.UnsafeTileIDToNeighbors_offsets;
			NativeArray<PlanetTile> unsafeTileIDToNeighbors_values = layer.UnsafeTileIDToNeighbors_values;
			int num2 = 0;
			if (!unsafeTileIDToNeighbors_offsets.IsCreated)
			{
				return;
			}
			openSet.Clear();
			if (rootTile.Valid)
			{
				visited.Add(rootTile);
				traversalDistance[rootTile.tileId] = 0;
				openSet.Enqueue((PlanetTile.Invalid, rootTile));
			}
			if (extraRootTiles != null)
			{
				foreach (PlanetTile extraRootTile in extraRootTiles)
				{
					visited.Add(extraRootTile);
				}
				if (extraRootTiles is IList<PlanetTile> list)
				{
					for (int j = 0; j < list.Count; j++)
					{
						PlanetTile item = list[j];
						traversalDistance[item.tileId] = 0;
						openSet.Enqueue((PlanetTile.Invalid, item));
					}
				}
				else
				{
					foreach (PlanetTile extraRootTile2 in extraRootTiles)
					{
						traversalDistance[extraRootTile2.tileId] = 0;
						openSet.Enqueue((PlanetTile.Invalid, extraRootTile2));
					}
				}
			}
			while (openSet.Count > 0)
			{
				(PlanetTile from, PlanetTile to) tuple = openSet.Dequeue();
				PlanetTile item2 = tuple.from;
				PlanetTile item3 = tuple.to;
				int num3 = traversalDistance[item3.tileId];
				if (processor(item2, item3, num3))
				{
					break;
				}
				num2++;
				if (num2 == maxTilesToProcess)
				{
					break;
				}
				(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(unsafeTileIDToNeighbors_offsets, unsafeTileIDToNeighbors_values, item3);
				int item4 = listIndexes.start;
				int item5 = listIndexes.end;
				for (int k = item4; k < item5; k++)
				{
					PlanetTile planetTile = unsafeTileIDToNeighbors_values[k];
					if (traversalDistance[planetTile.tileId] == -1 && passCheck(planetTile))
					{
						visited.Add(planetTile);
						openSet.Enqueue((item3, planetTile));
						traversalDistance[planetTile.tileId] = num3 + 1;
					}
				}
				if (openSet.Count > num)
				{
					Log.Error($"Overflow on world flood fill (>{num} cells). Make sure we're not flooding over the same area after we check it.");
					working = false;
					return;
				}
			}
			working = false;
		}
	}

	private void ClearVisited()
	{
		int i = 0;
		for (int count = visited.Count; i < count; i++)
		{
			traversalDistance[visited[i].tileId] = -1;
		}
		visited.Clear();
		openSet.Clear();
	}
}
