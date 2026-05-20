using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld;

public class WorkGiver_FeedPatient : WorkGiver_Scanner
{
	public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

	public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		return pawn.Map.mapPawns.SpawnedHungryPawns;
	}

	private bool TryFindBestFoodSourceFor(Pawn pawn, Pawn patient, out Thing foodSource, out ThingDef foodDef)
	{
		return FoodUtility.TryFindBestFoodSourceFor(pawn, patient, patient.needs.food.CurCategory == HungerCategory.Starving, out foodSource, out foodDef, canRefillDispenser: false, canUseInventory: true, canUsePackAnimalInventory: true, allowForbidden: false, allowCorpse: true, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap: false, ignoreReservations: false, calculateWantedStackCount: false, allowVenerated: true);
	}

	public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		if (!(t is Pawn pawn2) || pawn2 == pawn)
		{
			return false;
		}
		if (def.feedHumanlikesOnly && !pawn2.RaceProps.Humanlike)
		{
			return false;
		}
		if (def.feedAnimalsOnly && !pawn2.IsAnimal)
		{
			return false;
		}
		if (pawn2.DevelopmentalStage.Baby())
		{
			return false;
		}
		if (!FeedPatientUtility.IsHungry(pawn2))
		{
			return false;
		}
		if (!FeedPatientUtility.ShouldBeFed(pawn2))
		{
			return false;
		}
		if (WardenFeedUtility.ShouldBeFed(pawn2) && !pawn.IsColonyMech)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (pawn2.foodRestriction != null)
		{
			FoodPolicy currentRespectedRestriction = pawn2.foodRestriction.GetCurrentRespectedRestriction(pawn);
			if (currentRespectedRestriction != null && currentRespectedRestriction.filter.AllowedDefCount == 0)
			{
				JobFailReason.Is("NoFoodMatchingRestrictions".Translate());
				return false;
			}
		}
		if (!TryFindBestFoodSourceFor(pawn, pawn2, out var _, out var _))
		{
			JobFailReason.Is("NoFood".Translate());
			return false;
		}
		return true;
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
	{
		Pawn pawn2 = (Pawn)t;
		if (TryFindBestFoodSourceFor(pawn, pawn2, out var foodSource, out var foodDef))
		{
			float nutrition = FoodUtility.GetNutrition(pawn2, foodSource, foodDef);
			Job job = JobMaker.MakeJob(JobDefOf.FeedPatient);
			job.targetA = foodSource;
			job.targetB = pawn2;
			job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
			return job;
		}
		return null;
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
