using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class RefuelWorkGiverUtility
	{
		public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			if (compRefuelable == null || compRefuelable.IsFull || (!forced && !compRefuelable.allowAutoRefuel))
			{
				return false;
			}
			if (!forced && !compRefuelable.ShouldAutoRefuelNow)
			{
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (t.Faction != pawn.Faction)
			{
				return false;
			}
			if (FindBestFuel(pawn, t) == null)
			{
				ThingFilter fuelFilter = t.TryGetComp<CompRefuelable>().Props.fuelFilter;
				JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary));
				return false;
			}
			if (t.TryGetComp<CompRefuelable>().Props.atomicFueling && FindAllFuel(pawn, t) == null)
			{
				ThingFilter fuelFilter2 = t.TryGetComp<CompRefuelable>().Props.fuelFilter;
				JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter2.Summary));
				return false;
			}
			return true;
		}

		public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null, JobDef customAtomicRefuelJob = null)
		{
			if (!t.TryGetComp<CompRefuelable>().Props.atomicFueling)
			{
				Thing t2 = FindBestFuel(pawn, t);
				return JobMaker.MakeJob(customRefuelJob ?? JobDefOf.Refuel, t, t2);
			}
			List<Thing> source = FindAllFuel(pawn, t);
			Job job = JobMaker.MakeJob(customAtomicRefuelJob ?? JobDefOf.RefuelAtomic, t);
			job.targetQueueB = source.Select((Thing f) => new LocalTargetInfo(f)).ToList();
			return job;
		}

		private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
		{
			ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
			Predicate<Thing> validator = delegate(Thing x)
			{
				if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
				{
					return false;
				}
				return filter.Allows(x) ? true : false;
			};
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
		}

		private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
		{
			int quantity = refuelable.TryGetComp<CompRefuelable>().GetFuelCountToFullyRefuel();
			ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
			Predicate<Thing> validator = delegate(Thing x)
			{
				if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
				{
					return false;
				}
				return filter.Allows(x) ? true : false;
			};
			Region region = refuelable.Position.GetRegion(pawn.Map);
			TraverseParms traverseParams = TraverseParms.For(pawn);
			RegionEntryPredicate entryCondition = (Region from, Region r) => r.Allows(traverseParams, isDestination: false);
			List<Thing> chosenThings = new List<Thing>();
			int accumulatedQuantity = 0;
			RegionProcessor regionProcessor = delegate(Region r)
			{
				List<Thing> list = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (validator(thing) && !chosenThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, r, PathEndMode.ClosestTouch, pawn))
					{
						chosenThings.Add(thing);
						accumulatedQuantity += thing.stackCount;
						if (accumulatedQuantity >= quantity)
						{
							return true;
						}
					}
				}
				return false;
			};
			RegionTraverser.BreadthFirstTraverse(region, entryCondition, regionProcessor, 99999);
			if (accumulatedQuantity >= quantity)
			{
				return chosenThings;
			}
			return null;
		}
	}
}
