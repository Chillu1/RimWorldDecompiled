using System;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_EatInGatheringArea : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			PawnDuty duty = pawn.mindState.duty;
			if (duty == null)
			{
				return null;
			}
			if ((double)pawn.needs.food.CurLevelPercentage > 0.9)
			{
				return null;
			}
			IntVec3 cell = duty.focus.Cell;
			Thing thing = FindFood(pawn, cell);
			if (thing == null)
			{
				return null;
			}
			Job job = JobMaker.MakeJob(JobDefOf.Ingest, thing);
			job.count = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.def.GetStatValueAbstract(StatDefOf.Nutrition));
			return job;
		}

		private Thing FindFood(Pawn pawn, IntVec3 gatheringSpot)
		{
			Predicate<Thing> validator = delegate(Thing x)
			{
				if (!x.IngestibleNow)
				{
					return false;
				}
				if (!x.def.IsNutritionGivingIngestible)
				{
					return false;
				}
				if (!GatheringsUtility.InGatheringArea(x.Position, gatheringSpot, pawn.Map))
				{
					return false;
				}
				if (x.def.IsDrug)
				{
					return false;
				}
				if ((int)x.def.ingestible.preferability <= 4)
				{
					return false;
				}
				if (!pawn.WillEat(x))
				{
					return false;
				}
				if (x.IsForbidden(pawn))
				{
					return false;
				}
				if (!x.IsSociallyProper(pawn))
				{
					return false;
				}
				return pawn.CanReserve(x) ? true : false;
			};
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors), 14f, validator, null, 0, 12);
		}
	}
}
