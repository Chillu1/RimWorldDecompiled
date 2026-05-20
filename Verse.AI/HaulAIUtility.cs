using System.Collections.Generic;
using RimWorld;
using UnityEngine;

namespace Verse.AI;

public static class HaulAIUtility
{
	private static string ForbiddenLowerTrans;

	private static string ForbiddenOutsideAllowedAreaLowerTrans;

	private static string ReservedForPrisonersTrans;

	private static string BurningLowerTrans;

	public static string NoEmptyPlaceLowerTrans;

	public static string ContainerFullLowerTrans;

	private static List<IntVec3> candidates = new List<IntVec3>();

	public static void Reset()
	{
		ForbiddenLowerTrans = "ForbiddenLower".Translate();
		ForbiddenOutsideAllowedAreaLowerTrans = "ForbiddenOutsideAllowedAreaLower".Translate();
		ReservedForPrisonersTrans = "ReservedForPrisoners".Translate();
		BurningLowerTrans = "BurningLower".Translate();
		NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();
		ContainerFullLowerTrans = "ContainerFull".Translate();
	}

	public static bool IsInHaulableInventory(Thing thing)
	{
		if (thing.Spawned)
		{
			return false;
		}
		return thing.ParentHolder is IHaulSource;
	}

	public static bool PawnCanAutomaticallyHaul(Pawn p, Thing t, bool forced)
	{
		if (!t.def.EverHaulable)
		{
			return false;
		}
		if (t.Position.Fogged(t.Map))
		{
			return false;
		}
		if (t.IsForbidden(p))
		{
			if (!t.Position.InAllowedArea(p))
			{
				JobFailReason.Is(ForbiddenOutsideAllowedAreaLowerTrans + "(" + p.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap.Label + ")");
			}
			else
			{
				JobFailReason.Is(ForbiddenLowerTrans);
			}
			return false;
		}
		if (!t.def.alwaysHaulable && t.Map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInValidStorage())
		{
			return false;
		}
		if (!PawnCanAutomaticallyHaulFast(p, t, forced))
		{
			return false;
		}
		return true;
	}

	public static bool PawnCanAutomaticallyHaulFast(Pawn p, Thing t, bool forced)
	{
		if (t.Fogged())
		{
			return false;
		}
		if (!p.CanReserve(t, 1, -1, null, forced))
		{
			return false;
		}
		if (!p.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			return false;
		}
		if (t is UnfinishedThing { BoundBill: not null } unfinishedThing && unfinishedThing.BoundBill.billStack.FirstShouldDoNow == unfinishedThing.BoundBill && (!(unfinishedThing.BoundBill.billStack.billGiver is Building building) || (building.Spawned && building.OccupiedRect().ExpandedBy(1).Contains(unfinishedThing.Position))))
		{
			return false;
		}
		using (ProfilerBlock.Scope("CanReach"))
		{
			if (!p.CanReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger()))
			{
				return false;
			}
		}
		if (t.def.IsNutritionGivingIngestible && t.def.ingestible.HumanEdible && !t.IsSociallyProper(p, forPrisoner: false, animalsCare: true))
		{
			JobFailReason.Is(ReservedForPrisonersTrans);
			return false;
		}
		if (t.IsBurning())
		{
			JobFailReason.Is(BurningLowerTrans);
			return false;
		}
		return true;
	}

	public static Job HaulToStorageJob(Pawn p, Thing t, bool forced)
	{
		StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(t, forced);
		if (!StoreUtility.TryFindBestBetterStorageFor(t, p, p.Map, currentPriority, p.Faction, out var foundCell, out var haulDestination))
		{
			JobFailReason.Is(NoEmptyPlaceLowerTrans);
			return null;
		}
		if (haulDestination is ISlotGroupParent)
		{
			return HaulToCellStorageJob(p, t, foundCell, fitInStoreCell: false);
		}
		if (haulDestination is Thing thing && thing.TryGetInnerInteractableThingOwner() != null)
		{
			return HaulToContainerJob(p, t, thing);
		}
		Log.Error("Don't know how to handle HaulToStorageJob for storage " + haulDestination.ToStringSafe() + ". thing=" + t.ToStringSafe());
		return null;
	}

	public static Job HaulToCellStorageJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
	{
		Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
		ISlotGroup slotGroup = p.Map.haulDestinationManager.SlotGroupAt(storeCell);
		ISlotGroup storageGroup = slotGroup.StorageGroup;
		ISlotGroup obj = storageGroup ?? slotGroup;
		if (p.Map.thingGrid.ThingAt(storeCell, t.def) != null)
		{
			if (fitInStoreCell)
			{
				job.count = storeCell.GetItemStackSpaceLeftFor(p.Map, t.def);
			}
			else
			{
				job.count = t.def.stackLimit;
			}
		}
		else
		{
			job.count = 99999;
		}
		int num = 0;
		float statValue = p.GetStatValue(StatDefOf.CarryingCapacity);
		List<IntVec3> cellsList = obj.CellsList;
		for (int i = 0; i < cellsList.Count; i++)
		{
			if (StoreUtility.IsGoodStoreCell(cellsList[i], p.Map, t, p, p.Faction))
			{
				num += cellsList[i].GetItemStackSpaceLeftFor(p.Map, t.def);
				if (num >= job.count || (float)num >= statValue)
				{
					break;
				}
			}
		}
		job.count = Mathf.Min(job.count, num);
		job.haulOpportunisticDuplicates = true;
		job.haulMode = HaulMode.ToCellStorage;
		return job;
	}

	public static Job HaulToContainerJob(Pawn p, Thing t, Thing container)
	{
		ThingOwner thingOwner = container.TryGetInnerInteractableThingOwner();
		if (thingOwner == null)
		{
			Log.Error(container.ToStringSafe() + " gave null ThingOwner.");
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer, t, container);
		job.count = Mathf.Min(t.stackCount, thingOwner.GetCountCanAccept(t));
		job.haulMode = HaulMode.ToContainer;
		return job;
	}

	public static bool CanHaulAside(Pawn p, Thing t, out IntVec3 storeCell)
	{
		storeCell = IntVec3.Invalid;
		if (!t.def.EverHaulable)
		{
			return false;
		}
		if (t.IsBurning())
		{
			return false;
		}
		if (!p.CanReserveAndReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger()))
		{
			return false;
		}
		if (!TryFindSpotToPlaceHaulableCloseTo(t, p, t.PositionHeld, out storeCell))
		{
			return false;
		}
		return true;
	}

	public static Job HaulAsideJobFor(Pawn p, Thing t)
	{
		if (!CanHaulAside(p, t, out var storeCell))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
		job.count = 99999;
		job.haulOpportunisticDuplicates = false;
		job.haulMode = HaulMode.ToCellNonStorage;
		job.ignoreDesignations = true;
		return job;
	}

	private static bool TryFindSpotToPlaceHaulableCloseTo(Thing haulable, Pawn worker, IntVec3 center, out IntVec3 spot)
	{
		Region region = center.GetRegion(worker.Map);
		if (region == null)
		{
			spot = center;
			return false;
		}
		TraverseParms traverseParms = TraverseParms.For(worker);
		IntVec3 foundCell = IntVec3.Invalid;
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(traverseParms, isDestination: false), delegate(Region r)
		{
			candidates.Clear();
			candidates.AddRange(r.Cells);
			candidates.Sort((IntVec3 a, IntVec3 b) => a.DistanceToSquared(center).CompareTo(b.DistanceToSquared(center)));
			for (int num = 0; num < candidates.Count; num++)
			{
				IntVec3 intVec = candidates[num];
				if (HaulablePlaceValidator(haulable, worker, intVec))
				{
					foundCell = intVec;
					return true;
				}
			}
			return false;
		}, 100);
		if (foundCell.IsValid)
		{
			spot = foundCell;
			return true;
		}
		spot = center;
		return false;
	}

	private static bool HaulablePlaceValidator(Thing haulable, Pawn worker, IntVec3 c)
	{
		if (c.IsForbidden(worker))
		{
			return false;
		}
		if (!worker.CanReserveAndReach(c, PathEndMode.OnCell, worker.NormalMaxDanger()))
		{
			return false;
		}
		if (GenPlace.HaulPlaceBlockerIn(haulable, c, worker.Map, checkBlueprintsAndFrames: true) != null)
		{
			return false;
		}
		if (!c.Standable(worker.Map))
		{
			return false;
		}
		if (c == haulable.Position && haulable.Spawned)
		{
			return false;
		}
		if (c.ContainsStaticFire(worker.Map))
		{
			return false;
		}
		if (haulable != null && haulable.def.BlocksPlanting() && worker.Map.zoneManager.ZoneAt(c) is Zone_Growing)
		{
			return false;
		}
		if (haulable.def.passability != Traversability.Standable)
		{
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (worker.Map.designationManager.DesignationAt(c2, DesignationDefOf.Mine) != null || worker.Map.designationManager.DesignationAt(c2, DesignationDefOf.MineVein) != null)
				{
					return false;
				}
			}
		}
		Building edifice = c.GetEdifice(worker.Map);
		if (edifice != null && edifice is Building_Trap)
		{
			return false;
		}
		if (haulable is UnfinishedThing { BoundWorkTable: not null } unfinishedThing)
		{
			if (unfinishedThing.BoundWorkTable.InteractionCell == c)
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(haulable.Map);
			for (int j = 0; j < thingList.Count; j++)
			{
				if (unfinishedThing.BoundWorkTable == thingList[j])
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void UpdateJobWithPlacedThings(Job curJob, Thing th, int added)
	{
		if (curJob.placedThings == null)
		{
			curJob.placedThings = new List<ThingCountClass>();
		}
		ThingCountClass thingCountClass = curJob.placedThings.Find((ThingCountClass x) => x.thing == th);
		if (thingCountClass != null)
		{
			thingCountClass.Count += added;
		}
		else
		{
			curJob.placedThings.Add(new ThingCountClass(th, added));
		}
	}

	public static List<Thing> FindFixedIngredientCount(Pawn pawn, ThingDef def, int maxCount)
	{
		Region region = pawn.Position.GetRegion(pawn.Map);
		List<Thing> chosenThings = new List<Thing>();
		int countFound = 0;
		ThingRequest thingRequest = ThingRequest.ForDef(def);
		ThingListProcessor(pawn.Position.GetThingList(region.Map), region);
		if (countFound < maxCount)
		{
			RegionTraverser.BreadthFirstTraverse(region, (Region from, Region r) => r.Allows(TraverseParms.For(pawn), isDestination: false), (Region r) => ThingListProcessor(r.ListerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.HaulableEver)), r), 99999);
		}
		return chosenThings;
		bool ThingListProcessor(List<Thing> things, Region region2)
		{
			for (int i = 0; i < things.Count; i++)
			{
				Thing thing = things[i];
				if (thingRequest.Accepts(thing) && !chosenThings.Contains(thing) && !thing.IsForbidden(pawn) && pawn.CanReserve(thing) && ReachabilityWithinRegion.ThingFromRegionListerReachable(thing, region2, PathEndMode.ClosestTouch, pawn))
				{
					chosenThings.Add(thing);
					countFound += thing.stackCount;
					if (countFound >= maxCount)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
