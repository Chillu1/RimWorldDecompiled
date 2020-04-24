using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
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

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 == null || pawn2 == pawn)
			{
				return false;
			}
			if (def.feedHumanlikesOnly && !pawn2.RaceProps.Humanlike)
			{
				return false;
			}
			if (def.feedAnimalsOnly && !pawn2.RaceProps.Animal)
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
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out Thing _, out ThingDef _, canRefillDispenser: false))
			{
				JobFailReason.Is("NoFood".Translate());
				return false;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = (Pawn)t;
			if (FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2, pawn2.needs.food.CurCategory == HungerCategory.Starving, out Thing foodSource, out ThingDef foodDef, canRefillDispenser: false))
			{
				float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
				Job job = JobMaker.MakeJob(JobDefOf.FeedPatient);
				job.targetA = foodSource;
				job.targetB = pawn2;
				job.count = FoodUtility.WillIngestStackCountOf(pawn2, foodDef, nutrition);
				return job;
			}
			return null;
		}
	}
}
