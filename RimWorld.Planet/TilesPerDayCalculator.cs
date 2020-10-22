using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class TilesPerDayCalculator
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		public static float ApproxTilesPerDay(int caravanTicksPerMove, int tile, int nextTile, StringBuilder explanation = null, string caravanTicksPerMoveExplanation = null)
		{
			if (nextTile == -1)
			{
				nextTile = Find.WorldGrid.FindMostReasonableAdjacentTileForDisplayedPathCost(tile);
			}
			int num = Mathf.CeilToInt((float)Caravan_PathFollower.CostToMove(caravanTicksPerMove, tile, nextTile, null, perceivedStatic: false, explanation, caravanTicksPerMoveExplanation) / 1f);
			if (num == 0)
			{
				return 0f;
			}
			return 60000f / (float)num;
		}

		public static float ApproxTilesPerDay(Caravan caravan, StringBuilder explanation = null)
		{
			return ApproxTilesPerDay(caravan.TicksPerMove, caravan.Tile, caravan.pather.Moving ? caravan.pather.nextTile : (-1), explanation, (explanation != null) ? caravan.TicksPerMoveExplanation : null);
		}

		public static float ApproxTilesPerDay(List<TransferableOneWay> transferables, float massUsage, float massCapacity, int tile, int nextTile, StringBuilder explanation = null)
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
			float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsage, massCapacity, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString());
			tmpPawns.Clear();
			return result;
		}

		public static float ApproxTilesPerDayLeftAfterTransfer(List<TransferableOneWay> transferables, float massUsageLeftAfterTransfer, float massCapacityLeftAfterTransfer, int tile, int nextTile, StringBuilder explanation = null)
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
			float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsageLeftAfterTransfer, massCapacityLeftAfterTransfer, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString());
			tmpPawns.Clear();
			return result;
		}

		public static float ApproxTilesPerDayLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, float massUsageLeftAfterTradeableTransfer, float massCapacityLeftAfterTradeableTransfer, int tile, int nextTile, StringBuilder explanation = null)
		{
			tmpThingCounts.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
			float result = ApproxTilesPerDay(tmpThingCounts, massUsageLeftAfterTradeableTransfer, massCapacityLeftAfterTradeableTransfer, tile, nextTile, explanation);
			tmpThingCounts.Clear();
			return result;
		}

		public static float ApproxTilesPerDay(List<ThingCount> thingCounts, float massUsage, float massCapacity, int tile, int nextTile, StringBuilder explanation = null)
		{
			tmpPawns.Clear();
			for (int i = 0; i < thingCounts.Count; i++)
			{
				if (thingCounts[i].Count > 0)
				{
					Pawn pawn = thingCounts[i].Thing as Pawn;
					if (pawn != null)
					{
						tmpPawns.Add(pawn);
					}
				}
			}
			if (!tmpPawns.Any())
			{
				return 0f;
			}
			StringBuilder stringBuilder = ((explanation != null) ? new StringBuilder() : null);
			float result = ApproxTilesPerDay(CaravanTicksPerMoveUtility.GetTicksPerMove(tmpPawns, massUsage, massCapacity, stringBuilder), tile, nextTile, explanation, stringBuilder?.ToString());
			tmpPawns.Clear();
			return result;
		}
	}
}
