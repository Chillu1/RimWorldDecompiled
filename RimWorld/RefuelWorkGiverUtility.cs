using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld;

public static class RefuelWorkGiverUtility
{
	public static bool CanRefuel(Pawn pawn, Thing t, bool forced = false)
	{
		CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
		if (compRefuelable == null || compRefuelable.parent.Fogged() || compRefuelable.IsFull || (!forced && !compRefuelable.allowAutoRefuel))
		{
			return false;
		}
		if (compRefuelable.FuelPercentOfMax > 0f && !compRefuelable.Props.allowRefuelIfNotEmpty)
		{
			return false;
		}
		if (!forced && !compRefuelable.ShouldAutoRefuelNow)
		{
			return false;
		}
		if (!pawn.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (t.Faction != pawn.Faction)
		{
			return false;
		}
		if (t.TryGetComp(out CompInteractable comp) && comp.Props.cooldownPreventsRefuel && comp.OnCooldown)
		{
			JobFailReason.Is(comp.Props.onCooldownString.CapitalizeFirst());
			return false;
		}
		if (FindBestFuel(pawn, t) == null)
		{
			ThingFilter fuelFilter = compRefuelable.Props.fuelFilter;
			JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter.Summary));
			return false;
		}
		if (compRefuelable.Props.atomicFueling && FindAllFuel(pawn, t) == null)
		{
			ThingFilter fuelFilter2 = compRefuelable.Props.fuelFilter;
			JobFailReason.Is("NoFuelToRefuel".Translate(fuelFilter2.Summary));
			return false;
		}
		return true;
	}

	public static Job RefuelJob(Pawn pawn, Thing t, bool forced = false, JobDef customRefuelJob = null, JobDef customAtomicRefuelJob = null)
	{
		if (!t.TryGetComp<CompRefuelable>().Props.atomicFueling)
		{
			Thing thing = FindBestFuel(pawn, t);
			return JobMaker.MakeJob(customRefuelJob ?? JobDefOf.Refuel, t, thing);
		}
		List<Thing> source = FindAllFuel(pawn, t);
		Job job = JobMaker.MakeJob(customAtomicRefuelJob ?? JobDefOf.RefuelAtomic, t);
		job.targetQueueB = source.Select((Thing f) => new LocalTargetInfo(f)).ToList();
		return job;
	}

	private static Thing FindBestFuel(Pawn pawn, Thing refuelable)
	{
		ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
		return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
		bool Validator(Thing x)
		{
			if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			if (!filter.Allows(x))
			{
				return false;
			}
			return true;
		}
	}

	private static List<Thing> FindAllFuel(Pawn pawn, Thing refuelable)
	{
		int fuelCountToFullyRefuel = refuelable.TryGetComp<CompRefuelable>().GetFuelCountToFullyRefuel();
		ThingFilter filter = refuelable.TryGetComp<CompRefuelable>().Props.fuelFilter;
		return FindEnoughReservableThings(pawn, refuelable.Position, new IntRange(fuelCountToFullyRefuel, fuelCountToFullyRefuel), (Thing t) => filter.Allows(t));
	}

	public static List<Thing> FindEnoughReservableThings(Pawn pawn, IntVec3 rootCell, IntRange desiredQuantity, Predicate<Thing> validThing)
	{
		Region region = rootCell.GetRegion(pawn.Map);
		TraverseParms traverseParams = TraverseParms.For(pawn);
		List<Thing> chosenThings = new List<Thing>();
		int accumulatedQuantity = 0;
		ThingListProcessor(rootCell.GetThingList(region.Map), region);
		if (accumulatedQuantity < desiredQuantity.max)
		{
			RegionTraverser.BreadthFirstTraverse(region, EntryCondition, RegionProcessor, 99999);
		}
		if (accumulatedQuantity >= desiredQuantity.min)
		{
			return chosenThings;
		}
		return null;
		bool EntryCondition(Region from, Region r)
		{
			return r.Allows(traverseParams, isDestination: false);
		}
		bool RegionProcessor(Region r)
		{
			List<Thing> things = r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver));
			return ThingListProcessor(things, r);
		}
		bool ThingListProcessor(List<Thing> things, Region region2)
		{
			for (int i = 0; i < things.Count; i++)
			{
				Thing thing = things[i];
				if (Validator(thing) && !chosenThings.Contains(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, region2, PathEndMode.ClosestTouch, pawn))
				{
					chosenThings.Add(thing);
					accumulatedQuantity += thing.stackCount;
					if (accumulatedQuantity >= desiredQuantity.max)
					{
						return true;
					}
				}
			}
			return false;
		}
		bool Validator(Thing x)
		{
			if (x.Fogged() || x.IsForbidden(pawn) || !pawn.CanReserve(x))
			{
				return false;
			}
			if (!validThing(x))
			{
				return false;
			}
			return true;
		}
	}
}
