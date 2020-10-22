using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Warden_Feed : WorkGiver_Warden
	{
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (!ShouldTakeCareOfPrisoner_NewTemp(pawn, t, forced))
			{
				return null;
			}
			Pawn pawn2 = (Pawn)t;
			if (!WardenFeedUtility.ShouldBeFed(pawn2))
			{
				return null;
			}
			if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
			{
				return null;
			}
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, canRefillDispenser: false, canUseInventory: true, allowForbidden: false, allowCorpse: false))
			{
				JobFailReason.Is("NoFood".Translate());
				return null;
			}
			float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
			Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, foodSource, pawn2);
			job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
			return job;
		}
	}
}
