using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class DaysUntilRotCalculator
	{
		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		private static List<ThingCount> tmpThingCountsFromTradeables = new List<ThingCount>();

		private static List<Pair<float, float>> tmpNutritions = new List<Pair<float, float>>();

		private static List<Thing> thingsInReverse = new List<Thing>();

		private static List<Pair<int, int>> tmpTicksToArrive = new List<Pair<int, int>>();

		public const float InfiniteDaysUntilRot = 600f;

		public static float ApproxDaysUntilRot(List<ThingCount> potentiallyFood, int tile, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpTicksToArrive.Clear();
			if (path != null && path.Found)
			{
				CaravanArrivalTimeEstimator.EstimatedTicksToArriveToEvery(tile, path.LastNode, path, nextTileCostLeft, caravanTicksPerMove, Find.TickManager.TicksAbs, tmpTicksToArrive);
			}
			tmpNutritions.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				ThingCount thingCount = potentiallyFood[i];
				if (thingCount.Count > 0 && thingCount.Thing.def.IsNutritionGivingIngestible)
				{
					CompRottable compRottable = thingCount.Thing.TryGetComp<CompRottable>();
					float first = (compRottable == null || !compRottable.Active) ? 600f : ((float)ApproxTicksUntilRot_AssumeTimePassesBy(compRottable, tile, tmpTicksToArrive) / 60000f);
					float second = thingCount.Thing.GetStatValue(StatDefOf.Nutrition) * (float)thingCount.Count;
					tmpNutritions.Add(new Pair<float, float>(first, second));
				}
			}
			return GenMath.WeightedMedian(tmpNutritions, 600f);
		}

		public static int ApproxTicksUntilRot_AssumeTimePassesBy(CompRottable rot, int tile, List<Pair<int, int>> ticksToArrive = null)
		{
			float num = 0f;
			int num2 = Find.TickManager.TicksAbs;
			while (num < 1f && (float)num2 < (float)Find.TickManager.TicksAbs + 3.606E+07f)
			{
				int tile2 = ticksToArrive.NullOrEmpty() ? tile : CaravanArrivalTimeEstimator.TileIllBeInAt(num2, ticksToArrive, Find.TickManager.TicksAbs);
				int num3 = Mathf.FloorToInt((float)rot.ApproxTicksUntilRotWhenAtTempOfTile(tile2, num2) * (1f - num));
				if (num3 <= 0)
				{
					break;
				}
				int b = (num3 < 10800000) ? ((num3 < 3600000) ? ((num3 < 600000) ? 27000 : 66000) : 125999) : 306000;
				int num4 = Mathf.Min(num3, b);
				num += (float)num4 / (float)num3;
				num2 += num4;
			}
			return num2 - Find.TickManager.TicksAbs;
		}

		public static float ApproxDaysUntilRot(Caravan caravan)
		{
			return ApproxDaysUntilRot(CaravanInventoryUtility.AllInventoryItems(caravan), caravan.Tile, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove);
		}

		public static float ApproxDaysUntilRot(List<Thing> potentiallyFood, int tile, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				tmpThingCounts.Add(new ThingCount(potentiallyFood[i], potentiallyFood[i].stackCount));
			}
			float result = ApproxDaysUntilRot(tmpThingCounts, tile, path, nextTileCostLeft, caravanTicksPerMove);
			tmpThingCounts.Clear();
			return result;
		}

		public static float ApproxDaysUntilRot(List<TransferableOneWay> transferables, int tile, IgnorePawnsInventoryMode ignoreInventory, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (!transferableOneWay.HasAnyThing)
				{
					continue;
				}
				if (transferableOneWay.AnyThing is Pawn)
				{
					for (int j = 0; j < transferableOneWay.CountToTransfer; j++)
					{
						Pawn pawn = (Pawn)transferableOneWay.things[j];
						if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
						{
							ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
							for (int k = 0; k < innerContainer.Count; k++)
							{
								tmpThingCounts.Add(new ThingCount(innerContainer[k], innerContainer[k].stackCount));
							}
						}
					}
				}
				else if (transferableOneWay.CountToTransfer > 0)
				{
					TransferableUtility.TransferNoSplit(transferableOneWay.things, transferableOneWay.CountToTransfer, delegate(Thing thing, int count)
					{
						tmpThingCounts.Add(new ThingCount(thing, count));
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
			}
			float result = ApproxDaysUntilRot(tmpThingCounts, tile, path, nextTileCostLeft, caravanTicksPerMove);
			tmpThingCounts.Clear();
			return result;
		}

		public static float ApproxDaysUntilRotLeftAfterTransfer(List<TransferableOneWay> transferables, int tile, IgnorePawnsInventoryMode ignoreInventory, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpThingCounts.Clear();
			for (int i = 0; i < transferables.Count; i++)
			{
				TransferableOneWay transferableOneWay = transferables[i];
				if (!transferableOneWay.HasAnyThing)
				{
					continue;
				}
				if (transferableOneWay.AnyThing is Pawn)
				{
					for (int num = transferableOneWay.things.Count - 1; num >= transferableOneWay.CountToTransfer; num--)
					{
						Pawn pawn = (Pawn)transferableOneWay.things[num];
						if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
						{
							ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
							for (int j = 0; j < innerContainer.Count; j++)
							{
								tmpThingCounts.Add(new ThingCount(innerContainer[j], innerContainer[j].stackCount));
							}
						}
					}
				}
				else if (transferableOneWay.MaxCount - transferableOneWay.CountToTransfer > 0)
				{
					thingsInReverse.Clear();
					thingsInReverse.AddRange(transferableOneWay.things);
					thingsInReverse.Reverse();
					TransferableUtility.TransferNoSplit(thingsInReverse, transferableOneWay.MaxCount - transferableOneWay.CountToTransfer, delegate(Thing thing, int count)
					{
						tmpThingCounts.Add(new ThingCount(thing, count));
					}, removeIfTakingEntireThing: false, errorIfNotEnoughThings: false);
				}
			}
			thingsInReverse.Clear();
			float result = ApproxDaysUntilRot(tmpThingCounts, tile, path, nextTileCostLeft, caravanTicksPerMove);
			tmpThingCounts.Clear();
			return result;
		}

		public static float ApproxDaysUntilRotLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, int tile, IgnorePawnsInventoryMode ignoreInventory)
		{
			tmpThingCountsFromTradeables.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCountsFromTradeables);
			tmpThingCounts.Clear();
			for (int num = tmpThingCountsFromTradeables.Count - 1; num >= 0; num--)
			{
				if (tmpThingCountsFromTradeables[num].Count > 0)
				{
					Pawn pawn = tmpThingCountsFromTradeables[num].Thing as Pawn;
					if (pawn != null)
					{
						if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
						{
							ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
							for (int i = 0; i < innerContainer.Count; i++)
							{
								tmpThingCounts.Add(new ThingCount(innerContainer[i], innerContainer[i].stackCount));
							}
						}
					}
					else
					{
						tmpThingCounts.Add(tmpThingCountsFromTradeables[num]);
					}
				}
			}
			tmpThingCountsFromTradeables.Clear();
			float result = ApproxDaysUntilRot(tmpThingCounts, tile);
			tmpThingCounts.Clear();
			return result;
		}
	}
}
