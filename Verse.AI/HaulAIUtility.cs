using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse.AI
{
	public static class HaulAIUtility
	{
		private static string ForbiddenLowerTrans;

		private static string ForbiddenOutsideAllowedAreaLowerTrans;

		private static string ReservedForPrisonersTrans;

		private static string BurningLowerTrans;

		private static string NoEmptyPlaceLowerTrans;

		private static List<IntVec3> candidates = new List<IntVec3>();

		public static void Reset()
		{
			ForbiddenLowerTrans = "ForbiddenLower".Translate();
			ForbiddenOutsideAllowedAreaLowerTrans = "ForbiddenOutsideAllowedAreaLower".Translate();
			ReservedForPrisonersTrans = "ReservedForPrisoners".Translate();
			BurningLowerTrans = "BurningLower".Translate();
			NoEmptyPlaceLowerTrans = "NoEmptyPlaceLower".Translate();
		}

		public static bool PawnCanAutomaticallyHaul(Pawn p, Thing t, bool forced)
		{
			if (!t.def.EverHaulable)
			{
				return false;
			}
			if (t.IsForbidden(p))
			{
				if (!t.Position.InAllowedArea(p))
				{
					JobFailReason.Is(ForbiddenOutsideAllowedAreaLowerTrans);
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
			UnfinishedThing unfinishedThing = t as UnfinishedThing;
			Building building;
			if (unfinishedThing != null && unfinishedThing.BoundBill != null && ((building = (unfinishedThing.BoundBill.billStack.billGiver as Building)) == null || (building.Spawned && building.OccupiedRect().ExpandedBy(1).Contains(unfinishedThing.Position))))
			{
				return false;
			}
			if (!p.CanReach(t, PathEndMode.ClosestTouch, p.NormalMaxDanger()))
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

		public static Job HaulToStorageJob(Pawn p, Thing t)
		{
			StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(t);
			if (!StoreUtility.TryFindBestBetterStorageFor(t, p, p.Map, currentPriority, p.Faction, out IntVec3 foundCell, out IHaulDestination haulDestination))
			{
				JobFailReason.Is(NoEmptyPlaceLowerTrans);
				return null;
			}
			if (haulDestination is ISlotGroupParent)
			{
				return HaulToCellStorageJob(p, t, foundCell, fitInStoreCell: false);
			}
			Thing thing = haulDestination as Thing;
			if (thing != null && thing.TryGetInnerInteractableThingOwner() != null)
			{
				return HaulToContainerJob(p, t, thing);
			}
			Log.Error("Don't know how to handle HaulToStorageJob for storage " + haulDestination.ToStringSafe() + ". thing=" + t.ToStringSafe());
			return null;
		}

		public static Job HaulToCellStorageJob(Pawn p, Thing t, IntVec3 storeCell, bool fitInStoreCell)
		{
			Job job = JobMaker.MakeJob(JobDefOf.HaulToCell, t, storeCell);
			SlotGroup slotGroup = p.Map.haulDestinationManager.SlotGroupAt(storeCell);
			if (slotGroup != null)
			{
				Thing thing = p.Map.thingGrid.ThingAt(storeCell, t.def);
				if (thing != null)
				{
					job.count = t.def.stackLimit;
					if (fitInStoreCell)
					{
						job.count -= thing.stackCount;
					}
				}
				else
				{
					job.count = 99999;
				}
				int num = 0;
				float statValue = p.GetStatValue(StatDefOf.CarryingCapacity);
				List<IntVec3> cellsList = slotGroup.CellsList;
				for (int i = 0; i < cellsList.Count; i++)
				{
					if (StoreUtility.IsGoodStoreCell(cellsList[i], p.Map, t, p, p.Faction))
					{
						Thing thing2 = p.Map.thingGrid.ThingAt(cellsList[i], t.def);
						num = ((thing2 == null || thing2 == t) ? (num + t.def.stackLimit) : (num + Mathf.Max(t.def.stackLimit - thing2.stackCount, 0)));
						if (num >= job.count || (float)num >= statValue)
						{
							break;
						}
					}
				}
				job.count = Mathf.Min(job.count, num);
			}
			else
			{
				job.count = 99999;
			}
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
			if (!CanHaulAside(p, t, out IntVec3 storeCell))
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
				for (int i = 0; i < candidates.Count; i++)
				{
					IntVec3 intVec = candidates[i];
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
			if (haulable.def.passability != 0)
			{
				for (int i = 0; i < 8; i++)
				{
					IntVec3 c2 = c + GenAdj.AdjacentCells[i];
					if (worker.Map.designationManager.DesignationAt(c2, DesignationDefOf.Mine) != null)
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
			return true;
		}
	}
}
