using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		private HungerCategory minCategory;

		private float maxLevelPercentage = 1f;

		public bool forceScanWholeMap;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetFood obj = (JobGiver_GetFood)base.DeepCopy(resolve);
			obj.minCategory = minCategory;
			obj.maxLevelPercentage = maxLevelPercentage;
			obj.forceScanWholeMap = forceScanWholeMap;
			return obj;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if ((int)pawn.needs.food.CurCategory < 3 && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if ((int)food.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage > maxLevelPercentage)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null || (int)food.CurCategory < (int)minCategory || food.CurLevelPercentage > maxLevelPercentage)
			{
				return null;
			}
			bool allowCorpse;
			if (pawn.AnimalOrWildMan())
			{
				allowCorpse = true;
			}
			else
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
				allowCorpse = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
			}
			bool desperate = pawn.needs.food.CurCategory == HungerCategory.Starving;
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out Thing foodSource, out ThingDef foodDef, canRefillDispenser: true, canUseInventory: true, allowForbidden: false, allowCorpse, allowSociallyImproper: false, pawn.IsWildMan(), forceScanWholeMap))
			{
				return null;
			}
			Pawn pawn2 = foodSource as Pawn;
			if (pawn2 != null)
			{
				Job job = JobMaker.MakeJob(JobDefOf.PredatorHunt, pawn2);
				job.killIncappedTarget = true;
				return job;
			}
			if (foodSource is Plant && foodSource.def.plant.harvestedThingDef == foodDef)
			{
				return JobMaker.MakeJob(JobDefOf.Harvest, foodSource);
			}
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
			{
				Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
				if (building != null)
				{
					ISlotGroupParent hopperSgp = building as ISlotGroupParent;
					Job job2 = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
					if (job2 != null)
					{
						return job2;
					}
				}
				foodSource = FoodUtility.BestFoodSourceOnMap(pawn, pawn, desperate, out foodDef, FoodPreferability.MealLavish, allowPlant: false, !pawn.IsTeetotaler(), allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false, allowForbidden: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap);
				if (foodSource == null)
				{
					return null;
				}
			}
			float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
			Job job3 = JobMaker.MakeJob(JobDefOf.Ingest, foodSource);
			job3.count = FoodUtility.WillIngestStackCountOf(pawn, foodDef, nutrition);
			return job3;
		}
	}
}
