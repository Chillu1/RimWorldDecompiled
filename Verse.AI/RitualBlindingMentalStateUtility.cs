using System.Collections.Generic;
using System.Linq;
using RimWorld;

namespace Verse.AI;

public static class RitualBlindingMentalStateUtility
{
	private static List<Pawn> tmpTargets = new List<Pawn>();

	public static Pawn FindPawnToBlind(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		tmpTargets.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn2 = allPawnsSpawned[i];
			if (pawn2 != pawn && (pawn2.Faction == pawn.Faction || (pawn2.IsPrisoner && pawn2.HostFaction == pawn.Faction)) && pawn2.RaceProps.Humanlike && pawn2 != pawn && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly) && (pawn2.CurJob == null || !pawn2.CurJob.exitMapOnArrival) && pawn2.health.hediffSet.GetNotMissingParts().Any((BodyPartRecord x) => x.def == BodyPartDefOf.Eye))
			{
				tmpTargets.Add(pawn2);
			}
		}
		Pawn result = null;
		IEnumerable<Pawn> source = tmpTargets.Where((Pawn x) => x.IsPrisoner);
		if (source.Any())
		{
			result = source.RandomElement();
		}
		else if (tmpTargets.Any())
		{
			result = tmpTargets.RandomElement();
		}
		tmpTargets.Clear();
		return result;
	}
}
