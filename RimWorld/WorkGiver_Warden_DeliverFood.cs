using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_DeliverFood : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner_NewTemp(pawn, t, forced))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (!pawn2.guest.CanBeBroughtFood)
			{
				return null;
			}
			if (!pawn2.Position.IsInPrisonCell(pawn2.Map))
			{
				return null;
			}
			if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
			{
				return null;
			}
			if (WardenFeedUtility.ShouldBeFed(pawn2))
			{
				return null;
			}
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, canRefillDispenser: false, canUseInventory: true, allowForbidden: false, allowCorpse: false))
			{
				return null;
			}
			if (foodSource.GetRoom() == pawn2.GetRoom())
			{
				return null;
			}
			if (FoodAvailableInRoomTo(pawn2))
			{
				return null;
			}
			float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
			Job job = JobMaker.MakeJob(JobDefOf.DeliverFood, foodSource, pawn2);
			job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
			job.targetC = RCellFinder.SpotToChewStandingNear(pawn2, foodSource);
			return job;
		}

		private static bool FoodAvailableInRoomTo(Pawn prisoner)
		{
			if (prisoner.carryTracker.CarriedThing != null && NutritionAvailableForFrom(prisoner, prisoner.carryTracker.CarriedThing) > 0f)
			{
				return true;
			}
			float num = 0f;
			float num2 = 0f;
			Room room = prisoner.GetRoom();
			if (room == null)
			{
				return false;
			}
			for (int i = 0; i < room.RegionCount; i++)
			{
				Region region = room.Regions[i];
				List<Thing> list = region.ListerThings.ThingsInGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);
				for (int j = 0; j < list.Count; j++)
				{
					Thing thing = list[j];
					if (!thing.def.IsIngestible || (int)thing.def.ingestible.preferability > 3)
					{
						num2 += NutritionAvailableForFrom(prisoner, thing);
					}
				}
				List<Thing> list2 = region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
				for (int k = 0; k < list2.Count; k++)
				{
					Pawn pawn = list2[k] as Pawn;
					if (pawn.IsPrisonerOfColony && pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry + 0.02f && (pawn.carryTracker.CarriedThing == null || !pawn.WillEat(pawn.carryTracker.CarriedThing)))
					{
						num += pawn.needs.food.NutritionWanted;
					}
				}
			}
			if (num2 + 0.5f >= num)
			{
				return true;
			}
			return false;
		}

		private static float NutritionAvailableForFrom(Pawn p, Thing foodSource)
		{
			if (foodSource.def.IsNutritionGivingIngestible && p.WillEat(foodSource))
			{
				return foodSource.GetStatValue(StatDefOf.Nutrition) * (float)foodSource.stackCount;
			}
			if (p.RaceProps.ToolUser && p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
			{
				Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
				if (building_NutrientPasteDispenser != null && building_NutrientPasteDispenser.CanDispenseNow && p.CanReach(building_NutrientPasteDispenser.InteractionCell, PathEndMode.OnCell, Danger.Some))
				{
					return 99999f;
				}
			}
			return 0f;
		}
	}
}
