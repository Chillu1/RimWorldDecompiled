using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet;

public static class DistressCallUtility
{
	private static readonly IntRange BloodFilthToSpawn = new IntRange(1, 5);

	public static void SpawnPawns(Map map, IEnumerable<Pawn> pawns, IntVec3 root, int radius)
	{
		foreach (Pawn pawn in pawns)
		{
			if (!RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => c.Standable(map), map, out var result, radius))
			{
				break;
			}
			GenSpawn.Spawn(pawn, result, map);
		}
	}

	public static void SpawnCorpses(Map map, IEnumerable<Pawn> pawns, IEnumerable<Pawn> killers, IntVec3 root, int radius)
	{
		int num = Find.TickManager.TicksGame - map.Parent.creationGameTicks;
		foreach (Pawn pawn in pawns)
		{
			HealthUtility.SimulateKilledByPawn(pawn, killers.RandomElement());
			Corpse corpse = pawn.Corpse;
			if (corpse == null)
			{
				continue;
			}
			corpse.timeOfDeath = map.Parent.creationGameTicks;
			CompRottable compRottable = corpse.TryGetComp<CompRottable>();
			if (compRottable != null)
			{
				compRottable.RotProgress += num;
			}
			if (!RCellFinder.TryFindRandomCellNearWith(root, (IntVec3 c) => c.Standable(map) && c.GetEdifice(map) == null, map, out var result, radius))
			{
				continue;
			}
			if ((corpse.InnerPawn.kindDef.IsFleshBeast() && compRottable.Stage == RotStage.Dessicated) || corpse.InnerPawn.kindDef == PawnKindDefOf.Fingerspike)
			{
				FilthMaker.TryMakeFilth(result, map, ThingDefOf.Filth_TwistedFlesh);
				break;
			}
			GenSpawn.Spawn(corpse, result, map);
			corpse.SetForbidden(value: true);
			pawn.DropAndForbidEverything();
			if (num >= 300000)
			{
				continue;
			}
			int randomInRange = BloodFilthToSpawn.RandomInRange;
			for (int num2 = 0; num2 < randomInRange; num2++)
			{
				IntVec3 intVec = CellFinder.RandomClosewalkCellNear(result, map, 3);
				if (intVec.InBounds(map) && GenSight.LineOfSight(intVec, result, map))
				{
					FilthMaker.TryMakeFilth(intVec, map, pawn.RaceProps.BloodDef, pawn.LabelIndefinite());
				}
			}
		}
	}
}
