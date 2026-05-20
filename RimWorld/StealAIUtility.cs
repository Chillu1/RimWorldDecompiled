using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class StealAIUtility
{
	private const float MinMarketValueToTake = 320f;

	private static readonly FloatRange StealThresholdValuePerCombatPowerRange = new FloatRange(2f, 10f);

	private const float MinCombatPowerPerPawn = 100f;

	private static List<Thing> tmpToSteal = new List<Thing>();

	public static bool TryFindBestItemToSteal(IntVec3 root, Map map, float maxDist, out Thing item, Pawn thief, List<Thing> disallowed = null)
	{
		if (map == null)
		{
			item = null;
			return false;
		}
		if (thief != null && !thief.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
		{
			item = null;
			return false;
		}
		if ((thief != null && !map.reachability.CanReachMapEdge(thief.Position, TraverseParms.For(thief, Danger.Some))) || (thief == null && !map.reachability.CanReachMapEdge(root, TraverseParms.For(TraverseMode.PassDoors, Danger.Some))))
		{
			item = null;
			return false;
		}
		Predicate<Thing> validator = delegate(Thing t)
		{
			if (thief != null && !thief.CanReserve(t))
			{
				return false;
			}
			if (disallowed != null && disallowed.Contains(t))
			{
				return false;
			}
			if (!t.def.stealable)
			{
				return false;
			}
			return !t.IsBurning();
		};
		item = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(root, map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEverOrMinifiable), PathEndMode.ClosestTouch, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Some), maxDist, validator, (Thing x) => GetValue(x), 15, 15);
		if (item != null && GetValue(item) < 320f)
		{
			item = null;
		}
		return item != null;
	}

	public static float TotalMarketValueAround(List<Pawn> pawns)
	{
		float num = 0f;
		tmpToSteal.Clear();
		for (int i = 0; i < pawns.Count; i++)
		{
			if (pawns[i].Spawned && TryFindBestItemToSteal(pawns[i].Position, pawns[i].Map, 7f, out var item, pawns[i], tmpToSteal))
			{
				num += GetValue(item);
				tmpToSteal.Add(item);
			}
		}
		tmpToSteal.Clear();
		return num;
	}

	public static float StartStealingMarketValueThreshold(Lord lord)
	{
		Rand.PushState();
		Rand.Seed = lord.loadID;
		float randomInRange = StealThresholdValuePerCombatPowerRange.RandomInRange;
		Rand.PopState();
		float num = 0f;
		for (int i = 0; i < lord.ownedPawns.Count; i++)
		{
			num += Mathf.Max(lord.ownedPawns[i].kindDef.combatPower, 100f);
		}
		return num * randomInRange;
	}

	public static float GetValue(Thing thing)
	{
		return thing.MarketValue * (float)thing.stackCount;
	}
}
