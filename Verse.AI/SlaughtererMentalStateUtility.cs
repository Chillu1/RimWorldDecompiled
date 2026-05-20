using System.Collections.Generic;
using RimWorld;

namespace Verse.AI;

public static class SlaughtererMentalStateUtility
{
	private static List<Pawn> tmpAnimals = new List<Pawn>();

	public static Pawn FindAnimal(Pawn pawn)
	{
		if (!pawn.Spawned)
		{
			return null;
		}
		tmpAnimals.Clear();
		IReadOnlyList<Pawn> allPawnsSpawned = pawn.Map.mapPawns.AllPawnsSpawned;
		for (int i = 0; i < allPawnsSpawned.Count; i++)
		{
			Pawn pawn2 = allPawnsSpawned[i];
			if (pawn2.IsAnimal && pawn2.Faction == pawn.Faction && pawn2 != pawn && !pawn2.IsBurning() && !pawn2.InAggroMentalState && pawn.CanReserveAndReach(pawn2, PathEndMode.Touch, Danger.Deadly))
			{
				tmpAnimals.Add(pawn2);
			}
		}
		if (!tmpAnimals.Any())
		{
			return null;
		}
		Pawn result = tmpAnimals.RandomElement();
		tmpAnimals.Clear();
		return result;
	}
}
