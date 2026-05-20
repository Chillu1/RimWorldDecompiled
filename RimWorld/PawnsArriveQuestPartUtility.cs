using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld;

public static class PawnsArriveQuestPartUtility
{
	public static IEnumerable<Pawn> GetQuestLookTargets(IEnumerable<Pawn> pawns)
	{
		if (pawns.Count() == 1)
		{
			yield return pawns.First();
			yield break;
		}
		foreach (Pawn p in pawns)
		{
			if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
			{
				yield return p;
			}
			if (p.Faction == null && p.Downed)
			{
				yield return p;
			}
		}
	}

	public static bool IncreasesPopulation(IEnumerable<Pawn> pawns, bool joinPlayer, bool makePrisoners)
	{
		foreach (Pawn pawn in pawns)
		{
			if (pawn.RaceProps.Humanlike && !pawn.Destroyed && (pawn.Faction == Faction.OfPlayer || pawn.IsPrisonerOfColony || pawn.Downed || joinPlayer || makePrisoners))
			{
				return true;
			}
		}
		return false;
	}
}
