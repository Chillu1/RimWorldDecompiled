using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class CaravanArrivalTimeEstimator
{
	private static int cacheTicks = -1;

	private static Caravan cachedForCaravan;

	private static PlanetTile cachedForDest = PlanetTile.Invalid;

	private static int cachedResult = -1;

	private const int CacheDuration = 100;

	private const int MaxIterations = 10000;

	private static readonly List<(PlanetTile tile, int ticks)> tmpTicksToArrive = new List<(PlanetTile, int)>();

	public static int EstimatedTicksToArrive(Caravan caravan, bool allowCaching)
	{
		if (allowCaching && caravan == cachedForCaravan && caravan.pather.Destination == cachedForDest && Find.TickManager.TicksGame - cacheTicks < 100)
		{
			return cachedResult;
		}
		PlanetTile to;
		int result;
		if (!caravan.Spawned || !caravan.pather.Moving || caravan.pather.curPath == null)
		{
			to = PlanetTile.Invalid;
			result = 0;
		}
		else
		{
			to = caravan.pather.Destination;
			result = EstimatedTicksToArrive(caravan.Tile, to, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove, Find.TickManager.TicksAbs);
		}
		if (allowCaching)
		{
			cacheTicks = Find.TickManager.TicksGame;
			cachedForCaravan = caravan;
			cachedForDest = to;
			cachedResult = result;
		}
		return result;
	}

	public static int EstimatedTicksToArrive(PlanetTile from, PlanetTile to, Caravan caravan)
	{
		using WorldPath worldPath = from.Layer.Pather.FindPath(from, to, caravan);
		if (!worldPath.Found)
		{
			return 0;
		}
		return EstimatedTicksToArrive(from, to, worldPath, 0f, caravan?.TicksPerMove ?? 3300, Find.TickManager.TicksAbs);
	}

	public static int EstimatedTicksToArrive(PlanetTile from, PlanetTile to, WorldPath path, float nextTileCostLeft, int caravanTicksPerMove, int curTicksAbs)
	{
		tmpTicksToArrive.Clear();
		EstimatedTicksToArriveToEvery(from, to, path, nextTileCostLeft, caravanTicksPerMove, curTicksAbs, tmpTicksToArrive);
		return EstimatedTicksToArrive(to, tmpTicksToArrive);
	}

	public static void EstimatedTicksToArriveToEvery(PlanetTile from, PlanetTile to, WorldPath path, float nextTileCostLeft, int caravanTicksPerMove, int curTicksAbs, List<(PlanetTile tile, int ticks)> outTicksToArrive)
	{
		outTicksToArrive.Clear();
		outTicksToArrive.Add((from, 0));
		if (from == to || !from.Valid || !from.LayerDef.SurfaceTiles)
		{
			outTicksToArrive.Add((to, 0));
			return;
		}
		int num = 0;
		PlanetTile planetTile = from;
		PlanetTile planetTile2 = from;
		int num2 = 0;
		int num3 = Mathf.CeilToInt(20000f) - 1;
		int num4 = 60000 - num3;
		int num5 = 0;
		int num6 = 0;
		int num8;
		if (CaravanNightRestUtility.WouldBeRestingAt(from, curTicksAbs))
		{
			if (Caravan_PathFollower.IsValidFinalPushDestination(to) && (path.Peek(0) == to || (nextTileCostLeft <= 0f && path.NodesLeftCount >= 2 && path.Peek(1) == to)))
			{
				int num7 = Mathf.CeilToInt(GetCostToMove(nextTileCostLeft, path.Peek(0) == to, curTicksAbs, num, caravanTicksPerMove, from, to) / 1f);
				if (num7 <= 10000)
				{
					num += num7;
					outTicksToArrive.Add((to, num));
					return;
				}
			}
			num += CaravanNightRestUtility.LeftRestTicksAt(from, curTicksAbs);
			num8 = num4;
		}
		else
		{
			num8 = CaravanNightRestUtility.LeftNonRestTicksAt(from, curTicksAbs);
		}
		while (true)
		{
			num6++;
			if (num6 >= 10000)
			{
				Log.ErrorOnce("Could not calculate estimated ticks to arrive. Too many iterations.", 1837451324);
				outTicksToArrive.Add((to, num));
				return;
			}
			if (num5 <= 0)
			{
				if (planetTile2 == to)
				{
					outTicksToArrive.Add((to, num));
					return;
				}
				bool firstInPath = num2 == 0;
				planetTile = planetTile2;
				planetTile2 = path.Peek(num2);
				num2++;
				outTicksToArrive.Add((planetTile, num));
				num5 = Mathf.CeilToInt(GetCostToMove(nextTileCostLeft, firstInPath, curTicksAbs, num, caravanTicksPerMove, planetTile, planetTile2) / 1f);
			}
			if (num8 < num5)
			{
				num += num8;
				num5 -= num8;
				if (planetTile2 == to && num5 <= 10000 && Caravan_PathFollower.IsValidFinalPushDestination(to))
				{
					break;
				}
				num += num3;
				num8 = num4;
			}
			else
			{
				num += num5;
				num8 -= num5;
				num5 = 0;
			}
		}
		num += num5;
		outTicksToArrive.Add((to, num));
	}

	private static float GetCostToMove(float initialNextTileCostLeft, bool firstInPath, int initialTicksAbs, int curResult, int caravanTicksPerMove, PlanetTile curTile, PlanetTile nextTile)
	{
		if (firstInPath)
		{
			return initialNextTileCostLeft;
		}
		int value = initialTicksAbs + curResult;
		return Caravan_PathFollower.CostToMove(caravanTicksPerMove, curTile, nextTile, value);
	}

	public static int EstimatedTicksToArrive(PlanetTile destinationTile, List<(PlanetTile tile, int ticks)> estimatedTicksToArriveToEvery)
	{
		if (!destinationTile.Valid)
		{
			return 0;
		}
		for (int i = 0; i < estimatedTicksToArriveToEvery.Count; i++)
		{
			if (destinationTile == estimatedTicksToArriveToEvery[i].tile)
			{
				return estimatedTicksToArriveToEvery[i].ticks;
			}
		}
		return 0;
	}

	public static PlanetTile TileIllBeInAt(int ticksAbs, List<(PlanetTile tile, int ticks)> estimatedTicksToArriveToEvery, int ticksAbsUsedToCalculateEstimatedTicksToArriveToEvery)
	{
		if (!estimatedTicksToArriveToEvery.Any())
		{
			return PlanetTile.Invalid;
		}
		for (int num = estimatedTicksToArriveToEvery.Count - 1; num >= 0; num--)
		{
			int num2 = ticksAbsUsedToCalculateEstimatedTicksToArriveToEvery + estimatedTicksToArriveToEvery[num].ticks;
			if (ticksAbs >= num2)
			{
				return estimatedTicksToArriveToEvery[num].tile;
			}
		}
		return estimatedTicksToArriveToEvery[0].tile;
	}
}
