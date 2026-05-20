using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class PawnsArrivalModeWorkerUtility
{
	private const int MaxGroupsCount = 3;

	public static void DropInDropPodsNearSpawnCenter(IncidentParms parms, List<Pawn> pawns)
	{
		Map map = (Map)parms.target;
		bool flag = parms.faction != null && parms.faction.HostileTo(Faction.OfPlayer);
		DropPodUtility.DropThingsNear(parms.spawnCenter, map, pawns.Cast<Thing>(), parms.podOpenDelay, canInstaDropDuringInit: false, leaveSlag: true, flag || parms.raidArrivalModeForQuickMilitaryAid, forbid: true, allowFogged: true, parms.faction);
	}

	public static List<Pair<List<Pawn>, IntVec3>> SplitIntoRandomGroupsNearMapEdge(List<Pawn> pawns, Map map, bool arriveInPods)
	{
		List<Pair<List<Pawn>, IntVec3>> list = new List<Pair<List<Pawn>, IntVec3>>();
		if (!pawns.Any())
		{
			return list;
		}
		int maxGroupsCount = GetMaxGroupsCount(pawns.Count);
		int num = ((maxGroupsCount == 1) ? 1 : Rand.RangeInclusive(2, maxGroupsCount));
		for (int i = 0; i < num; i++)
		{
			IntVec3 second = FindNewMapEdgeGroupCenter(map, list, arriveInPods);
			Pair<List<Pawn>, IntVec3> item = new Pair<List<Pawn>, IntVec3>(new List<Pawn>(), second);
			item.First.Add(pawns[i]);
			list.Add(item);
		}
		for (int j = num; j < pawns.Count; j++)
		{
			list.RandomElement().First.Add(pawns[j]);
		}
		return list;
	}

	public static IntVec3 FindNewMapEdgeGroupCenter(Map map, List<Pair<List<Pawn>, IntVec3>> groups, bool arriveInPods)
	{
		IntVec3 result = IntVec3.Invalid;
		float num = 0f;
		for (int i = 0; i < 4; i++)
		{
			IntVec3 result2;
			if (arriveInPods)
			{
				result2 = DropCellFinder.FindRaidDropCenterDistant(map);
			}
			else if (!RCellFinder.TryFindRandomPawnEntryCell(out result2, map, CellFinder.EdgeRoadChance_Hostile))
			{
				result2 = DropCellFinder.FindRaidDropCenterDistant(map);
			}
			if (!groups.Any())
			{
				result = result2;
				break;
			}
			float num2 = float.MaxValue;
			for (int j = 0; j < groups.Count; j++)
			{
				float num3 = result2.DistanceToSquared(groups[j].Second);
				if (num3 < num2)
				{
					num2 = num3;
				}
			}
			if (!result.IsValid || num2 > num)
			{
				num = num2;
				result = result2;
			}
		}
		return result;
	}

	private static int GetMaxGroupsCount(int pawnsCount)
	{
		if (pawnsCount <= 1)
		{
			return 1;
		}
		return Mathf.Clamp(pawnsCount / 2, 2, 3);
	}

	public static void SetPawnGroupsInfo(IncidentParms parms, List<Pair<List<Pawn>, IntVec3>> groups)
	{
		parms.pawnGroups = new Dictionary<Pawn, int>();
		for (int i = 0; i < groups.Count; i++)
		{
			for (int j = 0; j < groups[i].First.Count; j++)
			{
				parms.pawnGroups.Add(groups[i].First[j], i);
			}
		}
	}
}
