using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld;

public abstract class WorkGiver_ConstructDeliverResources : WorkGiver_Scanner
{
	private static readonly List<Thing> resourcesAvailable = new List<Thing>();

	private static readonly Dictionary<ThingDef, int> missingResources = new Dictionary<ThingDef, int>();

	private const float MultiPickupRadius = 5f;

	private const float NearbyConstructScanRadius = 8f;

	protected static string ForbiddenLowerTranslated;

	protected static string NoPathTranslated;

	public override Danger MaxPathDanger(Pawn pawn)
	{
		return Danger.Deadly;
	}

	public static void ResetStaticData()
	{
		ForbiddenLowerTranslated = "ForbiddenLower".Translate();
		NoPathTranslated = "NoPath".Translate();
	}

	private static bool ResourceValidator(Pawn pawn, ThingDefCountClass need, Thing th)
	{
		if (th.def != need.thingDef)
		{
			return false;
		}
		if (th.IsForbidden(pawn))
		{
			return false;
		}
		if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, th, forced: false))
		{
			return false;
		}
		return true;
	}

	private bool CanUseCarriedResource(Pawn pawn, IConstructible c, ThingDefCountClass need)
	{
		if (pawn.carryTracker.CarriedThing?.def != need.thingDef)
		{
			return false;
		}
		if (!KeyBindingDefOf.QueueOrder.IsDownEvent)
		{
			return true;
		}
		if (pawn.CurJob != null && !IsValidJob(pawn.CurJob))
		{
			return false;
		}
		foreach (QueuedJob item in pawn.jobs.jobQueue)
		{
			if (!IsValidJob(item.job))
			{
				return false;
			}
		}
		return true;
		bool IsValidJob(Job job)
		{
			if (job.def != JobDefOf.HaulToContainer)
			{
				return true;
			}
			return job.targetA != pawn.carryTracker.CarriedThing;
		}
	}

	protected Job ResourceDeliverJobFor(Pawn pawn, IConstructible c, bool canRemoveExistingFloorUnderNearbyNeeders = true, bool forced = false)
	{
		if (c is Blueprint_Install install)
		{
			return InstallJob(pawn, install);
		}
		missingResources.Clear();
		foreach (ThingDefCountClass need in c.TotalMaterialCost())
		{
			int num = ((forced || !(c is IHaulEnroute enroute)) ? c.ThingCountNeeded(need.thingDef) : enroute.GetSpaceRemainingWithEnroute(need.thingDef, pawn));
			if (num <= 0)
			{
				continue;
			}
			if (!pawn.Map.itemAvailability.ThingsAvailableAnywhere(need.thingDef, num, pawn))
			{
				missingResources.Add(need.thingDef, num);
				if (FloatMenuMakerMap.makingFor != pawn)
				{
					break;
				}
				continue;
			}
			Thing foundRes;
			if (CanUseCarriedResource(pawn, c, need))
			{
				foundRes = pawn.carryTracker.CarriedThing;
			}
			else
			{
				foundRes = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(need.thingDef), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing r) => ResourceValidator(pawn, need, r));
			}
			if (foundRes == null)
			{
				missingResources.Add(need.thingDef, num);
				if (FloatMenuMakerMap.makingFor != pawn)
				{
					break;
				}
				continue;
			}
			FindAvailableNearbyResources(foundRes, pawn, out var resTotalAvailable);
			int neededTotal;
			Job jobToMakeNeederAvailable;
			HashSet<Thing> hashSet = FindNearbyNeeders(pawn, need.thingDef, c, num, resTotalAvailable, canRemoveExistingFloorUnderNearbyNeeders, out neededTotal, out jobToMakeNeederAvailable);
			if (jobToMakeNeederAvailable != null)
			{
				return jobToMakeNeederAvailable;
			}
			hashSet.Add((Thing)c);
			Thing thing;
			if (hashSet.Count > 0)
			{
				thing = hashSet.MinBy((Thing needer) => IntVec3Utility.ManhattanDistanceFlat(foundRes.Position, needer.Position));
				hashSet.Remove(thing);
			}
			else
			{
				thing = (Thing)c;
			}
			int num2 = 0;
			int num3 = 0;
			do
			{
				num2 += resourcesAvailable[num3].stackCount;
				num2 = Mathf.Min(num2, Mathf.Min(resTotalAvailable, neededTotal));
				num3++;
			}
			while (num2 < neededTotal && num2 < resTotalAvailable && num3 < resourcesAvailable.Count);
			resourcesAvailable.RemoveRange(num3, resourcesAvailable.Count - num3);
			resourcesAvailable.Remove(foundRes);
			Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer);
			job.targetA = foundRes;
			job.targetQueueA = new List<LocalTargetInfo>();
			for (num3 = 0; num3 < resourcesAvailable.Count; num3++)
			{
				job.targetQueueA.Add(resourcesAvailable[num3]);
			}
			job.targetC = (Thing)c;
			job.targetB = thing;
			if (hashSet.Count > 0)
			{
				job.targetQueueB = new List<LocalTargetInfo>();
				foreach (Thing item in hashSet)
				{
					job.targetQueueB.Add(item);
				}
			}
			job.count = num2;
			job.haulMode = HaulMode.ToContainer;
			return job;
		}
		if (missingResources.Count > 0 && FloatMenuMakerMap.makingFor == pawn)
		{
			JobFailReason.Is("MissingMaterials".Translate(missingResources.Select((KeyValuePair<ThingDef, int> kvp) => $"{kvp.Value}x {kvp.Key.label}").ToCommaList()));
		}
		return null;
	}

	private void FindAvailableNearbyResources(Thing firstFoundResource, Pawn pawn, out int resTotalAvailable)
	{
		int num = pawn.carryTracker.MaxStackSpaceEver(firstFoundResource.def);
		resTotalAvailable = 0;
		resourcesAvailable.Clear();
		resourcesAvailable.Add(firstFoundResource);
		resTotalAvailable += firstFoundResource.stackCount;
		if (resTotalAvailable < num)
		{
			foreach (Thing item in GenRadial.RadialDistinctThingsAround(firstFoundResource.PositionHeld, firstFoundResource.MapHeld, 5f, useCenter: false))
			{
				if (resTotalAvailable >= num)
				{
					resTotalAvailable = num;
					break;
				}
				if (item.def == firstFoundResource.def && GenAI.CanUseItemForWork(pawn, item))
				{
					resourcesAvailable.Add(item);
					resTotalAvailable += item.stackCount;
				}
			}
		}
		resTotalAvailable = Mathf.Min(resTotalAvailable, num);
	}

	private HashSet<Thing> FindNearbyNeeders(Pawn pawn, ThingDef stuff, IConstructible c, int resNeeded, int resTotalAvailable, bool canRemoveExistingFloorUnderNearbyNeeders, out int neededTotal, out Job jobToMakeNeederAvailable)
	{
		neededTotal = resNeeded;
		HashSet<Thing> hashSet = new HashSet<Thing>();
		Thing thing = (Thing)c;
		foreach (Thing item in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 8f, useCenter: true))
		{
			if (neededTotal >= resTotalAvailable)
			{
				break;
			}
			if (IsNewValidNearbyNeeder(item, hashSet, c, pawn) && (!(item is Blueprint blue) || !ShouldRemoveExistingFloorFirst(pawn, blue)))
			{
				int num = 0;
				if (item is IHaulEnroute enroute)
				{
					num = enroute.GetSpaceRemainingWithEnroute(stuff, pawn);
				}
				else if (item is IConstructible constructible)
				{
					num = constructible.ThingCountNeeded(stuff);
				}
				if (num > 0)
				{
					hashSet.Add(item);
					neededTotal += num;
				}
			}
		}
		if (c is Blueprint blueprint && blueprint.def.entityDefToBuild is TerrainDef && canRemoveExistingFloorUnderNearbyNeeders && neededTotal < resTotalAvailable)
		{
			foreach (Thing item2 in GenRadial.RadialDistinctThingsAround(thing.Position, thing.Map, 3f, useCenter: false))
			{
				if (IsNewValidNearbyNeeder(item2, hashSet, c, pawn) && item2 is Blueprint blue2)
				{
					Job job = RemoveExistingFloorJob(pawn, blue2);
					if (job != null)
					{
						jobToMakeNeederAvailable = job;
						return hashSet;
					}
				}
			}
		}
		jobToMakeNeederAvailable = null;
		return hashSet;
	}

	private bool IsNewValidNearbyNeeder(Thing t, HashSet<Thing> nearbyNeeders, IConstructible constructible, Pawn pawn)
	{
		if (t is IConstructible && t != constructible && t.Faction == pawn.Faction && t.Isnt<Blueprint_Install>() && !nearbyNeeders.Contains(t) && !t.IsForbidden(pawn))
		{
			return GenConstruct.CanConstruct(t, pawn, checkSkills: false, forced: false, JobDefOf.HaulToContainer);
		}
		return false;
	}

	protected static bool ShouldRemoveExistingFloorFirst(Pawn pawn, Blueprint blue)
	{
		if (blue.def.entityDefToBuild is TerrainDef)
		{
			return pawn.Map.terrainGrid.CanRemoveTopLayerAt(blue.Position);
		}
		return false;
	}

	protected Job RemoveExistingFloorJob(Pawn pawn, Blueprint blue)
	{
		if (!ShouldRemoveExistingFloorFirst(pawn, blue))
		{
			return null;
		}
		if (!pawn.CanReserve(blue.Position, 1, -1, ReservationLayerDefOf.Floor))
		{
			return null;
		}
		if (pawn.WorkTypeIsDisabled(WorkGiverDefOf.ConstructRemoveFloors.workType))
		{
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.RemoveFloor, blue.Position);
		job.ignoreDesignations = true;
		return job;
	}

	private Job InstallJob(Pawn pawn, Blueprint_Install install)
	{
		Thing miniToInstallOrBuildingToReinstall = install.MiniToInstallOrBuildingToReinstall;
		IThingHolder parentHolder = miniToInstallOrBuildingToReinstall.ParentHolder;
		if (parentHolder != null && parentHolder is Pawn_CarryTracker pawn_CarryTracker)
		{
			JobFailReason.Is("BeingCarriedBy".Translate(pawn_CarryTracker.pawn));
			return null;
		}
		if (miniToInstallOrBuildingToReinstall.IsForbidden(pawn))
		{
			JobFailReason.Is(ForbiddenLowerTranslated);
			return null;
		}
		if (!pawn.CanReach(miniToInstallOrBuildingToReinstall, PathEndMode.ClosestTouch, pawn.NormalMaxDanger()))
		{
			JobFailReason.Is(NoPathTranslated);
			return null;
		}
		if (!pawn.CanReserve(miniToInstallOrBuildingToReinstall))
		{
			Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(miniToInstallOrBuildingToReinstall, pawn);
			if (pawn2 != null)
			{
				JobFailReason.Is("ReservedBy".Translate(pawn2.LabelShort, pawn2));
			}
			return null;
		}
		Job job = JobMaker.MakeJob(JobDefOf.HaulToContainer);
		job.targetA = miniToInstallOrBuildingToReinstall;
		job.targetB = install;
		job.count = 1;
		job.haulMode = HaulMode.ToContainer;
		return job;
	}
}
