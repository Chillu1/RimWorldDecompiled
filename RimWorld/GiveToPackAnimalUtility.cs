using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public static class GiveToPackAnimalUtility
{
	public static IEnumerable<Pawn> CarrierCandidatesFor(Pawn pawn)
	{
		IEnumerable<Pawn> source = (pawn.IsFormingCaravan() ? pawn.GetLord().ownedPawns : pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction));
		source = source.Where((Pawn x) => x.RaceProps.packAnimal && !x.inventory.UnloadEverything);
		if (pawn.Map.IsPlayerHome)
		{
			source = source.Where((Pawn x) => x.IsFormingCaravan());
		}
		return source;
	}

	public static Pawn UsablePackAnimalWithTheMostFreeSpace(Pawn pawn)
	{
		IEnumerable<Pawn> enumerable = CarrierCandidatesFor(pawn);
		Pawn pawn2 = null;
		float num = 0f;
		foreach (Pawn item in enumerable)
		{
			if (item.RaceProps.packAnimal && item != pawn && pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
			{
				float num2 = MassUtility.FreeSpace(item);
				if (pawn2 == null || num2 > num)
				{
					pawn2 = item;
					num = num2;
				}
			}
		}
		return pawn2;
	}
}
