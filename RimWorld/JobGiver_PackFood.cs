using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_PackFood : ThinkNode_JobGiver
	{
		private const float MaxInvNutritionToConsiderLookingForFood = 0.4f;

		private const float MinFinalInvNutritionToPickUp = 0.8f;

		private const float MinNutritionPerColonistToDo = 1.5f;

		public const FoodPreferability MinFoodPreferability = FoodPreferability.MealAwful;

		protected override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.inventory == null)
			{
				return null;
			}
			float invNutrition = GetInventoryPackableFoodNutrition(pawn);
			if (invNutrition > 0.4f)
			{
				return null;
			}
			if (pawn.Map.resourceCounter.TotalHumanEdibleNutrition < (float)pawn.Map.mapPawns.ColonistsSpawnedCount * 1.5f)
			{
				return null;
			}
			Thing thing = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 20f, delegate(Thing t)
			{
				if (!IsGoodPackableFoodFor(t, pawn) || t.IsForbidden(pawn) || !pawn.CanReserve(t) || !t.IsSociallyProper(pawn))
				{
					return false;
				}
				if (invNutrition + t.GetStatValue(StatDefOf.Nutrition) * (float)t.stackCount < 0.8f)
				{
					return false;
				}
				List<ThoughtDef> list = FoodUtility.ThoughtsFromIngesting(pawn, t, FoodUtility.GetFinalIngestibleDef(t));
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].stages[0].baseMoodEffect < 0f)
					{
						return false;
					}
				}
				return true;
			}, (Thing x) => FoodUtility.FoodOptimality(pawn, x, FoodUtility.GetFinalIngestibleDef(x), 0f));
			if (thing == null)
			{
				return null;
			}
			int a = Mathf.FloorToInt((pawn.needs.food.MaxLevel - invNutrition) / thing.GetStatValue(StatDefOf.Nutrition));
			a = Mathf.Min(a, thing.stackCount);
			a = Mathf.Max(a, 1);
			Job job = JobMaker.MakeJob(JobDefOf.TakeInventory, thing);
			job.count = a;
			return job;
		}

		private float GetInventoryPackableFoodNutrition(Pawn pawn)
		{
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			float num = 0f;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				if (IsGoodPackableFoodFor(innerContainer[i], pawn))
				{
					num += innerContainer[i].GetStatValue(StatDefOf.Nutrition) * (float)innerContainer[i].stackCount;
				}
			}
			return num;
		}

		private bool IsGoodPackableFoodFor(Thing food, Pawn forPawn)
		{
			if (food.def.IsNutritionGivingIngestible && food.def.EverHaulable && (int)food.def.ingestible.preferability >= 6)
			{
				return forPawn.WillEat(food, null, careIfNotAcceptableForTitle: false);
			}
			return false;
		}
	}
}
