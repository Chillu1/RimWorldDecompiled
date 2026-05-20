using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld;

public class GenStep_SleepingMechanoids : GenStep
{
	public FloatRange defaultPointsRange = new FloatRange(340f, 1000f);

	public override int SeedPart => 341176078;

	public static void SendMechanoidsToSleepImmediately(List<Pawn> spawnedMechanoids)
	{
		for (int i = 0; i < spawnedMechanoids.Count; i++)
		{
			spawnedMechanoids[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
			JobDriver curDriver = spawnedMechanoids[i].jobs.curDriver;
			if (curDriver != null)
			{
				curDriver.asleep = true;
			}
			CompCanBeDormant comp = spawnedMechanoids[i].GetComp<CompCanBeDormant>();
			if (comp != null)
			{
				comp.ToSleep();
			}
			else
			{
				Log.ErrorOnce("Tried spawning sleeping mechanoid " + spawnedMechanoids[i]?.ToString() + " without CompCanBeDormant!", 0x12EA9A79 ^ spawnedMechanoids[i].def.defName.GetHashCode());
			}
		}
	}

	public override void Generate(Map map, GenStepParams parms)
	{
		if (!SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out var rectToDefend, out var singleCellToSpawnNear, map))
		{
			return;
		}
		List<Pawn> list = new List<Pawn>();
		foreach (Pawn item in GeneratePawns(parms, map))
		{
			if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(rectToDefend, singleCellToSpawnNear, map, out var spawnCell))
			{
				Find.WorldPawns.PassToWorld(item);
				break;
			}
			GenSpawn.Spawn(item, spawnCell, map);
			list.Add(item);
		}
		if (!list.Any())
		{
			return;
		}
		bool wakeUpIfTargetClose = Rand.Bool;
		foreach (Pawn item2 in list)
		{
			CompWakeUpDormant comp = item2.GetComp<CompWakeUpDormant>();
			if (comp != null)
			{
				comp.wakeUpIfTargetClose = wakeUpIfTargetClose;
			}
		}
		LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenAssaultColony(Faction.OfMechanoids), map, list);
		SendMechanoidsToSleepImmediately(list);
	}

	private IEnumerable<Pawn> GeneratePawns(GenStepParams parms, Map map)
	{
		float points = ((parms.sitePart != null) ? parms.sitePart.parms.threatPoints : defaultPointsRange.RandomInRange);
		PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
		{
			groupKind = PawnGroupKindDefOf.Combat,
			tile = map.Tile,
			faction = Faction.OfMechanoids,
			points = points
		};
		if (parms.sitePart != null)
		{
			pawnGroupMakerParms.seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.sitePart.parms);
		}
		return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
	}
}
