using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld;

public static class IncidentParmsUtility
{
	public static PawnGroupMakerParms GetDefaultPawnGroupMakerParms(PawnGroupKindDef groupKind, IncidentParms parms, bool ensureCanGenerateAtLeastOnePawn = false)
	{
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
		pawnGroupMakerParms.groupKind = groupKind;
		pawnGroupMakerParms.tile = parms.target.Tile;
		pawnGroupMakerParms.points = parms.points;
		pawnGroupMakerParms.faction = parms.faction;
		pawnGroupMakerParms.traderKind = parms.traderKind;
		pawnGroupMakerParms.generateFightersOnly = parms.generateFightersOnly;
		pawnGroupMakerParms.raidStrategy = parms.raidStrategy;
		pawnGroupMakerParms.forceOneDowned = parms.raidForceOneDowned;
		pawnGroupMakerParms.seed = parms.pawnGroupMakerSeed;
		pawnGroupMakerParms.ideo = parms.pawnIdeo;
		pawnGroupMakerParms.raidAgeRestriction = parms.raidAgeRestriction;
		if (ensureCanGenerateAtLeastOnePawn && parms.faction != null)
		{
			pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, parms.faction.def.MinPointsToGeneratePawnGroup(groupKind));
		}
		return pawnGroupMakerParms;
	}

	public static List<List<Pawn>> SplitIntoGroups(List<Pawn> pawns, Dictionary<Pawn, int> groups)
	{
		List<List<Pawn>> list = new List<List<Pawn>>();
		List<Pawn> list2 = pawns.ToList();
		while (list2.Any())
		{
			List<Pawn> list3 = new List<Pawn>();
			Pawn pawn = list2.Last();
			list2.RemoveLast();
			list3.Add(pawn);
			for (int num = list2.Count - 1; num >= 0; num--)
			{
				if (GetGroup(pawn, groups) == GetGroup(list2[num], groups))
				{
					list3.Add(list2[num]);
					list2.RemoveAt(num);
				}
			}
			list.Add(list3);
		}
		return list;
	}

	private static int GetGroup(Pawn pawn, Dictionary<Pawn, int> groups)
	{
		if (groups == null || !groups.TryGetValue(pawn, out var value))
		{
			return -1;
		}
		return value;
	}
}
