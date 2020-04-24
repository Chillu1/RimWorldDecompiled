using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class DaysWorthOfFoodCalculator
	{
		private static List<Pawn> tmpPawns = new List<Pawn>();

		private static List<ThingDefCount> tmpThingDefCounts = new List<ThingDefCount>();

		private static List<ThingCount> tmpThingCounts = new List<ThingCount>();

		public const float InfiniteDaysWorthOfFood = 600f;

		private static List<float> tmpDaysWorthOfFoodForPawn = new List<float>();

		private static List<ThingDefCount> tmpFood = new List<ThingDefCount>();

		private static List<ThingDefCount> tmpFood2 = new List<ThingDefCount>();

		private static List<Pair<int, int>> tmpTicksToArrive = new List<Pair<int, int>>();

		private static List<float> cachedNutritionBetweenHungryAndFed = new List<float>();

		private static List<int> cachedTicksUntilHungryWhenFed = new List<int>();

		private static List<float> cachedMaxFoodLevel = new List<float>();

		private static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingDefCount> extraFood, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300, bool assumeCaravanMoving = true)
		{
			if (!AnyFoodEatingPawn(pawns))
			{
				return 600f;
			}
			if (!assumeCaravanMoving)
			{
				path = null;
			}
			tmpFood.Clear();
			if (extraFood != null)
			{
				int i = 0;
				for (int count = extraFood.Count; i < count; i++)
				{
					ThingDefCount item = extraFood[i];
					if (item.ThingDef.IsNutritionGivingIngestible && item.Count > 0)
					{
						tmpFood.Add(item);
					}
				}
			}
			int j = 0;
			for (int count2 = pawns.Count; j < count2; j++)
			{
				Pawn pawn = pawns[j];
				if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
				{
					continue;
				}
				ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
				int k = 0;
				for (int count3 = innerContainer.Count; k < count3; k++)
				{
					Thing thing = innerContainer[k];
					if (thing.def.IsNutritionGivingIngestible)
					{
						tmpFood.Add(new ThingDefCount(thing.def, thing.stackCount));
					}
				}
			}
			tmpFood2.Clear();
			tmpFood2.AddRange(tmpFood);
			tmpFood.Clear();
			int l = 0;
			for (int count4 = tmpFood2.Count; l < count4; l++)
			{
				ThingDefCount item2 = tmpFood2[l];
				bool flag = false;
				int m = 0;
				for (int count5 = tmpFood.Count; m < count5; m++)
				{
					ThingDefCount thingDefCount = tmpFood[m];
					if (thingDefCount.ThingDef == item2.ThingDef)
					{
						tmpFood[m] = thingDefCount.WithCount(thingDefCount.Count + item2.Count);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					tmpFood.Add(item2);
				}
			}
			tmpDaysWorthOfFoodForPawn.Clear();
			int n = 0;
			for (int count6 = pawns.Count; n < count6; n++)
			{
				tmpDaysWorthOfFoodForPawn.Add(0f);
			}
			int ticksAbs = Find.TickManager.TicksAbs;
			tmpTicksToArrive.Clear();
			if (path != null && path.Found)
			{
				CaravanArrivalTimeEstimator.EstimatedTicksToArriveToEvery(tile, path.LastNode, path, nextTileCostLeft, caravanTicksPerMove, ticksAbs, tmpTicksToArrive);
			}
			cachedNutritionBetweenHungryAndFed.Clear();
			cachedTicksUntilHungryWhenFed.Clear();
			cachedMaxFoodLevel.Clear();
			int num = 0;
			for (int count7 = pawns.Count; num < count7; num++)
			{
				Pawn pawn2 = pawns[num];
				if (pawn2.RaceProps.EatsFood)
				{
					Need_Food food = pawn2.needs.food;
					cachedNutritionBetweenHungryAndFed.Add(food.NutritionBetweenHungryAndFed);
					cachedTicksUntilHungryWhenFed.Add(food.TicksUntilHungryWhenFedIgnoringMalnutrition);
					cachedMaxFoodLevel.Add(food.MaxLevel);
				}
				else
				{
					cachedNutritionBetweenHungryAndFed.Add(0f);
					cachedTicksUntilHungryWhenFed.Add(0);
					cachedMaxFoodLevel.Add(0f);
				}
			}
			float num2 = 0f;
			float num3 = 0f;
			float num4 = 0f;
			bool flag2 = false;
			WorldGrid worldGrid = Find.WorldGrid;
			bool flag3;
			do
			{
				flag3 = false;
				int num5 = ticksAbs + (int)(num3 * 60000f);
				int num6 = (path != null) ? CaravanArrivalTimeEstimator.TileIllBeInAt(num5, tmpTicksToArrive, ticksAbs) : tile;
				bool flag4 = CaravanNightRestUtility.WouldBeRestingAt(num6, num5);
				float progressPerTick = ForagedFoodPerDayCalculator.GetProgressPerTick(assumeCaravanMoving && !flag4, flag4);
				float num7 = 1f / progressPerTick;
				bool flag5 = VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsAt(num6, num5);
				float num8 = num3 - num2;
				if (num8 > 0f)
				{
					num4 += num8 * 60000f;
					if (num4 >= num7)
					{
						BiomeDef biome = worldGrid[num6].biome;
						int num9 = Mathf.RoundToInt(ForagedFoodPerDayCalculator.GetForagedFoodCountPerInterval(pawns, biome, faction));
						ThingDef foragedFood = biome.foragedFood;
						while (num4 >= num7)
						{
							num4 -= num7;
							if (num9 <= 0)
							{
								continue;
							}
							bool flag6 = false;
							for (int num10 = tmpFood.Count - 1; num10 >= 0; num10--)
							{
								ThingDefCount thingDefCount2 = tmpFood[num10];
								if (thingDefCount2.ThingDef == foragedFood)
								{
									tmpFood[num10] = thingDefCount2.WithCount(thingDefCount2.Count + num9);
									flag6 = true;
									break;
								}
							}
							if (!flag6)
							{
								tmpFood.Add(new ThingDefCount(foragedFood, num9));
							}
						}
					}
				}
				num2 = num3;
				int num11 = 0;
				for (int count8 = pawns.Count; num11 < count8; num11++)
				{
					Pawn pawn3 = pawns[num11];
					if (!pawn3.RaceProps.EatsFood)
					{
						continue;
					}
					if (flag5 && VirtualPlantsUtility.CanEverEatVirtualPlants(pawn3))
					{
						if (tmpDaysWorthOfFoodForPawn[num11] < num3)
						{
							tmpDaysWorthOfFoodForPawn[num11] = num3;
						}
						else
						{
							tmpDaysWorthOfFoodForPawn[num11] += 0.45f;
						}
						flag3 = true;
					}
					else
					{
						float num12 = cachedNutritionBetweenHungryAndFed[num11];
						int num13 = cachedTicksUntilHungryWhenFed[num11];
						do
						{
							int num14 = BestEverEdibleFoodIndexFor(pawn3, tmpFood);
							if (num14 < 0)
							{
								if (tmpDaysWorthOfFoodForPawn[num11] < num3)
								{
									flag2 = true;
								}
								break;
							}
							ThingDefCount thingDefCount3 = tmpFood[num14];
							float num15 = Mathf.Min(thingDefCount3.ThingDef.ingestible.CachedNutrition, num12);
							float num16 = num15 / num12 * (float)num13 / 60000f;
							int num17 = Mathf.Min(Mathf.CeilToInt(Mathf.Min(0.2f, cachedMaxFoodLevel[num11]) / num15), thingDefCount3.Count);
							tmpDaysWorthOfFoodForPawn[num11] += num16 * (float)num17;
							tmpFood[num14] = thingDefCount3.WithCount(thingDefCount3.Count - num17);
							flag3 = true;
						}
						while (tmpDaysWorthOfFoodForPawn[num11] < num3);
					}
					if (flag2)
					{
						break;
					}
					num3 = Mathf.Max(num3, tmpDaysWorthOfFoodForPawn[num11]);
				}
			}
			while (!(!flag3 | flag2) && !(num3 > 601f));
			float num18 = 600f;
			int num19 = 0;
			for (int count9 = pawns.Count; num19 < count9; num19++)
			{
				if (pawns[num19].RaceProps.EatsFood)
				{
					num18 = Mathf.Min(num18, tmpDaysWorthOfFoodForPawn[num19]);
				}
			}
			return num18;
		}

		public static float ApproxDaysWorthOfFood(Caravan caravan)
		{
			return ApproxDaysWorthOfFood(caravan.PawnsListForReading, null, caravan.Tile, IgnorePawnsInventoryMode.DontIgnore, caravan.Faction, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove, caravan.pather.Moving && !caravan.pather.Paused);
		}

		public static float ApproxDaysWorthOfFood(List<TransferableOneWay> transferables, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
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
						if (pawn.RaceProps.EatsFood)
						{
							tmpPawns.Add(pawn);
						}
					}
				}
				else
				{
					tmpThingDefCounts.Add(new ThingDefCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
				}
			}
			float result = ApproxDaysWorthOfFood(tmpPawns, tmpThingDefCounts, tile, ignoreInventory, faction, path, nextTileCostLeft, caravanTicksPerMove);
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTransfer(List<TransferableOneWay> transferables, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
		{
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
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
						if (pawn.RaceProps.EatsFood)
						{
							tmpPawns.Add(pawn);
						}
					}
				}
				else
				{
					tmpThingDefCounts.Add(new ThingDefCount(transferableOneWay.ThingDef, transferableOneWay.MaxCount - transferableOneWay.CountToTransfer));
				}
			}
			float result = ApproxDaysWorthOfFood(tmpPawns, tmpThingDefCounts, tile, ignoreInventory, faction, path, nextTileCostLeft, caravanTicksPerMove);
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<Thing> potentiallyFood, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
		{
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
			for (int i = 0; i < pawns.Count; i++)
			{
				Pawn pawn = pawns[i];
				if (pawn.RaceProps.EatsFood)
				{
					tmpPawns.Add(pawn);
				}
			}
			for (int j = 0; j < potentiallyFood.Count; j++)
			{
				tmpThingDefCounts.Add(new ThingDefCount(potentiallyFood[j].def, potentiallyFood[j].stackCount));
			}
			float result = ApproxDaysWorthOfFood(tmpPawns, tmpThingDefCounts, tile, ignoreInventory, faction);
			tmpThingDefCounts.Clear();
			tmpPawns.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingCount> potentiallyFood, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
		{
			tmpThingDefCounts.Clear();
			for (int i = 0; i < potentiallyFood.Count; i++)
			{
				if (potentiallyFood[i].Count > 0)
				{
					tmpThingDefCounts.Add(new ThingDefCount(potentiallyFood[i].Thing.def, potentiallyFood[i].Count));
				}
			}
			float result = ApproxDaysWorthOfFood(pawns, tmpThingDefCounts, tile, ignoreInventory, faction);
			tmpThingDefCounts.Clear();
			return result;
		}

		public static float ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
		{
			tmpThingCounts.Clear();
			TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
			tmpPawns.Clear();
			tmpThingDefCounts.Clear();
			for (int num = tmpThingCounts.Count - 1; num >= 0; num--)
			{
				if (tmpThingCounts[num].Count > 0)
				{
					Pawn pawn = tmpThingCounts[num].Thing as Pawn;
					if (pawn != null)
					{
						if (pawn.RaceProps.EatsFood)
						{
							tmpPawns.Add(pawn);
						}
					}
					else
					{
						tmpThingDefCounts.Add(new ThingDefCount(tmpThingCounts[num].Thing.def, tmpThingCounts[num].Count));
					}
				}
			}
			tmpThingCounts.Clear();
			float result = ApproxDaysWorthOfFood(tmpPawns, tmpThingDefCounts, tile, ignoreInventory, faction);
			tmpPawns.Clear();
			tmpThingDefCounts.Clear();
			return result;
		}

		private static bool AnyFoodEatingPawn(List<Pawn> pawns)
		{
			int i = 0;
			for (int count = pawns.Count; i < count; i++)
			{
				if (pawns[i].RaceProps.EatsFood)
				{
					return true;
				}
			}
			return false;
		}

		private static int BestEverEdibleFoodIndexFor(Pawn pawn, List<ThingDefCount> food)
		{
			int num = -1;
			float num2 = 0f;
			int i = 0;
			for (int count = food.Count; i < count; i++)
			{
				if (food[i].Count <= 0)
				{
					continue;
				}
				ThingDef thingDef = food[i].ThingDef;
				if (CaravanPawnsNeedsUtility.CanEatForNutritionEver(thingDef, pawn))
				{
					float foodScore = CaravanPawnsNeedsUtility.GetFoodScore(thingDef, pawn, thingDef.ingestible.CachedNutrition);
					if (num < 0 || foodScore > num2)
					{
						num = i;
						num2 = foodScore;
					}
				}
			}
			return num;
		}
	}
}
