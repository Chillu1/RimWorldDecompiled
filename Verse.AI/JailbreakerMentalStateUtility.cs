using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public static class JailbreakerMentalStateUtility
{
	private static List<Pawn> tmpPrisoners = new List<Pawn>();

	public static Pawn FindPrisoner(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		tmpPrisoners.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn2 = allPawnsSpawned[i];
			if (pawn2.IsPrisoner && pawn2.HostFaction == pawn.Faction && pawn2 != pawn && !pawn2.Downed && !pawn2.InMentalState && !pawn2.IsBurning() && pawn2.Awake() && pawn2.guest.PrisonerIsSecure && PrisonBreakUtility.CanParticipateInPrisonBreak(pawn2) && pawn.CanReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				tmpPrisoners.Add(pawn2);
			}
		}
		if (!tmpPrisoners.Any())
		{
			return null;
		}
		Pawn result = tmpPrisoners.RandomElement();
		tmpPrisoners.Clear();
		return result;
	}
}
