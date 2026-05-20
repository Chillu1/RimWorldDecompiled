using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;

namespace RimWorld;

public abstract class IncidentWorker_Ambush : IncidentWorker
{
	protected abstract List<Pawn> GeneratePawns(IncidentParms parms);

	protected virtual void PostProcessGeneratedPawnsAfterSpawning(List<Pawn> generatedPawns)
	{
	}

	protected virtual LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
	{
		return null;
	}

	protected override bool CanFireNowSub(IncidentParms parms)
	{
		if (parms.target is Map map)
		{
			IntVec3 cell;
			return TryFindEntryCell(map, out cell);
		}
		return CaravanIncidentUtility.CanFireIncidentWhichWantsToGenerateMapAt(parms.target.Tile);
	}

	protected override bool TryExecuteWorker(IncidentParms parms)
	{
		Map map = parms.target as Map;
		IntVec3 existingMapEdgeCell = IntVec3.Invalid;
		if (map != null && !TryFindEntryCell(map, out existingMapEdgeCell))
		{
			return false;
		}
		List<Pawn> generatedEnemies = GeneratePawns(parms);
		if (!generatedEnemies.Any())
		{
			return false;
		}
		if (map != null)
		{
			return DoExecute(parms, generatedEnemies, existingMapEdgeCell);
		}
		LongEventHandler.QueueLongEvent(delegate
		{
			DoExecute(parms, generatedEnemies, existingMapEdgeCell);
		}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
		return true;
	}

	private bool DoExecute(IncidentParms parms, List<Pawn> generatedEnemies, IntVec3 existingMapEdgeCell)
	{
		Map map = parms.target as Map;
		bool flag = false;
		if (map == null)
		{
			map = CaravanIncidentUtility.SetupCaravanAttackMap((Caravan)parms.target, generatedEnemies, sendLetterIfRelatedPawns: false);
			flag = true;
		}
		else
		{
			for (int i = 0; i < generatedEnemies.Count; i++)
			{
				IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(existingMapEdgeCell, map);
				GenSpawn.Spawn(generatedEnemies[i], loc, map, Rot4.Random);
			}
		}
		PostProcessGeneratedPawnsAfterSpawning(generatedEnemies);
		LordJob lordJob = CreateLordJob(generatedEnemies, parms);
		if (lordJob != null)
		{
			LordMaker.MakeNewLord(parms.faction, lordJob, map, generatedEnemies);
		}
		TaggedString letterLabel = GetLetterLabel(generatedEnemies[0], parms);
		TaggedString letterText = GetLetterText(generatedEnemies[0], parms);
		PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(generatedEnemies, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
		SendStandardLetter(letterLabel, letterText, GetLetterDef(generatedEnemies[0], parms), parms, generatedEnemies[0]);
		if (flag)
		{
			Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
		}
		return true;
	}

	private bool TryFindEntryCell(Map map, out IntVec3 cell)
	{
		return CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => x.Standable(map) && map.reachability.CanReachColony(x), map, CellFinder.EdgeRoadChance_Hostile, out cell);
	}

	protected virtual string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
	{
		return def.letterLabel;
	}

	protected virtual string GetLetterText(Pawn anyPawn, IncidentParms parms)
	{
		return def.letterText;
	}

	protected virtual LetterDef GetLetterDef(Pawn anyPawn, IncidentParms parms)
	{
		return def.letterDef;
	}

	protected virtual string GetRelatedPawnsInfoLetterText(IncidentParms parms)
	{
		return "LetterRelatedPawnsGroupGeneric".Translate(Faction.OfPlayer.def.pawnsPlural);
	}
}
