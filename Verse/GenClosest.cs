using System;
using System.Collections;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse.AI;

namespace Verse;

public static class GenClosest
{
	private const int DefaultLocalTraverseRegionsBeforeGlobal = 30;

	private static bool EarlyOutSearch(IntVec3 start, Map map, ThingRequest thingReq, IEnumerable<Thing> customGlobalSearchSet, Predicate<Thing> validator)
	{
		if (thingReq.group == ThingRequestGroup.Everything)
		{
			Log.Error("Cannot do ClosestThingReachable searching everything without restriction.");
			return true;
		}
		if (!start.InBounds(map))
		{
			IntVec3 intVec = start;
			string text = intVec.ToString();
			ThingRequest thingRequest = thingReq;
			Log.Error("Did FindClosestThing with start out of bounds (" + text + "), thingReq=" + thingRequest.ToString());
			return true;
		}
		if (thingReq.group == ThingRequestGroup.Nothing)
		{
			return true;
		}
		if ((thingReq.IsUndefined || map.listerThings.ThingsMatching(thingReq).Count == 0) && customGlobalSearchSet.EnumerableNullOrEmpty())
		{
			return true;
		}
		return false;
	}

	public static Thing ClosestThingReachable(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, IEnumerable<Thing> customGlobalSearchSet = null, int searchRegionsMin = 0, int searchRegionsMax = -1, bool forceAllowGlobalSearch = false, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false, bool lookInHaulSources = false)
	{
		bool flag = searchRegionsMax < 0 || forceAllowGlobalSearch;
		if (!flag && customGlobalSearchSet != null)
		{
			Log.ErrorOnce("searchRegionsMax >= 0 && customGlobalSearchSet != null && !forceAllowGlobalSearch. customGlobalSearchSet will never be used.", 634984);
		}
		if (!flag && !thingReq.IsUndefined && !thingReq.CanBeFoundInRegion)
		{
			Log.ErrorOnce("ClosestThingReachable with thing request group " + thingReq.group.ToString() + " and global search not allowed. This will never find anything because this group is never stored in regions. Either allow global search or don't call this method at all.", 518498981);
			return null;
		}
		if (EarlyOutSearch(root, map, thingReq, customGlobalSearchSet, validator))
		{
			return null;
		}
		Thing thing = null;
		bool flag2 = false;
		if (!thingReq.IsUndefined && thingReq.CanBeFoundInRegion)
		{
			int num = ((searchRegionsMax > 0) ? searchRegionsMax : 30);
			thing = RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, null, searchRegionsMin, num, maxDistance, out var regionsSeen, traversableRegionTypes, ignoreEntirelyForbiddenRegions, lookInHaulSources);
			flag2 = thing == null && regionsSeen < num;
		}
		if (thing == null && flag && !flag2)
		{
			if (traversableRegionTypes != RegionType.Set_Passable)
			{
				Log.ErrorOnce("ClosestThingReachable had to do a global search, but traversableRegionTypes is not set to passable only. It's not supported, because Reachability is based on passable regions only.", 14384767);
			}
			IEnumerable<Thing> searchSet = customGlobalSearchSet ?? map.listerThings.ThingsMatching(thingReq);
			thing = ClosestThing_Global(root, searchSet, maxDistance, GlobalValidator);
		}
		return thing;
		bool GlobalValidator(Thing t)
		{
			if (!map.reachability.CanReach(root, t, peMode, traverseParams))
			{
				return false;
			}
			if (validator != null && !validator(t))
			{
				return false;
			}
			return true;
		}
	}

	public static Thing ClosestThing_Regionwise_ReachablePrioritized(IntVec3 root, Map map, ThingRequest thingReq, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, int minRegions = 24, int maxRegions = 30, bool lookInHaulSources = false)
	{
		if (!thingReq.IsUndefined && !thingReq.CanBeFoundInRegion)
		{
			Log.ErrorOnce("ClosestThing_Regionwise_ReachablePrioritized with thing request group " + thingReq.group.ToString() + ". This will never find anything because this group is never stored in regions. Most likely a global search should have been used.", 738476712);
			return null;
		}
		if (EarlyOutSearch(root, map, thingReq, null, validator))
		{
			return null;
		}
		if (maxRegions < minRegions)
		{
			Log.ErrorOnce("maxRegions < minRegions", 754343);
		}
		Thing result = null;
		if (!thingReq.IsUndefined)
		{
			result = RegionwiseBFSWorker(root, map, thingReq, peMode, traverseParams, validator, priorityGetter, minRegions, maxRegions, maxDistance, out var _, RegionType.Set_Passable, ignoreEntirelyForbiddenRegions: false, lookInHaulSources);
		}
		return result;
	}

	public static Thing RegionwiseBFSWorker(IntVec3 root, Map map, ThingRequest req, PathEndMode peMode, TraverseParms traverseParams, Predicate<Thing> validator, Func<Thing, float> priorityGetter, int minRegions, int maxRegions, float maxDistance, out int regionsSeen, RegionType traversableRegionTypes = RegionType.Set_Passable, bool ignoreEntirelyForbiddenRegions = false, bool lookInHaulSources = false)
	{
		regionsSeen = 0;
		if (traverseParams.mode == TraverseMode.PassAllDestroyableThings)
		{
			Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThings. Use ClosestThingGlobal.");
			return null;
		}
		if (traverseParams.mode == TraverseMode.PassAllDestroyablePlayerOwnedThings)
		{
			Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyablePlayerOwnedThings. Use ClosestThingGlobal.");
			return null;
		}
		if (traverseParams.mode == TraverseMode.PassAllDestroyableThingsNotWater)
		{
			Log.Error("RegionwiseBFSWorker with traverseParams.mode PassAllDestroyableThingsNotWater. Use ClosestThingGlobal.");
			return null;
		}
		if (!req.IsUndefined && !req.CanBeFoundInRegion)
		{
			Log.ErrorOnce("RegionwiseBFSWorker with thing request group " + req.group.ToString() + ". This group is never stored in regions. Most likely a global search should have been used.", 385766189);
			return null;
		}
		Region region = root.GetRegion(map, traversableRegionTypes);
		if (region == null)
		{
			return null;
		}
		RegionProcessorClosestThingReachable regionProcessorClosestThingReachable = SimplePool<RegionProcessorClosestThingReachable>.Get();
		regionProcessorClosestThingReachable.SetParameters(traverseParams, maxDistance, root, ignoreEntirelyForbiddenRegions, req, peMode, priorityGetter, validator, minRegions, 9999999f, 0, float.MinValue, null, lookInHaulSources);
		RegionTraverser.BreadthFirstTraverse(region, regionProcessorClosestThingReachable, maxRegions, traversableRegionTypes);
		regionsSeen = regionProcessorClosestThingReachable.regionsSeenScan;
		Thing closestThing = regionProcessorClosestThingReachable.closestThing;
		regionProcessorClosestThingReachable.Clear();
		SimplePool<RegionProcessorClosestThingReachable>.Return(regionProcessorClosestThingReachable);
		return closestThing;
	}

	public static Thing ClosestThing_Global(IntVec3 center, IEnumerable searchSet, float maxDistance = 99999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, bool lookInHaulSources = false)
	{
		if (searchSet == null)
		{
			return null;
		}
		float closestDistSquared = 2.1474836E+09f;
		Thing chosen = null;
		float bestPrio = float.MinValue;
		float maxDistanceSquared = maxDistance * maxDistance;
		if (searchSet is IList<Thing> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Process(list[i]);
			}
		}
		else if (searchSet is IList<Pawn> list2)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				Process(list2[j]);
			}
		}
		else if (searchSet is IList<Building> list3)
		{
			for (int k = 0; k < list3.Count; k++)
			{
				Process(list3[k]);
			}
		}
		else if (searchSet is IList<IAttackTarget> list4)
		{
			for (int l = 0; l < list4.Count; l++)
			{
				Process((Thing)list4[l]);
			}
		}
		else
		{
			foreach (Thing item in searchSet)
			{
				Process(item);
			}
		}
		return chosen;
		void Process(Thing t)
		{
			if (t.Spawned || HaulAIUtility.IsInHaulableInventory(t))
			{
				float num = (center - t.PositionHeld).LengthHorizontalSquared;
				if (!(num > maxDistanceSquared) && (priorityGetter != null || num < closestDistSquared))
				{
					ValidateThing(t, num);
					if (lookInHaulSources && t is IHaulSource haulSource)
					{
						ThingOwner directlyHeldThings = haulSource.GetDirectlyHeldThings();
						for (int m = 0; m < directlyHeldThings.Count; m++)
						{
							ValidateThing(directlyHeldThings[m], num);
						}
					}
				}
			}
		}
		void ValidateThing(Thing t, float distSquared)
		{
			if (validator == null || validator(t))
			{
				float num = 0f;
				if (priorityGetter != null)
				{
					num = priorityGetter(t);
					if (num < bestPrio || (Mathf.Approximately(num, bestPrio) && distSquared >= closestDistSquared))
					{
						return;
					}
				}
				chosen = t;
				closestDistSquared = distSquared;
				bestPrio = num;
			}
		}
	}

	public static Thing ClosestThing_Global_Reachable(IntVec3 center, Map map, IEnumerable<Thing> searchSet, PathEndMode peMode, TraverseParms traverseParams, float maxDistance = 9999f, Predicate<Thing> validator = null, Func<Thing, float> priorityGetter = null, bool canLookInHaulableSources = false)
	{
		if (searchSet == null)
		{
			return null;
		}
		int debug_changeCount = 0;
		int debug_scanCount = 0;
		Thing bestThing = null;
		float bestPrio = float.MinValue;
		float maxDistanceSquared = maxDistance * maxDistance;
		float closestDistSquared = 2.1474836E+09f;
		bool careAboutHaulSourceEnabled = canLookInHaulableSources && traverseParams.pawn != null && traverseParams.pawn.IsColonist;
		if (searchSet is IList<Thing> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				Process(list[i]);
			}
		}
		else if (searchSet is IList<Pawn> list2)
		{
			for (int j = 0; j < list2.Count; j++)
			{
				Process(list2[j]);
			}
		}
		else if (searchSet is IList<Building> list3)
		{
			for (int k = 0; k < list3.Count; k++)
			{
				Process(list3[k]);
			}
		}
		else
		{
			foreach (Thing item in searchSet)
			{
				Process(item);
			}
		}
		return bestThing;
		void Process(Thing t)
		{
			if (t != null && t.Spawned)
			{
				debug_scanCount++;
				float num = (center - t.PositionHeld).LengthHorizontalSquared;
				if (!(num > maxDistanceSquared) && (priorityGetter != null || num < closestDistSquared))
				{
					ValidateThing(t, num);
					if (canLookInHaulableSources && t is IHaulSource haulSource && (!careAboutHaulSourceEnabled || haulSource.HaulSourceEnabled))
					{
						ThingOwner directlyHeldThings = haulSource.GetDirectlyHeldThings();
						for (int l = 0; l < directlyHeldThings.Count; l++)
						{
							ValidateThing(directlyHeldThings[l], num);
						}
					}
				}
			}
		}
		void ValidateThing(Thing t, float distSquared)
		{
			if (map.reachability.CanReach(center, t.SpawnedParentOrMe, peMode, traverseParams) && (validator == null || validator(t)))
			{
				float num = 0f;
				if (priorityGetter != null)
				{
					num = priorityGetter(t);
					if (num < bestPrio || (Mathf.Approximately(num, bestPrio) && distSquared >= closestDistSquared))
					{
						return;
					}
				}
				bestThing = t;
				closestDistSquared = distSquared;
				bestPrio = num;
				debug_changeCount++;
			}
		}
	}
}
