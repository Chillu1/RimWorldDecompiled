using System;
using System.Collections.Generic;
using LudeonTK;
using Unity.Collections;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public class WorldPathing : IDisposable
{
	private struct PathFinderNodeFast
	{
		public enum Status : byte
		{
			None,
			Open,
			Closed
		}

		public int knownCost;

		public int heuristicCost;

		public PlanetTile parentTile;

		public int costNodeCost;

		public Status status;
	}

	private NativePriorityQueue<int, int, IntMinComparer> frontier;

	private NativeArray<PathFinderNodeFast> calcGrid;

	private const int SearchLimit = 500000;

	private static readonly SimpleCurve HeuristicStrength_DistanceCurve = new SimpleCurve
	{
		new CurvePoint(30f, 1f),
		new CurvePoint(40f, 1.3f),
		new CurvePoint(130f, 2f)
	};

	private const float BestRoadDiscount = 0.5f;

	public PlanetLayer Layer { get; }

	public WorldPathing(PlanetLayer layer)
	{
		Layer = layer;
		calcGrid = new NativeArray<PathFinderNodeFast>(layer.TilesCount, Allocator.Persistent);
		frontier = new NativePriorityQueue<int, int, IntMinComparer>(Mathf.Min(layer.TilesCount, 500000), default(IntMinComparer), Allocator.Persistent);
	}

	public WorldPath FindPath(PlanetTile startTile, PlanetTile destTile, Caravan caravan, Func<float, bool> terminator = null)
	{
		if (!startTile.Valid)
		{
			Log.Error($"Tried to FindPath with invalid start tile {startTile}, caravan = {caravan}");
			return WorldPath.NotFound;
		}
		if (!destTile.Valid)
		{
			Log.Error($"Tried to FindPath with invalid dest tile {destTile}, caravan = {caravan}");
			return WorldPath.NotFound;
		}
		if (startTile.Layer != destTile.Layer)
		{
			Log.Error($"Tried to FindPath to a different layer {startTile} -> {destTile}, caravan = {caravan}");
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
		PlanetTile tile = startTile;
		World world = Find.World;
		WorldGrid grid = world.grid;
		NativeArray<int> unsafeTileIDToNeighbors_offsets = startTile.Layer.UnsafeTileIDToNeighbors_offsets;
		NativeArray<PlanetTile> unsafeTileIDToNeighbors_values = startTile.Layer.UnsafeTileIDToNeighbors_values;
		Vector3 normalized = grid.GetTileCenter(destTile).normalized;
		float[] array = world.pathGrid.layerMovementDifficulty[startTile.Layer];
		int num = 0;
		int num2 = caravan?.TicksPerMove ?? 3300;
		int num3 = CalculateHeuristicStrength(startTile, destTile);
		frontier.Clear();
		calcGrid.Clear();
		InitalizeSearch(tile);
		while (true)
		{
			if (frontier.Count <= 0)
			{
				Log.Warning($"{caravan} pathing from {startTile} to {destTile} ran out of tiles to process.");
				return WorldPath.NotFound;
			}
			frontier.Dequeue(out var element, out var priority);
			PathFinderNodeFast value = calcGrid[element];
			if (priority != value.costNodeCost)
			{
				continue;
			}
			tile = new PlanetTile(element, Layer);
			if (value.status == PathFinderNodeFast.Status.Closed)
			{
				continue;
			}
			if (tile == destTile)
			{
				return FinalizedPath(tile);
			}
			if (num > 500000)
			{
				Log.Warning($"{caravan} pathing from {startTile} to {destTile} hit search limit of {500000} tiles.");
				return WorldPath.NotFound;
			}
			(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(unsafeTileIDToNeighbors_offsets, unsafeTileIDToNeighbors_values, tile.tileId);
			int item = listIndexes.start;
			int item2 = listIndexes.end;
			for (int i = item; i < item2; i++)
			{
				PlanetTile planetTile = unsafeTileIDToNeighbors_values[i];
				PathFinderNodeFast value2 = calcGrid[planetTile.tileId];
				if (value2.status == PathFinderNodeFast.Status.Closed || world.Impassable(planetTile))
				{
					continue;
				}
				int num4 = (int)((float)num2 * array[planetTile.tileId] * grid.GetRoadMovementDifficultyMultiplier(tile, planetTile)) + value.knownCost;
				PathFinderNodeFast.Status status = calcGrid[planetTile.tileId].status;
				if ((status != PathFinderNodeFast.Status.Closed && status != PathFinderNodeFast.Status.Open) || calcGrid[planetTile.tileId].knownCost > num4)
				{
					Vector3 tileCenter = grid.GetTileCenter(planetTile);
					if (status != PathFinderNodeFast.Status.Closed && status != PathFinderNodeFast.Status.Open)
					{
						float num5 = grid.ApproxDistanceInTiles(GenMath.SphericalDistance(tileCenter.normalized, normalized));
						value2.heuristicCost = Mathf.RoundToInt((float)num2 * num5 * (float)num3 * 0.5f);
					}
					int num6 = num4 + calcGrid[planetTile.tileId].heuristicCost;
					value2.parentTile = tile;
					value2.knownCost = num4;
					value2.status = PathFinderNodeFast.Status.Open;
					value2.costNodeCost = num6;
					calcGrid[planetTile.tileId] = value2;
					frontier.Enqueue(planetTile.tileId, num6);
				}
			}
			num++;
			value.status = PathFinderNodeFast.Status.Closed;
			calcGrid[element] = value;
			if (terminator != null && terminator(value.costNodeCost))
			{
				break;
			}
		}
		return WorldPath.NotFound;
	}

	private void InitalizeSearch(PlanetTile tile)
	{
		PathFinderNodeFast value = calcGrid[tile.tileId];
		value.knownCost = 0;
		value.heuristicCost = 0;
		value.costNodeCost = 0;
		value.parentTile = tile;
		value.status = PathFinderNodeFast.Status.Open;
		calcGrid[tile.tileId] = value;
		frontier.Clear();
		frontier.Enqueue(tile.tileId, 0);
	}

	public void FloodPathsWithCost(List<PlanetTile> startTiles, Func<PlanetTile, PlanetTile, int> costFunc, Func<PlanetTile, bool> impassable = null, Func<PlanetTile, float, bool> terminator = null)
	{
		if (startTiles.Count < 1 || startTiles.Contains(PlanetTile.Invalid))
		{
			Log.Error("Tried to FindPath with invalid start tiles");
			return;
		}
		for (int i = 0; i < startTiles.Count; i++)
		{
			if (startTiles[i].Layer != Layer)
			{
				Log.Error($"Tried to FindPath with tiles on different layers, layer is {Layer} but tile at index {i} is on layer {startTiles[i].Layer}");
				return;
			}
		}
		NativeArray<int> unsafeTileIDToNeighbors_offsets = Layer.UnsafeTileIDToNeighbors_offsets;
		NativeArray<PlanetTile> unsafeTileIDToNeighbors_values = Layer.UnsafeTileIDToNeighbors_values;
		if (impassable == null)
		{
			impassable = (PlanetTile tid) => Find.World.Impassable(tid);
		}
		calcGrid.Clear();
		frontier.Clear();
		foreach (PlanetTile startTile in startTiles)
		{
			PathFinderNodeFast value = calcGrid[startTile.tileId];
			value.knownCost = 0;
			value.costNodeCost = 0;
			value.parentTile = startTile;
			value.status = PathFinderNodeFast.Status.Open;
			calcGrid[startTile.tileId] = value;
			frontier.Enqueue(startTile.tileId, 0);
		}
		while (frontier.Count > 0)
		{
			frontier.Dequeue(out var element, out var priority);
			PathFinderNodeFast value2 = calcGrid[element];
			if (priority != value2.costNodeCost)
			{
				continue;
			}
			PlanetTile planetTile = new PlanetTile(element, Layer);
			if (calcGrid[planetTile.tileId].status == PathFinderNodeFast.Status.Closed)
			{
				continue;
			}
			(int start, int end) listIndexes = PackedListOfLists.GetListIndexes(unsafeTileIDToNeighbors_offsets, unsafeTileIDToNeighbors_values, planetTile.tileId);
			int item = listIndexes.start;
			int item2 = listIndexes.end;
			for (int num = item; num < item2; num++)
			{
				PlanetTile planetTile2 = unsafeTileIDToNeighbors_values[num];
				PathFinderNodeFast value3 = calcGrid[planetTile2.tileId];
				if (calcGrid[planetTile2.tileId].status != PathFinderNodeFast.Status.Closed && !impassable(planetTile2))
				{
					int num2 = costFunc(planetTile, planetTile2) + calcGrid[planetTile.tileId].knownCost;
					PathFinderNodeFast.Status status = calcGrid[planetTile2.tileId].status;
					if ((status != PathFinderNodeFast.Status.Closed && status != PathFinderNodeFast.Status.Open) || calcGrid[planetTile2.tileId].knownCost > num2)
					{
						int num3 = num2;
						value3.parentTile = planetTile;
						value3.knownCost = num2;
						value3.status = PathFinderNodeFast.Status.Open;
						value3.costNodeCost = num3;
						calcGrid[planetTile2.tileId] = value3;
						frontier.Enqueue(planetTile2.tileId, num3);
					}
				}
			}
			value2.status = PathFinderNodeFast.Status.Closed;
			calcGrid[planetTile.tileId] = value2;
			if (terminator != null && terminator(planetTile, calcGrid[planetTile.tileId].costNodeCost))
			{
				break;
			}
		}
	}

	public List<PlanetTile>[] FloodPathsWithCostForTree(List<PlanetTile> startTiles, Func<PlanetTile, PlanetTile, int> costFunc, Func<PlanetTile, bool> impassable = null, Func<PlanetTile, float, bool> terminator = null)
	{
		FloodPathsWithCost(startTiles, costFunc, impassable, terminator);
		PlanetLayer layer = startTiles[0].Layer;
		List<PlanetTile>[] array = new List<PlanetTile>[layer.TilesCount];
		for (int i = 0; i < layer.TilesCount; i++)
		{
			if (calcGrid[i].status != PathFinderNodeFast.Status.Closed)
			{
				continue;
			}
			PlanetTile planetTile = new PlanetTile(i, layer);
			PlanetTile parentTile = calcGrid[i].parentTile;
			if (parentTile != planetTile)
			{
				if (array[parentTile.tileId] == null)
				{
					array[parentTile.tileId] = new List<PlanetTile>();
				}
				array[parentTile.tileId].Add(planetTile);
			}
		}
		return array;
	}

	private WorldPath FinalizedPath(PlanetTile lastTile)
	{
		WorldPath emptyWorldPath = Find.WorldPathPool.GetEmptyWorldPath();
		PlanetTile planetTile = lastTile;
		while (true)
		{
			PlanetTile parentTile = calcGrid[planetTile.tileId].parentTile;
			PlanetTile planetTile2 = planetTile;
			emptyWorldPath.AddNodeAtStart(planetTile2);
			if (planetTile2 == parentTile)
			{
				break;
			}
			planetTile = parentTile;
		}
		emptyWorldPath.SetupFound(calcGrid[lastTile.tileId].knownCost, lastTile.Layer);
		return emptyWorldPath;
	}

	private int CalculateHeuristicStrength(PlanetTile startTile, PlanetTile destTile)
	{
		float x = Find.WorldGrid.ApproxDistanceInTiles(startTile, destTile);
		return Mathf.RoundToInt(HeuristicStrength_DistanceCurve.Evaluate(x));
	}

	public void Dispose()
	{
		NativeArrayUtility.EnsureDisposed(ref calcGrid);
		NativeArrayUtility.EnsureDisposed(ref frontier);
	}
}
