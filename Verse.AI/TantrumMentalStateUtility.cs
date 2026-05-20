using System;
using System.Collections.Generic;

namespace Verse.AI;

public static class TantrumMentalStateUtility
{
	private const int MaxRegionsToSearch = 40;

	private const int AbsoluteMinItemMarketValue = 75;

	public static bool CanSmash(Pawn pawn, Thing thing, bool skipReachabilityCheck = false, Predicate<Thing> customValidator = null, int extraMinBuildingOrItemMarketValue = 0)
	{
		if (customValidator != null)
		{
			if (!customValidator(thing))
			{
				return false;
			}
		}
		else if (!thing.def.IsBuildingArtificial && thing.def.category != ThingCategory.Item)
		{
			return false;
		}
		if (!thing.Destroyed && thing.Spawned && thing != pawn && (thing.def.category == ThingCategory.Pawn || thing.def.useHitPoints) && (thing.def.category == ThingCategory.Pawn || !thing.def.CanHaveFaction || thing.Faction == pawn.Faction) && (thing.def.category != ThingCategory.Item || !(thing.MarketValue * (float)thing.stackCount < 75f)) && (thing.def.category != ThingCategory.Pawn || !((Pawn)thing).Downed) && ((thing.def.category != ThingCategory.Item && thing.def.category != ThingCategory.Building) || !(thing.MarketValue * (float)thing.stackCount < (float)extraMinBuildingOrItemMarketValue)) && (thing.def.category != ThingCategory.Building || !thing.def.building.ai_neverTrashThis))
		{
			if (!skipReachabilityCheck)
			{
				return pawn.CanReach(thing, PathEndMode.Touch, Danger.Deadly);
			}
			return true;
		}
		return false;
	}

	public static void GetSmashableThingsNear(Pawn pawn, IntVec3 near, List<Thing> outCandidates, Predicate<Thing> customValidator = null, int extraMinBuildingOrItemMarketValue = 0, int maxDistance = 40)
	{
		outCandidates.Clear();
		if (!pawn.CanReach(near, PathEndMode.OnCell, Danger.Deadly))
		{
			return;
		}
		Region region = near.GetRegion(pawn.Map);
		if (region == null)
		{
			return;
		}
		TraverseParms traverseParams = TraverseParms.For(pawn);
		RegionTraverser.BreadthFirstTraverse(region, (Region from, Region to) => to.Allows(traverseParams, isDestination: false), delegate(Region r)
		{
			List<Thing> list = r.ListerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Position.InHorDistOf(near, maxDistance) && CanSmash(pawn, list[i], skipReachabilityCheck: true, customValidator, extraMinBuildingOrItemMarketValue))
				{
					outCandidates.Add(list[i]);
				}
			}
			List<Thing> list2 = r.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].Position.InHorDistOf(near, maxDistance) && CanSmash(pawn, list2[j], skipReachabilityCheck: true, customValidator, extraMinBuildingOrItemMarketValue))
				{
					outCandidates.Add(list2[j]);
				}
			}
			List<Thing> list3 = r.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			for (int k = 0; k < list3.Count; k++)
			{
				if (list3[k].Position.InHorDistOf(near, maxDistance) && CanSmash(pawn, list3[k], skipReachabilityCheck: true, customValidator, extraMinBuildingOrItemMarketValue))
				{
					outCandidates.Add(list3[k]);
				}
			}
			return false;
		}, 40);
	}

	public static void GetSmashableThingsIn(Room room, Pawn pawn, List<Thing> outCandidates, Predicate<Thing> customValidator = null, int extraMinBuildingOrItemMarketValue = 0)
	{
		outCandidates.Clear();
		List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
		for (int i = 0; i < containedAndAdjacentThings.Count; i++)
		{
			if (CanSmash(pawn, containedAndAdjacentThings[i], skipReachabilityCheck: false, customValidator, extraMinBuildingOrItemMarketValue))
			{
				outCandidates.Add(containedAndAdjacentThings[i]);
			}
		}
	}

	public static bool CanAttackPrisoner(Pawn pawn, Thing prisoner)
	{
		if (prisoner is Pawn { IsPrisoner: not false, Downed: false } pawn2)
		{
			return pawn2.HostFaction == pawn.Faction;
		}
		return false;
	}
}
