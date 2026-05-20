using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_Warden_Feed : WorkGiver_Warden
{
	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!ShouldTakeCareOfPrisoner(pawn, t))
		{
			return null;
		}
		Pawn pawn2 = (Pawn)t;
		if (!WardenFeedUtility.ShouldBeFed(pawn2))
		{
			return null;
		}
		if (pawn2.needs.food == null)
		{
			return null;
		}
		if (pawn2.needs.food.CurLevelPercentage >= pawn2.needs.food.PercentageThreshHungry + 0.02f)
		{
			return null;
		}
		if (pawn2.foodRestriction != null)
		{
			FoodPolicy currentRespectedRestriction = pawn2.foodRestriction.GetCurrentRespectedRestriction(pawn);
			if (currentRespectedRestriction != null && currentRespectedRestriction.filter.AllowedDefCount == 0)
			{
				JobFailReason.Is("NoFoodMatchingRestrictions".Translate());
				return null;
			}
		}
		if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out var foodSource, out var foodDef, canRefillDispenser: false, canUseInventory: true, canUsePackAnimalInventory: false, allowForbidden: false, allowCorpse: false))
		{
			JobFailReason.Is("NoFood".Translate());
			return null;
		}
		float nutrition = FoodUtility.GetNutrition(pawn2, foodSource, foodDef);
		Job job = JobMaker.MakeJob(JobDefOf.FeedPatient, foodSource, pawn2);
		job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
		return job;
	}

	public override string JobInfo(Pawn pawn, Job job)
	{
		if (FoodUtility.MoodFromIngesting((Pawn)(Thing)job.targetB, job.targetA.Thing, FoodUtility.GetFinalIngestibleDef(job.targetA.Thing)) < 0f)
		{
			return string.Format("({0})", "WarningFoodDisliked".Translate());
		}
		return string.Empty;
	}
}
