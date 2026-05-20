using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet;

public static class DaysWorthOfFoodCalculator
{
	private static readonly List<Pawn> tmpPawns = new List<Pawn>();

	private static readonly List<ThingDefCount> tmpThingDefCounts = new List<ThingDefCount>();

	private static readonly List<ThingCount> tmpThingCounts = new List<ThingCount>();

	public const float InfiniteDaysWorthOfFood = 600f;

	private static readonly List<float> tmpDaysWorthOfFoodForPawn = new List<float>();

	private static readonly List<ThingDefCount> tmpFood = new List<ThingDefCount>();

	private static readonly List<ThingDefCount> tmpFood2 = new List<ThingDefCount>();

	private static readonly List<(PlanetTile, int)> tmpTicksToArrive = new List<(PlanetTile, int)>();

	private static readonly List<float> cachedNutritionBetweenHungryAndFed = new List<float>();

	private static readonly List<int> cachedTicksUntilHungryWhenFed = new List<int>();

	private static readonly List<float> cachedMaxFoodLevel = new List<float>();

	private static readonly HashSet<Pawn> babiesWithFeeders = new HashSet<Pawn>();

	private static readonly List<Pawn> tmpLactatingPawns = new List<Pawn>(16);

	private static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingDefCount> extraFood, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300, bool assumeCaravanMoving = true)
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
			if (pawn2.RaceProps.EatsFood && pawn2.needs.food != null)
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
		babiesWithFeeders.Clear();
		tmpLactatingPawns.Clear();
		tmpLactatingPawns.AddRange(pawns);
		tmpLactatingPawns.RemoveAll((Pawn mom) => !ChildcareUtility.CanBreastfeed(mom, out var _));
		int count8 = tmpLactatingPawns.Count;
		for (int num5 = 0; num5 < count8; num5++)
		{
			for (int num6 = 0; num6 < 1; num6++)
			{
				tmpLactatingPawns.Add(tmpLactatingPawns[num5]);
			}
		}
		foreach (Pawn pawn3 in pawns)
		{
			if (ChildcareUtility.CanSuckle(pawn3, out var _))
			{
				int num7 = tmpLactatingPawns.FindIndex((Pawn feeder) => ChildcareUtility.CanMomBreastfeedBaby(feeder, pawn3, out reason2) && pawn3.mindState.AutofeedSetting(feeder) != AutofeedMode.Never);
				if (num7 >= 0)
				{
					tmpLactatingPawns[num7] = null;
					babiesWithFeeders.Add(pawn3);
				}
			}
		}
		bool flag3;
		do
		{
			flag3 = false;
			int num8 = ticksAbs + (int)(num3 * 60000f);
			PlanetTile tile2 = ((path != null) ? CaravanArrivalTimeEstimator.TileIllBeInAt(num8, tmpTicksToArrive, ticksAbs) : tile);
			bool flag4 = CaravanNightRestUtility.WouldBeRestingAt(tile2, num8);
			float progressPerTick = ForagedFoodPerDayCalculator.GetProgressPerTick(assumeCaravanMoving && !flag4, flag4);
			float num9 = 1f / progressPerTick;
			bool flag5 = VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsAt(tile2, num8);
			float num10 = num3 - num2;
			if (num10 > 0f)
			{
				num4 += num10 * 60000f;
				if (num4 >= num9)
				{
					BiomeDef primaryBiome = worldGrid[tile2].PrimaryBiome;
					int num11 = Mathf.RoundToInt(ForagedFoodPerDayCalculator.GetForagedFoodCountPerInterval(pawns, primaryBiome, faction));
					ThingDef foragedFood = primaryBiome.foragedFood;
					while (num4 >= num9)
					{
						num4 -= num9;
						if (num11 <= 0)
						{
							continue;
						}
						bool flag6 = false;
						for (int num12 = tmpFood.Count - 1; num12 >= 0; num12--)
						{
							ThingDefCount thingDefCount2 = tmpFood[num12];
							if (thingDefCount2.ThingDef == foragedFood)
							{
								tmpFood[num12] = thingDefCount2.WithCount(thingDefCount2.Count + num11);
								flag6 = true;
								break;
							}
						}
						if (!flag6)
						{
							tmpFood.Add(new ThingDefCount(foragedFood, num11));
						}
					}
				}
			}
			num2 = num3;
			int num13 = 0;
			for (int count9 = pawns.Count; num13 < count9; num13++)
			{
				Pawn pawn4 = pawns[num13];
				if (!pawn4.RaceProps.EatsFood || pawn4.needs?.food == null || babiesWithFeeders.Contains(pawn4))
				{
					continue;
				}
				if (flag5 && VirtualPlantsUtility.CanEverEatVirtualPlants(pawn4))
				{
					if (tmpDaysWorthOfFoodForPawn[num13] < num3)
					{
						tmpDaysWorthOfFoodForPawn[num13] = num3;
					}
					else
					{
						tmpDaysWorthOfFoodForPawn[num13] += 0.45f;
					}
					flag3 = true;
				}
				else
				{
					float num14 = cachedNutritionBetweenHungryAndFed[num13];
					int num15 = cachedTicksUntilHungryWhenFed[num13];
					do
					{
						int num16 = BestEverEdibleFoodIndexFor(pawn4, tmpFood);
						if (num16 < 0)
						{
							if (tmpDaysWorthOfFoodForPawn[num13] < num3)
							{
								flag2 = true;
							}
							break;
						}
						ThingDefCount thingDefCount3 = tmpFood[num16];
						float num17 = Mathf.Min(thingDefCount3.ThingDef.ingestible.CachedNutrition, num14);
						float num18 = num17 / num14 * (float)num15 / 60000f;
						int num19 = Mathf.Min(Mathf.CeilToInt(Mathf.Min(0.2f, cachedMaxFoodLevel[num13]) / num17), thingDefCount3.Count);
						tmpDaysWorthOfFoodForPawn[num13] += num18 * (float)num19;
						tmpFood[num16] = thingDefCount3.WithCount(thingDefCount3.Count - num19);
						flag3 = true;
					}
					while (tmpDaysWorthOfFoodForPawn[num13] < num3);
				}
				if (flag2)
				{
					break;
				}
				num3 = Mathf.Max(num3, tmpDaysWorthOfFoodForPawn[num13]);
			}
		}
		while (!(!flag3 || flag2) && !(num3 > 601f));
		float num20 = 600f;
		int num21 = 0;
		for (int count10 = pawns.Count; num21 < count10; num21++)
		{
			if (pawns[num21].RaceProps.EatsFood && pawns[num21].needs?.food != null && !babiesWithFeeders.Contains(pawns[num21]))
			{
				num20 = Mathf.Min(num20, tmpDaysWorthOfFoodForPawn[num21]);
			}
		}
		return num20;
	}

	public static float ApproxDaysWorthOfFood(Caravan caravan)
	{
		return ApproxDaysWorthOfFood(caravan.PawnsListForReading, null, caravan.Tile, IgnorePawnsInventoryMode.DontIgnore, caravan.Faction, caravan.pather.curPath, caravan.pather.nextTileCostLeft, caravan.TicksPerMove, caravan.pather.Moving && !caravan.pather.Paused);
	}

	public static float ApproxDaysWorthOfFood(List<TransferableOneWay> transferables, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
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
					if (pawn.RaceProps.EatsFood && pawn.needs?.food != null)
					{
						tmpPawns.Add(pawn);
					}
				}
			}
			else if (!(transferableOneWay.AnyThing is Corpse t) || t.GetRotStage() == RotStage.Fresh)
			{
				tmpThingDefCounts.Add(new ThingDefCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
			}
		}
		float result = ApproxDaysWorthOfFood(tmpPawns, tmpThingDefCounts, tile, ignoreInventory, faction, path, nextTileCostLeft, caravanTicksPerMove);
		tmpThingDefCounts.Clear();
		tmpPawns.Clear();
		return result;
	}

	public static float ApproxDaysWorthOfFoodLeftAfterTransfer(List<TransferableOneWay> transferables, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, WorldPath path = null, float nextTileCostLeft = 0f, int caravanTicksPerMove = 3300)
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
					if (pawn.RaceProps.EatsFood && pawn.needs?.food != null)
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

	public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<Thing> potentiallyFood, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
	{
		tmpThingDefCounts.Clear();
		tmpPawns.Clear();
		for (int i = 0; i < pawns.Count; i++)
		{
			Pawn pawn = pawns[i];
			if (pawn.RaceProps.EatsFood && pawn.needs?.food != null)
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

	public static float ApproxDaysWorthOfFood(List<Pawn> pawns, List<ThingCount> potentiallyFood, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
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

	public static float ApproxDaysWorthOfFoodLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, PlanetTile tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction)
	{
		tmpThingCounts.Clear();
		TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingCounts);
		tmpPawns.Clear();
		tmpThingDefCounts.Clear();
		for (int num = tmpThingCounts.Count - 1; num >= 0; num--)
		{
			if (tmpThingCounts[num].Count > 0)
			{
				if (tmpThingCounts[num].Thing is Pawn pawn)
				{
					if (pawn.RaceProps.EatsFood && pawn.needs?.food != null)
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

	public static bool AnyFoodEatingPawn(List<Pawn> pawns)
	{
		int i = 0;
		for (int count = pawns.Count; i < count; i++)
		{
			if (pawns[i].RaceProps.EatsFood && pawns[i].needs?.food != null)
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
