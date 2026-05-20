using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class TilesPerDayCalculator
{
	private static readonly List<Pawn> tmpPawns = new List<Pawn>();

	private static readonly List<ThingCount> tmpThingCounts = new List<ThingCount>();

	public static float ApproxTilesPerDay(int caravanTicksPerMove, PlanetTile tile, PlanetTile nextTile, StringBuilder explanation = null, string caravanTicksPerMoveExplanation = null, bool immobile = false)
	{
		if (!nextTile.Valid)
		{
			nextTile = Find.WorldGrid.FindMostReasonableAdjacentTileForDisplayedPathCost(tile);
		}
		PlanetTile end = nextTile;
		bool immobile2 = immobile;
		int num = Mathf.CeilToInt((float)Caravan_PathFollower.CostToMove(caravanTicksPerMove, tile, end, null, perceivedStatic: false, explanation, caravanTicksPerMoveExplanation, immobile2) / 1f);
		if (num == 0)
		{
			return 0f;
		}
		return 60000f / (float)num;
	}

	public static float ApproxTilesPerDay(Caravan caravan, StringBuilder explanation = null)
	{
		return ApproxTilesPerDay(caravan.TicksPerMove, caravan.Tile, caravan.pather.Moving ? caravan.pather.nextTile : PlanetTile.Invalid, explanation, (explanation != null) ? caravan.TicksPerMoveExplanation : null, caravan.ImmobilizedByMass || caravan.Shuttle != null);
	}

	public static float ApproxTilesPerDay(List<TransferableOneWay> transferables, float massUsage, float massCapacity, PlanetTile tile, PlanetTile nextTile, bool isShuttle, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < transferables.Count; i++)
		{
			TransferableOneWay transferableOneWay = transferables[i];
			if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
			{
				for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
				{
					tmpPawns.Add((Pawn)transferableOneWay.things[j]);
				}
			}
		}
		if (!tmpPawns.Any())
		{
			return 0f;
		}
		StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
		float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsage, massCapacity, isShuttle, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString(), massUsage > massCapacity);
		tmpPawns.Clear();
		return result;
	}

	public static float ApproxTilesPerDayLeftAfterTransfer(List<TransferableOneWay> transferables, float massUsageLeftAfterTransfer, float massCapacityLeftAfterTransfer, PlanetTile tile, PlanetTile nextTile, bool isShuttle, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < transferables.Count; i++)
		{
			TransferableOneWay transferableOneWay = transferables[i];
			if (transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn)
			{
				for (int num = transferableOneWay.things.Count - 1; num >= transferableOneWay.CountToTransfer; num--)
				{
					tmpPawns.Add((Pawn)transferableOneWay.things[num]);
				}
			}
		}
		if (!tmpPawns.Any())
		{
			return 0f;
		}
		StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
		float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsageLeftAfterTransfer, massCapacityLeftAfterTransfer, isShuttle, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString(), massUsageLeftAfterTransfer > massCapacityLeftAfterTransfer);
		tmpPawns.Clear();
		return result;
	}

	public static float ApproxTilesPerDayLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, float massUsageLeftAfterTradeableTransfer, float massCapacityLeftAfterTradeableTransfer, PlanetTile tile, PlanetTile nextTile, bool isShuttle, StringBuilder explanation = null)
	{
		tmpThingCounts.Clear();
		TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
		float result = ApproxTilesPerDay(tmpThingCounts, massUsageLeftAfterTradeableTransfer, massCapacityLeftAfterTradeableTransfer, tile, nextTile, isShuttle, explanation);
		tmpThingCounts.Clear();
		return result;
	}

	public static float ApproxTilesPerDay(List<ThingCount> thingCounts, float massUsage, float massCapacity, PlanetTile tile, PlanetTile nextTile, bool isShuttle, StringBuilder explanation = null)
	{
		tmpPawns.Clear();
		for (int i = 0; i < thingCounts.Count; i++)
		{
			if (thingCounts[i].Count > 0 && thingCounts[i].Thing is Pawn item)
			{
				tmpPawns.Add(item);
			}
		}
		if (!tmpPawns.Any())
		{
			return 0f;
		}
		StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
		float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsage, massCapacity, isShuttle, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString(), massUsage > massCapacity);
		tmpPawns.Clear();
		return result;
	}
}
